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

        public override void Down(RenderTexture targetTexture, PaintParameters parameters, LayerManager layers)
        {
            var sourceTexture = layers.TopMostLayer.RenderTexture;
            
            var width = sourceTexture.width;
            var height = sourceTexture.height;
            var texture = new Texture2D(width, height);
            sourceTexture.CopyToTexture(texture);
            
            var sourcePixels = texture.GetPixels32();
            var maskPixels = new Color32[sourcePixels.Length];
            var sourcePosition = parameters.Position;
            sourcePosition.Scale(new Vector2(
                (float) sourceTexture.width / targetTexture.width,
                (float) sourceTexture.height / targetTexture.height));
            var intPosition = Vector2Int.FloorToInt(sourcePosition);
            FloodFill(sourcePixels, maskPixels, width, height, intPosition, parameters.Color);
            // TODO: Make optional:
            Dilate(maskPixels, width, height);
            texture.SetPixels32(maskPixels);
            texture.Apply(false);

            BrushMaterial.color = parameters.Color;

            var renderTexture = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(texture, renderTexture);
            //PaintUtils.GaussianBlur(renderTexture, 5f);

            Graphics.Blit(renderTexture, targetTexture, BrushMaterial);
            RenderTexture.ReleaseTemporary(renderTexture);
            Destroy(texture);
        }

        private bool EqualColors(Color32 a, Color32 b)
        {
            var sumA = a.r + a.g + a.b + a.a;
            var sumB = b.r + b.g + b.b + b.a;
            return Mathf.Abs(sumA - sumB) <= Threshold * 4;
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

        private void Dilate(Color32[] pixels, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;
                    if (pixels[index].r == 0)
                    {
                        continue;
                    }

                    if (x > 0 && pixels[index - 1].r == 0)
                    {
                        pixels[index - 1].g = 255;
                    }
                    if (x < width - 1 && pixels[index + 1].r == 0)
                    {
                        pixels[index + 1].g = 255;
                    }
                    if (y > 0 && pixels[index - width].r == 0)
                    {
                        pixels[index - width].g = 255;
                    }
                    if (y < height - 1 && pixels[index + width].r == 0)
                    {
                        pixels[index + width].g = 255;
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;
                    if (pixels[index].g == 255)
                    {
                        pixels[index] = Color.white;
                    }
                }
            }
        }
    }
}