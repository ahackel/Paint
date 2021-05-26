using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

namespace Tools
{
    [CreateAssetMenu(fileName = "FloodFillTool", menuName = "FloodFillTool", order = 0)]
    public class FloodFillTool : Tool
    {
        [Header("Brush")]
        public Material BrushMaterial;
        [Range(0, 255)]
        public int Threshold = 0;

        public override void Down(RenderTexture targetTexture, PaintParameters parameters)
        {
            var width = targetTexture.width;
            var height = targetTexture.height;
            var texture = targetTexture.CaptureRenderTexture();
            
            var sourcePixels = texture.GetPixels32();
            var maskPixels = new Color32[sourcePixels.Length];
            var intPosition = Vector2Int.FloorToInt(parameters.Position);
            FloodFill(sourcePixels, maskPixels, width, height, intPosition, parameters.Color);
            texture.SetPixels32(maskPixels);
            texture.Apply(false);

            BrushMaterial.color = parameters.Color;
            Graphics.Blit(texture, targetTexture, BrushMaterial);
        }

        private bool EqualColors(Color32 a, Color32 b)
        {
            var sumA = a.r + a.g + a.b;
            var sumB = b.r + b.g + b.b;
            return Mathf.Abs(sumA - sumB) <= Threshold * 3;
        }

        private void FloodFill(Color32[] sourcePixels, Color32[] maskPixels,
            int width, int height, Vector2Int startPosition, Color32 color)
        {
            var startIndex = startPosition.x + startPosition.y * width;
            var startColor = sourcePixels[startIndex];
            // var r = (byte)Mathf.Min(color.r, startColor.r);
            // var g = (byte)Mathf.Min(color.g, startColor.g);
            // var b = (byte)Mathf.Min(color.b, startColor.b);
            // color = new Color32(r, g, b, color.a);
            
            if (EqualColors(startColor, color))
            {
                return;
            }

            var toProcess = new Stack<int>();
            toProcess.Push(startIndex);

            while (toProcess.Count > 0)
            {
                var index = toProcess.Pop();
                if (maskPixels[index].a != 0 || !EqualColors(sourcePixels[index], startColor))
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
                    if (!EqualColors(sourcePixels[current], startColor))
                    {
                        break;
                    }

                    maskPixels[current] = Color.white;
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
                    if (!EqualColors(sourcePixels[current], startColor))
                    {
                        break;
                    }

                    maskPixels[current] = Color.white;
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
                    if (!EqualColors(sourcePixels[current], startColor))
                    {
                        continue;
                    }

                    toProcess.Push(current);
                }
            }
        }
    }
}