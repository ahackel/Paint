using System;
using UnityEngine.UIElements;

namespace Controls
{
    public class ToolButton : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ToolButton, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlBoolAttributeDescription _selected = new UxmlBoolAttributeDescription { name = "selected" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((ToolButton)ve).Selected = _selected.GetValueFromBag(bag, cc);
            }
        }

        private const string USSClassName = "tool-button";
        private const string USSClassNameImage = "tool-button-image";

        private VisualElement _image;
        private bool _selected;
        private Clickable _clickable;

        public Clickable Clickable
        {
            get => _clickable;
            set
            {
                if (_clickable != null && _clickable.target == this)
                {
                    this.RemoveManipulator(_clickable);
                }

                _clickable = value;

                if (_clickable != null)
                {
                    this.AddManipulator(_clickable);
                }
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                EnableInClassList("selected", _selected);
            }
        }
        
        public event Action Clicked
        {
            add
            {
                if (_clickable == null)
                {
                    Clickable = new Clickable(value);
                }
                else
                {
                    _clickable.clicked += value;
                }
            }
            remove
            {
                if (_clickable != null)
                {
                    _clickable.clicked -= value;
                }
            }
        }
        
        public ToolButton()
        {
            AddToClassList(USSClassName);
            
            _image = new VisualElement { name = "tool-button-image", pickingMode = PickingMode.Ignore };
            _image.AddToClassList(USSClassNameImage);
            Add(_image);
        }
    }
}