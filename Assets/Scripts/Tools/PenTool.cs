using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
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
        public float StrokeSpeedInfluence = 0f;
        [Range(0f, 1f)]
        public float PenPressureInfluence = 0.3f;
        [Range(1f, 10f)]
        private const float MaxBrushScaleFromTilt = 4f;

        [Header("WaterColor")]
        public bool UseWaterColor;
        public Material WaterColorProcessingMaterial;
        public Material BlurMaterial;
        [Range(0f, 1f)]
        public float Wetness = 0.3f;


        private List<PaintParameters> _lastParameters = new List<PaintParameters>();
        private RenderTexture _waterColorBuffer;
        
        private static readonly int WetBufferPropId = Shader.PropertyToID("_WetBuffer");

        public override void Down(RenderTexture targetTexture, PaintParameters parameters, LayerManager layers)
        {
	        _waterColorBuffer = RenderTexture.GetTemporary(targetTexture.width, targetTexture.height, 0);
	        Graphics.Blit(layers.CurrentLayer.RenderTexture, _waterColorBuffer, WaterColorProcessingMaterial, 0);
        }

        public override void Up(RenderTexture targetTexture, PaintParameters parameters)
        {
	        RenderTexture.ReleaseTemporary(_waterColorBuffer);
			_lastParameters.Clear();
        }

        public override void Move(RenderTexture targetTexture, PaintParameters parameters, Texture2D currentState)
        {
	        // wear off 
	        // if (_lastParameters.Count > 0)
	        // {
		       //  var color = currentState.GetPixel(Mathf.FloorToInt(parameters.Position.x), Mathf.FloorToInt(parameters.Position.y));
		       //  parameters.Color = Color.Lerp(_lastParameters[0].Color, color, 0.01f);
	        // }

	        if (!UseWaterColor)
	        {
		        DrawStroke(targetTexture, parameters);
		        return;
	        }

	        var tempTexture = RenderTexture.GetTemporary(_waterColorBuffer.descriptor);
	        var blurredTexture = RenderTexture.GetTemporary(_waterColorBuffer.descriptor);
	        var strokeTexture = RenderTexture.GetTemporary(targetTexture.descriptor);

	        // stroke
	        DrawStroke(strokeTexture, parameters);
	        WaterColorProcessingMaterial.SetTexture("_StrokeTex", strokeTexture);
	        //WaterColorProcessingMaterial.SetFloat("_Wetness", Wetness);
	        
	        // blur
	        Graphics.Blit(_waterColorBuffer, tempTexture, BlurMaterial, 0);
	        Graphics.Blit(tempTexture, blurredTexture, BlurMaterial, 1);
	        WaterColorProcessingMaterial.SetTexture("_BlurredTex", blurredTexture);

	        // process
	        Graphics.Blit(_waterColorBuffer, tempTexture, WaterColorProcessingMaterial, 1);
	        Graphics.Blit(tempTexture, _waterColorBuffer);
	        Graphics.Blit(tempTexture, targetTexture, WaterColorProcessingMaterial, 2);
	        
	        RenderTexture.ReleaseTemporary(strokeTexture);
	        RenderTexture.ReleaseTemporary(tempTexture);
	        RenderTexture.ReleaseTemporary(blurredTexture);
        }

        private void DrawStroke(RenderTexture targetTexture, PaintParameters current)
        {
	        var previous = _lastParameters.Count > 0 ? _lastParameters[0] : current;
	        var last = _lastParameters.Count > 1 ? _lastParameters[1] : previous;
	        bool hasMoved = _lastParameters.Count > 0;

	        var actualSpeed = Vector2.Distance(current.Position, previous.Position) / (Time.deltaTime * 1000f);
	        // Smooth speed:
	        current.Speed = Mathf.Lerp(previous.Speed, actualSpeed, 0.2f);

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

		        var pressure = Mathf.Lerp(startPressure, endPressure, t);
		        var speed = Mathf.Lerp(previous.Speed, current.Speed, t);
		        var size = brushSize
		                   * Mathf.Lerp(1f, pressure, PenPressureInfluence)
		                   * (1f + Random.Range(-1f, 1f) * SizeJitter)
		                   * Mathf.Lerp(1f, 1f / (1f + speed), StrokeSpeedInfluence);

		        var offset = normal * (Random.Range(-1f, 1f) * size * Scatter);
		        var offsetPos = pos + offset;

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

	        current.Position = pos;
	        _lastParameters.Insert(0, current);

	        GL.PopMatrix();
	        RenderTexture.active = null;
        }
    }
}