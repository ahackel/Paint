using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "ImageBookConfig", menuName = "ImageBookConfig", order = 0)]
    public class ImageBookConfig : ScriptableObject
    {
        [Serializable]
        public class ImageDocumentConfig
        {
            public string Name;
            public Texture2D Overlay;
        }

        public List<ImageDocumentConfig> Documents;
    }
}