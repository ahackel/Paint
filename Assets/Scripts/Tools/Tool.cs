using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Tools
{
    public class Tool : ScriptableObject
    {
        public struct PaintParameters
        {
            public bool IsPen;
            public Vector2 Position;
            public float Pressure;
            public Vector2 Tilt;
            public Color Color;
            public float BrushSize;
        }

        [Range(0f, 1f)]
        public float Opacity = 1f;
        
        public virtual void Down(RenderTexture targetTexture, PaintParameters parameters, LayerManager layers)
        {
        }

        public virtual void Up(RenderTexture targetTexture, PaintParameters parameters)
        {
        }

        public virtual void Move(RenderTexture targetTexture, PaintParameters parameters)
        {
        }
    }
}