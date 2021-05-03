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
        
        public event Action Clicked;
        public event Action DoubleClicked;
        

        private const string USSClassName = "tool-button";
        private const string USSClassNameImage = "tool-button-image";

        private VisualElement _image;
        private bool _selected;
        private long _lastClickTime;

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                EnableInClassList("selected", _selected);
            }
        }

        
        public ToolButton()
        {
            AddToClassList(USSClassName);
            
            _image = new VisualElement { name = "tool-button-image", pickingMode = PickingMode.Ignore };
            _image.AddToClassList(USSClassNameImage);
            Add(_image);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.timestamp - _lastClickTime < 400f && DoubleClicked != null)
            {
                DoubleClicked?.Invoke();
            }
            else
            {
                Clicked?.Invoke();
            }
            _lastClickTime = evt.timestamp;
        }
    }
}