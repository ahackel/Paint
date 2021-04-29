using System.Collections.Generic;
using System.IO;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaintView : MonoBehaviour, IPointerDownHandler
{
	public RawImage Image;
	public Texture2D BrushTexture;
	public ColorPalette ColorPalette;
	public BrushSizePalette BrushSizePalette;
	public Color Color;
	[Range(1, 100)]
	public float BrushSize = 32f;
	public Tool CurrentTool;

	private RenderTexture _texture;
	private List<Tool.PaintParameters> PaintParameters = new List<Tool.PaintParameters>(3);
	private bool _isPainting;
	private const string TextureFilename = "Image01.png";

	private void OnEnable()
	{
		ColorPalette.OnValueChanged.AddListener(index => Color = ColorPalette.SelectedValue);
		BrushSizePalette.OnValueChanged.AddListener(index => BrushSize = BrushSizePalette.SelectedValue);
		Application.targetFrameRate = 60;
		ReadTexture();
		Image.texture = _texture;
	}

	private void OnDisable()
	{
		StoreTexture();
	}

	private void ReadTexture()
	{
		_texture = new RenderTexture(Screen.width, Screen.height, 0);
		try
		{
			var path = $"{Application.persistentDataPath}/{TextureFilename}";
			var bytes = File.ReadAllBytes(path);
			var texture = new Texture2D(1, 1);
			texture.LoadImage(bytes);
			Graphics.Blit(texture, _texture);
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
		var width = _texture.width;
		var height = _texture.height;
		var texture = new Texture2D(width, height);
		RenderTexture.active = _texture;
		texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		RenderTexture.active = null;
		var bytes = texture.EncodeToPNG();
		Destroy(texture);
		File.WriteAllBytes(path, bytes);
	}

	private Vector2 ScreenToTexture(Vector2 screenPosition)
	{
		var rectTransform = (RectTransform)transform;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, null,
			out var localPosition);

		var normalizedPosition = (localPosition - rectTransform.rect.min) / rectTransform.rect.size;
		var texturePosition = normalizedPosition * new Vector2(_texture.width, _texture.height);
		return texturePosition;
	}

	private Vector2 TextureToScreen(Vector2 texturePosition)
	{
		var rectTransform = (RectTransform)transform;
		var normalizedPosition = texturePosition / new Vector2(_texture.width, _texture.height);

		var localPosition = (normalizedPosition * rectTransform.rect.size) + rectTransform.rect.min;
		var worldPosition = rectTransform.TransformPoint(localPosition);
		var screenPosition = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
		return screenPosition;
	}

	private void Update()
	{
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

		if (!CurrentTool.Paint(_texture, PaintParameters))
		{
			PaintParameters.RemoveAt(0);
		}
	}

	public void Clear()
	{
		Clear(Color.white);
	}

	public void Clear(Color color)
	{
		RenderTexture.active = _texture;
		GL.Clear(false, true, color);
		RenderTexture.active = null;
	}

	private void StartPainting()
	{
		_isPainting = true;
		PaintParameters.Clear();
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
		_isPainting = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		StartPainting();
	}
}
