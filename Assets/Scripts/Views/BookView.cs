using UnityEngine.UIElements;

namespace Views
{
    public class BookView : UiView
    {
        public UIDocument Ui;
        
        private void OnEnable()
        {
            var root = Ui.rootVisualElement;
            var container = root.Q<VisualElement>("thumbnail-container");
            container.Clear();
            for (int i = 0; i < 10; i++)
            {
                var thumbnail = new VisualElement();
                thumbnail.AddToClassList("thumbnail");
                thumbnail.RegisterCallback<PointerDownEvent>(evt => OpenView("PaintView"));
                container.Add(thumbnail);
            }
        }
    }
}