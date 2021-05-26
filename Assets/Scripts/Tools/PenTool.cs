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
	    public Texture2D BrushTexture;
	    [Range(0f, 1f)]
	    public float Flow = 1f;
	    [Range(0f, 1f)]
	    public float BrushHardness = 1f;
        [Range(0f, 360f)]
        public float Angle = 0f;
        [Range(0.01f, 1f)]
        public float Roundness = 1f;
        [Range(0.01f, 3f)]
        public float BrushSpacing = 0.01f;

        [Header("Jitter")]
        [Range(0f, 1f)]
        public float Scatter = 0f;
        [Range(0f, 1f)]
        public float AngleJitter = 0f;
        [Range(0f, 1f)]
        public float RoundnessJitter = 0f;
        [Range(0f, 1f)]
        public float FlowJitter = 0f;
        [Range(0f, 1f)]
        public float SizeJitter = 0f;

        [Header("Pen")]
        [Range(0f, 1f)]
        public float PenPressureInfluence = 0.3f;
        [Range(1f, 10f)]
        private const float MaxBrushScaleFromTilt = 4f;

        private List<PaintParameters> _lastParameters = new List<PaintParameters>();

        public override void Up(RenderTexture targetTexture, PaintParameters parameters)
        {
			_lastParameters.Clear();
        }

        public override void Move(RenderTexture targetTexture, PaintParameters parameters)
        {
	        var current = parameters;
	        var previous = _lastParameters.Count > 0 ? _lastParameters[0] : current;
	        var last = _lastParameters.Count > 1 ? _lastParameters[1] : previous;
	        bool hasMoved = _lastParameters.Count > 0;

	        var brushSize = current.BrushSize;
	        float angle = Angle;
			float roundness = Roundness;
			if (BrushTexture != null)
			{
				BrushMaterial.EnableKeyword("USE_TEXTURE");
			}
			else
			{
				BrushMaterial.DisableKeyword("USE_TEXTURE");
			}
			BrushMaterial.SetFloat("_Hardness", BrushHardness);
			var brushTexture = BrushTexture != null ? BrushTexture : Texture2D.whiteTexture;
			
			if (current.IsPen)
			{
				var tilt = current.Tilt;
				var azimuth = Mathf.Atan2(tilt.y, tilt.x);
				var length = Mathf.Clamp01(tilt.magnitude);
				var altitude = Mathf.Atan2(1f - length, length);
				angle = azimuth * Mathf.Rad2Deg;
				var brushAngle = 1f - altitude / (Mathf.PI * 0.5f);
				// scale brush none linearly:
				roundness = 1f + Mathf.Pow(brushAngle, 4f) * MaxBrushScaleFromTilt;
			}

			var startPosition = previous.Position;
			var endPosition = current.Position;
			var startPressure = previous.Pressure;
			var endPressure = current.Pressure;
			var distance = Vector2.Distance(endPosition, startPosition);

			var absoluteBrushDistance = Mathf.Max(1f, brushSize * BrushSpacing);
			if (hasMoved && distance < absoluteBrushDistance)
			{
				return;
			}
			
			var tangent = (startPosition - last.Position).normalized;
			var normal = new Vector2(tangent.y, -tangent.x);
			var control = startPosition + tangent * (distance * 0.3f);

			RenderTexture.active = targetTexture;
			GL.PushMatrix();
			GL.LoadPixelMatrix(0, targetTexture.width, 0, targetTexture.height);

			Vector2 pos = endPosition;

			foreach (float t in PaintUtils.DrawLine(startPosition, endPosition, absoluteBrushDistance))
			{
				if (hasMoved && t == 0f)
				{
					continue;
				}
				
				pos = QuadraticInterpolation
					? PaintUtils.PointOnQuadraticCurve(startPosition, control, endPosition, t)
					: Vector2.LerpUnclamped(startPosition, endPosition, t);

				var offset = normal * (Random.Range(-1f, 1f) * brushSize * Scatter);
				var offsetPos = pos + offset;
				
				var pressure = Mathf.Lerp(startPressure, endPressure, t);
				var size = brushSize
				           * (Mathf.Lerp(1f, pressure, PenPressureInfluence) + Random.Range(-1f, 1f) * SizeJitter);
				var rect = new Rect(
					offsetPos.x - 0.5f * size,
					offsetPos.y - 0.5f * size,
					size, size);

				angle = Angle + Mathf.Lerp(0f, Random.Range(-180f, 180f), AngleJitter);
				var localBrushRotation = Quaternion.Euler(0, 0, -angle);
				roundness = Mathf.Clamp01(Roundness + Random.Range(-1f, 1f) * RoundnessJitter);
				
				var matrix = Matrix4x4.TRS(offsetPos, localBrushRotation,
					new Vector3(roundness, 1f, 1f)) * Matrix4x4.TRS(-offsetPos, Quaternion.identity, Vector3.one);
				GL.MultMatrix(matrix);
				
				var color = UseOwnColor ? Color : current.Color;
				color.a = Mathf.Clamp01(Flow + Random.Range(-1f, 1f) * FlowJitter);
				BrushMaterial.color = color;
				Graphics.DrawTexture(rect, brushTexture, BrushMaterial);
			}

			if (_lastParameters.Count > 1)
			{
				_lastParameters.RemoveAt(1);
			}

			parameters.Position = pos;
			_lastParameters.Insert(0, parameters);

			GL.PopMatrix();
			RenderTexture.active = null;
        }
    }
}