using System.Collections.Generic;
using UnityEngine;

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
        
        public virtual bool Down(RenderTexture targetTexture, List<PaintParameters> parameters)
        {
            return false;
        }

        public virtual bool Up(RenderTexture targetTexture, List<PaintParameters> parameters)
        {
            return false;
        }

        public virtual bool Move(RenderTexture targetTexture, List<PaintParameters> parameters)
        {
            return false;
        }
    }
}