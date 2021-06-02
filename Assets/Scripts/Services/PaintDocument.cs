using System.Collections.Generic;
using System.IO;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Services
{
    public class PaintDocument : MonoBehaviour
    {
        public Camera Camera;
        public LayerManager Layers;
        public Color Color;

        [Range(1, 100)]
        public float BrushSize = 32f;

        public Tool CurrentTool;
        public Tool[] Tools;
        private bool _isPainting;
        private readonly UndoHistory<Texture2D> _history = new UndoHistory<Texture2D>();
        private string _imageFilename;
        private Material _blurMaterial;
        private Vector2Int _imageSize = new Vector2Int(0, 0);

        private static readonly int PropId_Alpha = Shader.PropertyToID("_Alpha");
        private PaintLayer _paintLayer;

        private const int defaultImageWidth = 2 * 1024;
        private const int defaultImageHeight = 2 * 768;
        
        public string ImageFilename => _imageFilename;
        public Vector2Int ImageSize => _imageSize;

        
        public void Start()
        {
            // _blurMaterial = new Material(Shader.Find("Paint/Blur"));
            // _blurMaterial.SetVector("_TextureSize", new Vector2(_paintBuffer.width, _paintBuffer.height));
        }
        
        // private void Blur()
        // {
        //     var destTexture = RenderTexture.GetTemporary(_paintBuffer.descriptor);
        //     Graphics.Blit(_paintBuffer, destTexture, _blurMaterial, 0);
        //     Graphics.Blit(destTexture, _paintBuffer);
        //     RenderTexture.ReleaseTemporary(destTexture);
        // }

        public void LoadImage(string fileName)
        {
            Debug.Log($"LoadImage '{fileName}'");
            
            Layers.Clear();
            _imageFilename = fileName;
            var texture = PaintUtils.LoadImageTexture(_imageFilename);

            if (texture != null && (texture.width != defaultImageWidth || texture.height != defaultImageHeight))
            {
                Destroy(texture);
                texture = null;
            }

            if (texture != null){
                // Set whole wetness buffer to dry:
                //_copyPaintBufferMaterial.SetFloat(PropId_Alpha, 0);
                //Graphics.Blit(texture, _paintBuffer, _copyPaintBufferMaterial);
                Layers.Create("Base", new Vector2Int(texture.width, texture.height));
                Graphics.Blit(texture, Layers.CurrentLayer.RenderTexture);
                Destroy(texture);
            }
            else
            {
                Layers.Create("Base", new Vector2Int(defaultImageWidth, defaultImageHeight));
                Clear(Color.white);
            }

            _imageSize = Layers.CurrentLayer.Size;
        }

        public void SaveImage()
        {
            if (string.IsNullOrEmpty(_imageFilename))
            {
                return;
            }
			
            Debug.Log($"SaveImage '{_imageFilename}'");
            var texture = _history.GetCurrentState();
            PaintUtils.SaveImageTexture(_imageFilename, texture);
            Destroy(texture);
            SaveImageThumbnail();
        }

        private void SaveImageThumbnail()
        {
            var texture = Layers.GetImageThumbnail();
            PaintUtils.SaveImageTexture(Path.ChangeExtension(_imageFilename, ".thumb.png"), texture);
            Destroy(texture);
        }

        private Vector2 ScreenToTexture(Vector2 screenPosition)
        {
            var worldPosition = Camera.ScreenToWorldPoint(screenPosition);
            var localPosition = Layers.CurrentLayer.transform.InverseTransformPoint(worldPosition);
            var texturePosition = new Vector2(localPosition.x + 0.5f, localPosition.y + 0.5f) * ImageSize;
            return texturePosition;
        }

        // private Vector2 TextureToScreen(Vector2 texturePosition)
        // {
        //     var rectTransform = (RectTransform) _paintView.transform;
        //     var normalizedPosition = texturePosition / new Vector2(RenderTexture.width, RenderTexture.height);
        //
        //     var localPosition = (normalizedPosition * rectTransform.rect.size) + rectTransform.rect.min;
        //     var worldPosition = rectTransform.TransformPoint(localPosition);
        //     var screenPosition = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
        //     return screenPosition;
        // }

        public void Clear()
        {
            Clear(new Color(1, 1, 1, 1));
            RecordUndo();
        }

        public void Clear(Color color)
        {
            RenderTexture.active = Layers.CurrentLayer.RenderTexture;
            GL.Clear(false, true, color);
            RenderTexture.active = null;
        }

        public void RecordUndo()
        {
            _history.RecordState(Layers.CurrentLayer.RenderTexture.CaptureRenderTexture());
        }

        public void ResetUndoHistory()
        {
            _history.Clear();
            RecordUndo();
        }

        public void Undo()
        {
            if (!_history.CanUndo())
            {
                return;
            }

            // _copyPaintBufferMaterial.SetFloat(PropId_Alpha, 0);
            // Graphics.Blit(_history.Undo(), Layers.Current.RenderTexture, _copyPaintBufferMaterial);
            Graphics.Blit(_history.Undo(), Layers.CurrentLayer.RenderTexture);
        }

        public void Redo()
        {
            if (!_history.CanRedo())
            {
                return;
            }

            // _copyPaintBufferMaterial.SetFloat(PropId_Alpha, 0);
            // Graphics.Blit(_history.Redo(), Layers.Current.RenderTexture, _copyPaintBufferMaterial);
            Graphics.Blit(_history.Redo(), Layers.CurrentLayer.RenderTexture);
        }

        public void StartPainting()
        {
            if (CurrentTool == null)
            {
                return;
            }

            if (_isPainting)
            {
                return;
            }

            _isPainting = true;
            _paintLayer = Layers.Create("Paint", ImageSize, true);
            RenderTexture.active = _paintLayer.RenderTexture;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = null;

            CurrentTool.Down(_paintLayer.RenderTexture, GetPaintParameters(), Layers);
        }

        public void StopPainting()
        {
            if (!_isPainting)
            {
                return;
            }
            
            CurrentTool.Up(_paintLayer.RenderTexture, GetPaintParameters());

            Layers.MergeDown(_paintLayer);
            _paintLayer = null;
            _isPainting = false;
            RecordUndo();
        }

        private Tool.PaintParameters GetPaintParameters()
        {
            var pointer = Pointer.current;
            var pen = Pen.current;
            return new Tool.PaintParameters
            {
                IsPen = pointer is Pen,
                Position = ScreenToTexture(pointer.position.ReadValue()),
                Pressure = pointer.pressure.ReadValue(),
                BrushSize = BrushSize,
                Tilt = pen != null ? pen.tilt.ReadValue() : Vector2.zero,
                Color = Color
            };
        }

        public void Update()
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                PaintUtils.GaussianBlur(Layers.CurrentLayer.RenderTexture);
            }
            
            //Blur();
            if (!_isPainting)
            {
                return;
            }
            
            var pointer = Pointer.current;
            if (pointer == null || CurrentTool == null)
            {
                StopPainting();
                return;
            }

            CurrentTool.Move(_paintLayer.RenderTexture, GetPaintParameters(), _history.GetCurrentState());

            // _copyPaintBufferMaterial.SetFloat(PropId_Alpha, 1);
            //Graphics.Blit(_paintBuffer, _renderTexture, _copyPaintBufferMaterial, 0);
        }

        private void DrawCursor()
        {
        	GL.PushMatrix();
        	GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height);
        
        	var position = Pointer.current.position.ReadValue();
        	var size = 10f;
        	var rect = new Rect(position.x - 0.5f * size, position.y - 0.5f * size, size, size);
        	Graphics.DrawTexture(rect, Texture2D.whiteTexture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.green);
        
        	GL.PopMatrix();
        }

        // public void Draw()
        // {
        //     var imageAspectRatio = (float)ImageSize.x / ImageSize.y;
        //     var screenAspectRatio = (float)Screen.width / Screen.height;
        //     var imageWidthOnScreen = Screen.height * imageAspectRatio;
        //
        //     var scale = new Vector2(Screen.width / imageWidthOnScreen, 1f);
        //     var offset = new Vector2(0.5f * (1f - scale.x), 0f);
        //     
        //     foreach (var layer in Layers)
        //     {
        //         Graphics.Blit(layer.RenderTexture, null, scale, offset);
        //     }
        //
        //     if (_isPainting)
        //     {
        //         DrawPaintBuffer(null, scale, offset);
        //     }
        //
        //     //DrawCursor();
        // }

        public bool CanUndo() => _history.CanUndo();
        public bool CanRedo() => _history.CanRedo();
    }
}