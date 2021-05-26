using UnityEngine.UIElements;
using Utilities;

namespace Views
{
    public class BookView : UiView
    {
        const int ThumbnailCount = 16;

        private Image[] _thumbnails = new Image[ThumbnailCount];
        
        public override void Initialize()
        {
            base.Initialize();
            var container = _rootElement.Q<VisualElement>("thumbnail-container");
            container.Clear();
            for (int i = 0; i < ThumbnailCount; i++)
            {
                _thumbnails[i] = new Image();
                _thumbnails[i].AddToClassList("thumbnail");
                
                var filename = $"image{i:D2}";
                _thumbnails[i].RegisterCallback<PointerDownEvent>(evt =>
                {
                    OpenView("PaintView", filename);
                });
                container.Add(_thumbnails[i]);
            }
        }

        public override void Opened(object data)
        {
            LoadThumbnails();
        }

        private void LoadThumbnails()
        {
            for (int i = 0; i < ThumbnailCount; i++)
            {
                var filename = $"image{i:D2}";
                _thumbnails[i].image = PaintUtils.LoadImageTexture(filename);
            }
        }
    }
}