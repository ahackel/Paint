using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "BrushSetSet", menuName = "BrushSiseSet", order = 0)]
    public class BrushSizeSet : ScriptableObject
    {
        public List<float> BrushSizes;
    }
}