using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls
{
    public class ColorPalette : BasePopupPalette<Color>
    {
        public new static readonly string ussClassName = "color-palette";
        
        public new class UxmlFactory : UxmlFactory<ColorPalette, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
        
        public ColorPalette(List<Color> choices) : base(choices)
        {
            AddToClassList(ussClassName);
        }

        public ColorPalette() : this(new List<Color>(0))
        {
        }

        protected override void SetupChoice(VisualElement element, Color choice, int index)
        {
            element.style.backgroundColor = choice;
        }
    }
}