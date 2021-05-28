using System;
using System.Collections.Generic;
using Configs;
using Controls;
using Services;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using ColorPalette = Controls.ColorPalette;
using Image = UnityEngine.UIElements.Image;

namespace Views
{
	public class PaintView : UiView, IPointerDownHandler
	{
		public PaintDocument PaintDocument;
		public ColorSet ColorSet;
		public BrushSizeSet BrushSizeSet;
		
		private ToolButton[] _toolButtons;
		private ToolButton _undoButton;
		private ToolButton _redoButton;
		private ToolButton _closeButton;
		private Image _canvasImage;
		private ColorPalette _colorPalette;
		private BrushSizePalette _brushSizePalette;
		
		public override void Initialize()
		{
			base.Initialize();
			_canvasImage = _rootElement.Q<Image>("CanvasImage");
			_canvasImage.RegisterCallback<PointerDownEvent>(e => PaintDocument.StartPainting());
			_canvasImage.RegisterCallback<PointerUpEvent>(e => PaintDocument.StopPainting());

			_toolButtons = new ToolButton[PaintDocument.Tools.Length];
			for (var i = 0; i < PaintDocument.Tools.Length; i++)
			{
				var tool = PaintDocument.Tools[i];
				if (tool == null)
				{
					throw new Exception("Empty tool in tools list of PaintView");
				}
				_toolButtons[i] = _rootElement.Q<ToolButton>(tool.name);
				_toolButtons[i].Clicked += () => SelectTool(tool);

				if (tool.name == "EraserTool")
				{
					_toolButtons[i].DoubleClicked += PaintDocument.Clear;
				}
			}

			_undoButton = _rootElement.Q<ToolButton>("Undo");
			_undoButton.Clicked += () => PaintDocument.Undo();
			_redoButton = _rootElement.Q<ToolButton>("Redo");
			_redoButton.Clicked += () => PaintDocument.Redo();

			_closeButton = _rootElement.Q<ToolButton>("Close");
			_closeButton.Clicked += () => OpenView("BookView");

			_colorPalette = _rootElement.Q<ColorPalette>();
			_colorPalette.Choices = ColorSet.Colors;
			_colorPalette.RegisterCallback<ChangeEvent<Color>>(evt => PaintDocument.Color = evt.newValue);
			_colorPalette.Value = PaintDocument.Color;

			_brushSizePalette = _rootElement.Q<BrushSizePalette>();
			_brushSizePalette.Choices = BrushSizeSet.BrushSizes;
			_brushSizePalette.RegisterCallback<ChangeEvent<float>>(evt => PaintDocument.BrushSize = evt.newValue);
			_brushSizePalette.Value = PaintDocument.BrushSize;
		}
		
		private void SelectTool(Tool newTool)
		{
			PaintDocument.CurrentTool = newTool;
			for (var i = 0; i < PaintDocument.Tools.Length; i++)
			{
				_toolButtons[i].Selected = PaintDocument.CurrentTool == PaintDocument.Tools[i];
			}
		}

		public override void Opened(object data)
		{
			var documentConfig = (ImageBookConfig.ImageDocumentConfig) data;
			PaintDocument.LoadImage($"{documentConfig.Name}.png");
			if (documentConfig.Overlay != null)
			{
				var overlayLayer = PaintDocument.Layers.Create("Overlay",
					new Vector2Int(documentConfig.Overlay.width, documentConfig.Overlay.height));
				Graphics.Blit(documentConfig.Overlay, overlayLayer.RenderTexture);
			}
			
			PaintDocument.ResetUndoHistory();
		}

		public override void Closing()
		{
			if (IsOpen)
			{
				PaintDocument.SaveImage();
			}
		}
		
		private void Update()
		{
			if (!IsOpen)
			{
				return;
			}
			
			PaintDocument.Update();
			UpdateUndoButtonState();
		}

		private void OnPostRender()
		{
			// PaintDocument.Draw();
		}

		public void UpdateUndoButtonState()
		{
			_undoButton.SetEnabled(PaintDocument.CanUndo());
			_redoButton.SetEnabled(PaintDocument.CanRedo());
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			PaintDocument.StartPainting();
		}
	}
}
