using System.Collections.Generic;
using Configs;
using UnityEngine.UIElements;
using Utilities;

namespace Views
{
    public class BookView : UiView
    {
        public ImageBookConfig ImageBookConfig;
        
        private List<Image> _thumbnails = new List<Image>();
        
        public override void Initialize()
        {
            base.Initialize();
            var container = _rootElement.Q<VisualElement>("thumbnail-container");
            container.Clear();
            foreach (var document in ImageBookConfig.Documents)
            {
                var thumbnail = new Image();
                thumbnail.AddToClassList("thumbnail");
                
                thumbnail.RegisterCallback<PointerDownEvent>(evt =>
                {
                    OpenView("PaintView", document);
                });
                container.Add(thumbnail);
                _thumbnails.Add(thumbnail);
            }
        }

        public override void Opened(object data)
        {
            LoadThumbnails();
        }

        private void LoadThumbnails()
        {
            for (int i = 0; i < ImageBookConfig.Documents.Count; i++)
            {
                var document = ImageBookConfig.Documents[i];
                var thumbnail = PaintUtils.LoadImageTexture(document.Name + ".thumb.png");
                if (thumbnail == null)
                {
                    thumbnail = document.Overlay;
                }

                _thumbnails[i].image = thumbnail;
            }
        }
    }
}