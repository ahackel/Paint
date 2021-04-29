using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[Serializable]
public class DropdownPaletteEvent : UnityEvent<int> {}

public class DropdownPalette<T> : MonoBehaviour, IPointerClickHandler
{
    public RectTransform Palette;
    public Image CaptionImage;
    public Image ArrowImage;
    public Button ItemTemplate;
    public T[] Options;

    private GameObject _blocker;
    private int _selectedIndex;

    public DropdownPaletteEvent OnValueChanged { get; } = new DropdownPaletteEvent();

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetIndex(value);
    }
    
    public T SelectedValue => Options[SelectedIndex];

    public bool IsOpen => Palette.gameObject.activeSelf;
    
    private void Awake()
    {
        SetupPalette();
        Palette.gameObject.SetActive(false);
        ArrowImage.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        if (ItemTemplate == null || Options == null || Options.Length == 0)
        {
            return;
        }
        
        SetupOptions();
        RefreshShownValue();
    }

    private void SetupOptions()
    {
        for (int i = 0; i < Options.Length; i++)
        {
            var item = Instantiate(ItemTemplate, Palette.transform);
            var index = i;
            item.onClick.AddListener(() =>
            {
                SelectedIndex = index;
                Hide();
            });
            ItemTemplate.gameObject.SetActive(true);
            SetupItem(item, i);
        }

        ItemTemplate.gameObject.SetActive(false);
    }

    protected virtual void SetupItem(Button item, int index)
    {
    }

    private void SetupPalette()
    {
        Canvas popupCanvas = Palette.gameObject.AddComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 30000;
        Palette.gameObject.AddComponent<CanvasGroup>();
        Palette.gameObject.AddComponent<GraphicRaycaster>();
    }
    
    protected void SetIndex(int index, bool sendCallback = true)
    {
        if (Application.isPlaying && index == _selectedIndex)
        {
            return;
        }

        _selectedIndex = index;
        RefreshShownValue();

        if (sendCallback)
        {
            OnValueChanged.Invoke(_selectedIndex);
        }
    }

    protected virtual void RefreshShownValue()
    {
    }

    private void OnDisable()
    {
        DestroyBlocker();
    }

    public void Show()
    {
        Palette.gameObject.SetActive(true);
        ArrowImage.gameObject.SetActive(true);
        CreateBlocker();
    }

    private void CreateBlocker()
    {
        if (_blocker != null)
        {
            return;
        }
            
        var rootCanvas = GetComponentInParent<Canvas>();
            
        // Create blocker GameObject.
        GameObject blocker = new GameObject("Blocker");

        // Setup blocker RectTransform to cover entire root canvas area.
        RectTransform blockerRect = blocker.AddComponent<RectTransform>();
        blockerRect.SetParent(rootCanvas.transform, false);
        blockerRect.anchorMin = Vector3.zero;
        blockerRect.anchorMax = Vector3.one;
        blockerRect.sizeDelta = Vector2.zero;

        // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
        Canvas blockerCanvas = blocker.AddComponent<Canvas>();
        blockerCanvas.overrideSorting = true;
        Canvas dropdownCanvas = Palette.GetComponent<Canvas>();
        blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
        blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

        // Find the Canvas that this dropdown is a part of
        Canvas parentCanvas = null; 
        blocker.AddComponent<GraphicRaycaster>();

        // Add image since it's needed to block, but make it clear.
        Image blockerImage = blocker.AddComponent<Image>();
        blockerImage.color = Color.clear;

        // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
        Button blockerButton = blocker.AddComponent<Button>();
        blockerButton.onClick.AddListener(Hide);

        _blocker = blocker;
    }

    public void Hide()
    {
        Palette.gameObject.SetActive(false);
        ArrowImage.gameObject.SetActive(false);
        DestroyBlocker();
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }
        
    private void DestroyBlocker()
    {
        if (_blocker == null)
        {
            return;
        }
        Destroy(_blocker);
        _blocker = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }
}