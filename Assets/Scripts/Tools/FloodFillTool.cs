using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Tools
{
    [CreateAssetMenu(fileName = "FloodFillTool", menuName = "FloodFillTool", order = 0)]
    public class FloodFillTool : Tool
    {
        public override bool Down(RenderTexture targetTexture, List<PaintParameters> parameters)
        {
            FloodFill(targetTexture, Vector2Int.FloorToInt(parameters[0].Position), parameters[0].Color);
			return true;
        }

        private void FloodFill(RenderTexture renderTexture, Vector2Int position, Color color)
        {
            RenderTexture.active = renderTexture;

            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            FloodFill(texture, position, color);
            Graphics.Blit(texture, renderTexture);
            
            RenderTexture.active = null;
        }

        private void FloodFill(Texture2D texture, Vector2Int position, Color color)
        {
            var pixels = texture.GetPixels32();
            FloodFill(pixels, texture.width, texture.height, position, color);
            texture.SetPixels32(pixels);
            texture.Apply(false);
        }

        private bool EqualColors(Color32 a, Color32 b)
        {
            var sumA = a.r + a.g + a.b;
            var sumB = b.r + b.g + b.b;
            return Mathf.Abs(sumA - sumB) < 32;
        }

        private void FloodFill(Color32[] pixels, int width, int height, Vector2Int startPosition, Color32 color)
        {
            var startIndex = startPosition.x + startPosition.y * width;
            var startColor = pixels[startIndex];
            var r = (byte)Mathf.Min(color.r, startColor.r);
            var g = (byte)Mathf.Min(color.g, startColor.g);
            var b = (byte)Mathf.Min(color.b, startColor.b);
            color = new Color32(r, g, b, color.a);
            
            if (EqualColors(startColor, color))
            {
                return;
            }

            var toProcess = new Stack<int>();
            toProcess.Push(startIndex);

            while (toProcess.Count > 0)
            {
                var index = toProcess.Pop();
                if (!EqualColors(pixels[index], startColor))
                {
                    continue;
                }

                var x = index % width;
                var y = index / width;

                int minX = ScanLeft(x, y);
                int maxX = ScanRight(x, y);
                AddCellsToProcessList(minX, maxX, y - 1);
                AddCellsToProcessList(minX, maxX, y + 1);
            }

            int ScanLeft(int x, int y)
            {
                int minX = x;
                while (minX >= 0)
                {
                    var current = minX + y * width;
                    if (!EqualColors(pixels[current], startColor))
                    {
                        break;
                    }

                    pixels[current] = color;
                    minX -= 1;
                }

                return minX;
            }

            int ScanRight(int x, int y)
            {
                int maxX = x + 1;
                while (maxX < width)
                {
                    var current = maxX + y * width;
                    if (!EqualColors(pixels[current], startColor))
                    {
                        break;
                    }

                    pixels[current] = color;
                    maxX += 1;
                }

                return maxX;
            }

            void AddCellsToProcessList(int minX, int maxX, int y)
            {
                if (y < 0 || y >= height)
                {
                    return;
                }

                for (int cx = minX + 1; cx < maxX; cx++)
                {
                    var current = cx + y * width;
                    if (!EqualColors(pixels[current], startColor))
                    {
                        continue;
                    }

                    toProcess.Push(current);
                }
            }
        }
    }
}