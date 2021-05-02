using System;
using System.Collections.Generic;
using System.IO;
using Controls;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using ColorPalette = Controls.ColorPalette;
using Image = UnityEngine.UIElements.Image;

public class PaintView : MonoBehaviour, IPointerDownHandler
{
	public Color Color;
	[Range(1, 100)]
	public float BrushSize = 32f;
	public Tool CurrentTool;
	public UIDocument Ui;
	public UIDocument PaintScreen;
	public Tool[] Tools;

	private RenderTexture _renderTexture;
	private List<Tool.PaintParameters> PaintParameters = new List<Tool.PaintParameters>(3);
	private bool _isPainting;
	private ToolButton[] _toolButtons;
	private Image _canvasImage;
	private ColorPalette _colorPalette;
	private BrushSizePalette _brushSizePalette;

	private const string TextureFilename = "Image01.png";


	private readonly List<float> _brushSizes = new List<float>
	{
		4f,
		20f,
		40f
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

	private void OnEnable()
	{
		Application.targetFrameRate = 60;
		ReadTexture();

		var root = Ui.rootVisualElement;
		
		_canvasImage = PaintScreen.rootVisualElement.Q<Image>("CanvasImage");
		_canvasImage.image = _renderTexture;
		_canvasImage.RegisterCallback<MouseDownEvent>(e => StartPainting());

		_toolButtons = new ToolButton[Tools.Length];
		for (var i = 0; i < Tools.Length; i++)
		{
			var tool = Tools[i];
			if (tool == null)
			{
				throw new Exception("Empty tool in tools list of PaintView");
			}
			_toolButtons[i] = root.Q<ToolButton>(tool.name);
			_toolButtons[i].Clicked += () => SelectTool(tool);

			if (tool.name == "EraserTool")
			{
				_toolButtons[i].RegisterCallback<PointerDownEvent>(evt =>
				{
					if (evt.clickCount > 1)
					{
						Clear();
					}
				});
			}
		}

		_colorPalette = root.Q<ColorPalette>();
		_colorPalette.Choices = _colors;
		_colorPalette.RegisterCallback<ChangeEvent<Color>>(evt => Color = evt.newValue);

		_brushSizePalette = root.Q<BrushSizePalette>();
		_brushSizePalette.Choices = _brushSizes;
		_brushSizePalette.RegisterCallback<ChangeEvent<float>>(evt => BrushSize = evt.newValue);
	}

	private void SelectTool(Tool newTool)
	{
		CurrentTool = newTool;
		for (var i = 0; i < Tools.Length; i++)
		{
			_toolButtons[i].Selected = CurrentTool == Tools[i];
		}
	}

	private void OnDisable()
	{
		StoreTexture();
	}

	private void ReadTexture()
	{
		_renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
		try
		{
			var path = $"{Application.persistentDataPath}/{TextureFilename}";
			var bytes = File.ReadAllBytes(path);
			var texture = new Texture2D(1, 1);
			texture.LoadImage(bytes);
			Graphics.Blit(texture, _renderTexture);
			Destroy(texture);
		}
		catch
		{
			Clear(Color.white);
		}
	}

	private void StoreTexture()
	{
		var path = $"{Application.persistentDataPath}/{TextureFilename}";
		var width = _renderTexture.width;
		var height = _renderTexture.height;
		var texture = new Texture2D(width, height);
		RenderTexture.active = _renderTexture;
		texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		RenderTexture.active = null;
		var bytes = texture.EncodeToPNG();
		Destroy(texture);
		File.WriteAllBytes(path, bytes);
	}

	private Vector2 ScreenToTexture(Vector2 screenPosition)
	{
		var normalizedPosition = screenPosition / new Vector2(Screen.width, Screen.height);
		var texturePosition = normalizedPosition * new Vector2(_renderTexture.width, _renderTexture.height);
		return texturePosition;
	}

	private Vector2 TextureToScreen(Vector2 texturePosition)
	{
		var rectTransform = (RectTransform)transform;
		var normalizedPosition = texturePosition / new Vector2(_renderTexture.width, _renderTexture.height);

		var localPosition = (normalizedPosition * rectTransform.rect.size) + rectTransform.rect.min;
		var worldPosition = rectTransform.TransformPoint(localPosition);
		var screenPosition = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
		return screenPosition;
	}

	private void Update()
	{
		// DrawCursor();
		if (!_isPainting)
		{
			return;
		}
		
		var pointer = Pointer.current;
		if (pointer == null || !pointer.press.isPressed || CurrentTool == null)
		{
			StopPainting();
			return;
		}

		if (PaintParameters.Count > 2)
		{
			PaintParameters.RemoveAt(2);
		}
		PaintParameters.Insert(0, GetPaintParameters());

		if (!CurrentTool.Move(_renderTexture, PaintParameters))
		{
			PaintParameters.RemoveAt(0);
		}
	}

	private void DrawCursor()
	{
		GL.PushMatrix();
		GL.LoadPixelMatrix(0, _renderTexture.width, 0, _renderTexture.height);

		var position = Pointer.current.position.ReadValue();
		var size = 10f;
		var rect = new Rect(position.x - 0.5f * size, position.y - 0.5f * size, size, size);
		Graphics.DrawTexture(rect, Texture2D.whiteTexture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.green);
		
		GL.PopMatrix();
	}

	public void Clear()
	{
		Clear(Color.white);
	}

	public void Clear(Color color)
	{
		RenderTexture.active = _renderTexture;
		GL.Clear(false, true, color);
		RenderTexture.active = null;
	}

	private void StartPainting()
	{
		_isPainting = true;
		PaintParameters.Clear();
		PaintParameters.Add(GetPaintParameters());
		CurrentTool.Down(_renderTexture, PaintParameters);
	}

	private Tool.PaintParameters GetPaintParameters()
	{
		var pointer = Pointer.current;
		var pen = Pen.current;
		return new Tool.PaintParameters
		{
			Position = ScreenToTexture(pointer.position.ReadValue()),
			Pressure = pointer.pressure.ReadValue(),
			BrushSize = BrushSize,
			Tilt = pen != null ? pen.tilt.ReadValue() : Vector2.zero,
			Color = Color
		};
	}

	private void StopPainting()
	{
		CurrentTool.Up(_renderTexture, PaintParameters);
		_isPainting = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (CurrentTool == null)
		{
			return;
		}
		StartPainting();
	}
}
