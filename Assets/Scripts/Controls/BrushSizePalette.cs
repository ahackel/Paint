using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls
{
    public class BrushSizePalette : BasePopupPalette<float>
    {
        public new static readonly string ussClassName = "brush-size-palette";
        
        public new class UxmlFactory : UxmlFactory<BrushSizePalette, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
        
        public BrushSizePalette(List<float> choices) : base(choices)
        {
            AddToClassList(ussClassName);
        }

        public BrushSizePalette() : this(new List<float>(0))
        {
        }

        protected override void SetupChoice(VisualElement element, float choice, int index)
        {
            var borderWidth = 0.5f * (60f - choice);
            element.style.borderLeftWidth = borderWidth;
            element.style.borderRightWidth = borderWidth;
            element.style.borderTopWidth = borderWidth;
            element.style.borderBottomWidth = borderWidth;

            // var borderRadius = 0.5f * choice;
            // element.style.borderTopLeftRadius = borderRadius;
            // element.style.borderTopRightRadius = borderRadius;
            // element.style.borderBottomLeftRadius = borderRadius;
            // element.style.borderBottomRightRadius = borderRadius;
        }
    }
}