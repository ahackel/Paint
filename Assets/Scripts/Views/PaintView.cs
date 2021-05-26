using System;
using System.Collections.Generic;
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
		
		private ToolButton[] _toolButtons;
		private ToolButton _undoButton;
		private ToolButton _redoButton;
		private ToolButton _closeButton;
		private Image _canvasImage;
		private ColorPalette _colorPalette;
		private BrushSizePalette _brushSizePalette;

		private readonly List<float> _brushSizes = new List<float>
		{
			1f,
			4f,
			20f,
			200f
		};

		private readonly List<Color> _colors = new List<Color>
		{
			new Color32(0x00, 0x00, 0x00, 0xFF),
			new Color32(0x22, 0x20, 0x34, 0xFF),
			new Color32(0x45, 0x28, 0x3c, 0xFF),
			new Color32(0x66, 0x39, 0x31, 0xFF),
			new Color32(0x8f, 0x56, 0x3b, 0xFF),
			new Color32(0xdf, 0x71, 0x26, 0xFF),
			new Color32(0xd9, 0xa0, 0x66, 0xFF),
			new Color32(0xee, 0xc3, 0x9a, 0xFF),
			new Color32(0xfb, 0xf2, 0x36, 0xFF),
			new Color32(0x99, 0xe5, 0x50, 0xFF),
			new Color32(0x6a, 0xbe, 0x30, 0xFF),
			new Color32(0x37, 0x94, 0x6e, 0xFF),
			new Color32(0x4b, 0x69, 0x2f, 0xFF),
			new Color32(0x52, 0x4b, 0x24, 0xFF),
			new Color32(0x32, 0x3c, 0x39, 0xFF),
			new Color32(0x3f, 0x3f, 0x74, 0xFF),
			new Color32(0x30, 0x60, 0x82, 0xFF),
			new Color32(0x5b, 0x6e, 0xe1, 0xFF),
			new Color32(0x63, 0x9b, 0xff, 0xFF),
			new Color32(0x5f, 0xcd, 0xe4, 0xFF),
			new Color32(0xcb, 0xdb, 0xfc, 0xFF),
			new Color32(0xff, 0xff, 0xff, 0xFF),
			new Color32(0x9b, 0xad, 0xb7, 0xFF),
			new Color32(0x84, 0x7e, 0x87, 0xFF),
			new Color32(0x69, 0x6a, 0x6a, 0xFF),
			new Color32(0x59, 0x56, 0x52, 0xFF),
			new Color32(0x76, 0x42, 0x8a, 0xFF),
			new Color32(0xac, 0x32, 0x32, 0xFF),
			new Color32(0xd9, 0x57, 0x63, 0xFF),
			new Color32(0xd7, 0x7b, 0xba, 0xFF),
			new Color32(0x8f, 0x97, 0x4a, 0xFF),
			new Color32(0x8a, 0x6f, 0x30, 0xFF)
		};

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
			_colorPalette.Choices = _colors;
			_colorPalette.RegisterCallback<ChangeEvent<Color>>(evt => PaintDocument.Color = evt.newValue);
			_colorPalette.Value = PaintDocument.Color;

			_brushSizePalette = _rootElement.Q<BrushSizePalette>();
			_brushSizePalette.Choices = _brushSizes;
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
			var filename = (string) data;
			PaintDocument.LoadImage(filename);
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
