using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utilities
{
	public static class PaintUtils
	{
		public const int ThumbnailWidth = 512;
		public const int ThumbnailHeight = ThumbnailWidth * 3 / 4;
		
		public static IEnumerable<float> DrawLine(Vector2 start, Vector2 end, float stepLength)
		{
			float dist = Vector2.Distance(start, end);
			float fStepCount = dist / stepLength;
			int stepCount = Mathf.FloorToInt(fStepCount);
			float position = 0f;
    
			//yield return position;
        	
			for (int i = 1; i <= stepCount + 1; i++)
			{
				yield return position;
				position = i / fStepCount;
			}
		}

		public static Vector2 PointOnQuadraticCurve(Vector2 start, Vector2 control, Vector2 end, float a) {
			float f1 = (1 - a) * (1 - a);
			float f2 = 2 * a * (1 - a);
			float f3 = a * a;
            
			return start * f1 + control * f2 + end * f3;
		}
	
		public static void CopyToTexture(this RenderTexture renderTexture, Texture2D target)
		{
			RenderTexture.active = renderTexture;
			target.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
			target.Apply(false);
			RenderTexture.active = null;
		}
		
		public static void Clear(this RenderTexture renderTexture, Color color)
		{
			RenderTexture.active = renderTexture;
			GL.Clear(false, true, color);
			RenderTexture.active = null;
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

		public static void SaveImageTexture(string filename, Texture2D texture)
		{
			var path = $"{Application.persistentDataPath}/{filename}";
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(path, bytes);
		}

		// public static void SaveImageThumbnail(string filename, Texture texture)
		// {
		// 	var path = $"{Application.persistentDataPath}/{filename}";
		// 	path = Path.ChangeExtension(path, ".thumb.png");
		// 	var thumbnail = GetScaledTexture(texture, ThumbnailWidth, ThumbnailHeight);
		// 	var bytes = thumbnail.EncodeToPNG();
		// 	File.WriteAllBytes(path, bytes);
		// 	Object.Destroy(thumbnail);
		// }
		//
		// public static Texture2D GetScaledTexture(Texture texture, int width, int height)
		// {
		// 	var renderTexture = RenderTexture.GetTemporary(width, height, 0);
		// 	renderTexture.filterMode = FilterMode.Trilinear;
		// 	Graphics.Blit(texture, renderTexture);
		// 	var resizedTexture = renderTexture.CopyToTexture();
		// 	RenderTexture.ReleaseTemporary(renderTexture);
		// 	return resizedTexture;
		// }
		
		public static void GaussianBlur(RenderTexture renderTexture, float radius = 4f)
		{
			var blurMaterial = new Material(Shader.Find("Paint/Blur"));
			blurMaterial.SetFloat("_Radius", radius);
			var buffer = RenderTexture.GetTemporary(renderTexture.descriptor);
			Graphics.Blit(renderTexture, buffer, blurMaterial, 0);
			Graphics.Blit(buffer, renderTexture, blurMaterial, 1);
			RenderTexture.ReleaseTemporary(buffer);
		}
	}
}