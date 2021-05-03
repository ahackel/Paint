using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls
{
    public abstract class BasePopupPalette<T> : VisualElement
    {
        public static readonly string ussClassName = "popup-palette";
        public static readonly string panelUssClassName = ussClassName + "-panel";
        public static readonly string optionUssClassName = ussClassName + "-option";
        public static readonly string arrowUssClassName = ussClassName + "-arrow";

        private VisualElement _panel;
        private VisualElement _selectedChoiceElement;
        private readonly VisualElement _panelContainer;
        private List<T> _choices;
        private int _index;
        private List<VisualElement> _choiceElements = new List<VisualElement>();
        private T _value;

        public List<T> Choices
        {
            get => _choices;
            set
            {
                _choices = value;
                if (_index < _choices.Count)
                {
                    SetValueWithoutNotify(_choices[_index]);
                }
            }
        }

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                Value = _choices[_index];
            }
        }

        public T Value
        {
            get => _value;
            set
            {
                if (panel != null)
                {
                    using (ChangeEvent<T> evt = ChangeEvent<T>.GetPooled(_value, value))
                    {
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                    }
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
            }
        }

        private void SetValueWithoutNotify(T value)
        {
            _value = value;
            _index = _choices.IndexOf(_value);
            SetupChoice(_selectedChoiceElement, _value, _index);
        }

        public BasePopupPalette(List<T> choices)
        {
            AddToClassList(ussClassName);

            RegisterCallback<PointerDownEvent>(OnPointerDownEvent);

            _selectedChoiceElement = new VisualElement { name = "popup-palette-selected-option", pickingMode = PickingMode.Ignore };
            _selectedChoiceElement.AddToClassList(optionUssClassName);
            Add(_selectedChoiceElement);
            
            // _panelContainer = new VisualElement { name = "popup-palette-container" };
            // Add(_panelContainer);
            _panel = new VisualElement { name = "popup-palette-panel", pickingMode = PickingMode.Ignore };
            _panel.AddToClassList(panelUssClassName);
            _panel.pickingMode = PickingMode.Position;
            _panel.contentContainer.focusable = true;
            _panel.RegisterCallback<FocusOutEvent>(OnFocusOut);
            Add(_panel);
            Hide();
            Choices = choices;
        }

        private int GetIndexOfElement(VisualElement ve)
        {
            return _choiceElements.IndexOf(ve);
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            Hide();
        }

        public void Hide()
        {
            _panel.style.display = DisplayStyle.None;
        }

        public void Show()
        {
            AddChoices();
            
            _panel.style.display = DisplayStyle.Flex;
            // var rootVisualContainer = (VisualElement)typeof(VisualElement).GetMethod("GetRootVisualContainer",
            //     BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);
            //
            // if (rootVisualContainer == null)
            // {
            //     Debug.LogError("Could not find rootVisualContainer...");
            //     return;
            // }

            // _panelContainer.style.position = Position.Absolute;
            // _panelContainer.style.left = rootVisualContainer.layout.x;
            // _panelContainer.style.top = rootVisualContainer.layout.y;
            // _panelContainer.style.width = rootVisualContainer.layout.width;
            // _panelContainer.style.height = rootVisualContainer.layout.height;

            schedule.Execute(_panel.contentContainer.Focus);
        }

        private void AddChoices()
        {
            _panel.Clear();
            var arrow = new VisualElement { name = "popup-palette-arrow", pickingMode = PickingMode.Ignore };
            arrow.AddToClassList(arrowUssClassName);
            _panel.Add(arrow);

            _choiceElements.Clear();

            for (var i = 0; i < Choices.Count; i++)
            {
                var choice = Choices[i];
                var element = new VisualElement();
                element.AddToClassList(optionUssClassName);
                element.RegisterCallback<PointerDownEvent>(OnItemPointerDownEvent);
                SetupChoice(element, choice, i);
                _panel.Add(element);
                _choiceElements.Add(element);
            }
        }

        private void OnItemPointerDownEvent(PointerDownEvent evt)
        {
            Index = GetIndexOfElement((VisualElement)evt.target);
            Hide();
            evt.StopPropagation();
        }

        protected virtual void SetupChoice(VisualElement element, T choice, int index)
        {
        }

        private void OnPointerDownEvent(PointerDownEvent evt)
        {
            Show();
        }
    }
}