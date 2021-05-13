using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utilities
{
	public static class PaintUtils
	{
		public static IEnumerable<float> DrawLine(Vector2 start, Vector2 end, float stepLength)
		{
			float dist = Vector2.Distance(start, end);
			int stepCount = Mathf.FloorToInt(dist / stepLength);
			float position = 0f;
    
			yield return position;
        	
			for (int i = 1; i < stepCount; i++)
			{
				yield return position;
				position = (float)i / stepCount;
			}
    
			// Leave last point to avoid overlapping:
			//yield return 1f;
		}

		public static Vector2 PointOnQuadraticCurve(Vector2 start, Vector2 control, Vector2 end, float a) {
			float f1 = (1 - a) * (1 - a);
			float f2 = 2 * a * (1 - a);
			float f3 = a * a;
            
			return start * f1 + control * f2 + end * f3;
		}
	
		public static Texture2D CaptureRenderTexture(this RenderTexture renderTexture)
		{
			var width = renderTexture.width;
			var height = renderTexture.height;
			var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
			RenderTexture.active = renderTexture;
			texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			texture.Apply(false);
			RenderTexture.active = null;
			return texture;
		}

		public static Texture2D LoadImageTexture(string filename)
		{
			try
			{
				var path = $"{Application.persistentDataPath}/{filename}";
				var bytes = File.ReadAllBytes(path);
				var texture = new Texture2D(1, 1);
				texture.LoadImage(bytes);
				return texture;
			}
			catch
			{
				return null;
			}
		}
	}
}