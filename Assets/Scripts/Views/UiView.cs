using UnityEngine;
using UnityEngine.UIElements;

namespace Views
{
    public abstract class UiView : MonoBehaviour
    {
        public UiService UiService;
        public string RootElementName;
        
        protected VisualElement _rootElement;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        public virtual void Initialize()
        {
            _rootElement = UiService.UiRoot.Q(RootElementName);
        }

        public void Open(object data)
        {
            if (_rootElement.parent != null)
            {
                return;
            }

            UiService.UiRoot.Add(_rootElement);
            _isOpen = true;
            Opened(data);
        }

        public void Close()
        {
            if (_rootElement.parent == null)
            {
                return;
            }

            Closing();
            _isOpen = false;
            _rootElement.RemoveFromHierarchy();
        }

        public virtual void Opened(object data)
        {
        }

        public virtual void Closing()
        {
        }

        public void OpenView(string name, object data = null) => UiService.OpenView(name, data);
    }
}