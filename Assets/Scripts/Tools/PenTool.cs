using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Tools
{
    [CreateAssetMenu(fileName = "PenTool", menuName = "PenTool", order = 0)]
    public class PenTool : Tool
    {
	    public Color Color;
	    public bool UseOwnColor;
	    public bool QuadraticInterpolation;

	    [Header("Brush")]
	    public Material BrushMaterial;
        [Range(0f, 360f)]
        public float BrushRotation = 0f;
        [Range(1f, 4f)]
        public float BrushScale = 1f;
        [Range(1, 100)]
        public float BrushSize = 32f;
        [Range(0.01f, 3f)]
        public float BrushSpacing = 0.01f;

        [Header("Pen")]
        [Range(0f, 1f)]
        public float PenPressureInfluence = 0.3f;
        [Range(1f, 10f)]
        private const float MaxBrushScaleFromTilt = 4f;

        public override bool Move(RenderTexture targetTexture, List<PaintParameters> parameters)
        {
	        var current = parameters[0];
	        var previous = parameters.Count > 1 ? parameters[1] : current;
	        var last = parameters.Count > 2 ? parameters[2] : previous;

	        var brushSize = current.BrushSize;
	        float brushRotation = BrushRotation;
			float brushScale = BrushScale;
			
			if (current.IsPen)
			{
				var tilt = current.Tilt;
				var azimuth = Mathf.Atan2(tilt.y, tilt.x);
				var length = Mathf.Clamp01(tilt.magnitude);
				var altitude = Mathf.Atan2(1f - length, length);
				brushRotation = azimuth * Mathf.Rad2Deg;
				var brushAngle = 1f - altitude / (Mathf.PI * 0.5f);
				// scale brush none linearly:
				brushScale = 1f + Mathf.Pow(brushAngle, 4f) * MaxBrushScaleFromTilt;
			}

			var startPosition = previous.Position;
			var endPosition = current.Position;
			var startPressure = previous.Pressure;
			var endPressure = current.Pressure;
			var color = UseOwnColor ? Color : current.Color;
			BrushMaterial.color = color;
			var distance = Vector2.Distance(endPosition, startPosition);

			var trueBrushDistance = Mathf.Max(1f, brushSize * BrushSpacing);
			if (distance > 0 && distance < trueBrushDistance)
			{
				return false;
			}
			
			var tangent = (startPosition - last.Position).normalized;
			var control = startPosition + tangent * (distance * 0.3f);

			RenderTexture.active = targetTexture;
			GL.PushMatrix();
			GL.LoadPixelMatrix(0, targetTexture.width, 0, targetTexture.height);

			foreach (float t in PaintUtils.DrawLine(startPosition, endPosition, trueBrushDistance))
			{
				var pos = QuadraticInterpolation
					? PaintUtils.PointOnQuadraticCurve(startPosition, control, endPosition, t)
					: Vector2.Lerp(startPosition, endPosition, t);
				var pressure = Mathf.Lerp(startPressure, endPressure, t);
				var size = brushSize * Mathf.Lerp(1f, pressure, PenPressureInfluence);
				var rect = new Rect(
					pos.x - 0.5f * size,
					pos.y - 0.5f * size,
					size, size);

				var matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, -brushRotation), new Vector3(brushScale, 1f, 1f)) * Matrix4x4.TRS(-pos, Quaternion.identity, Vector3.one);  ;
				GL.MultMatrix(matrix);
				Graphics.DrawTexture(rect, BrushMaterial.mainTexture, BrushMaterial);
			}

			GL.PopMatrix();
			RenderTexture.active = null;
			return true;
        }
    }
}