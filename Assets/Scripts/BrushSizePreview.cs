using UnityEngine;

public class BrushSizePreview : MonoBehaviour
{
    public RectTransform RectTransform;
        
    private float _size;

    public float Size
    {
        get => _size;
        set
        {
            _size = value;
            RectTransform.sizeDelta = Vector2.one * _size;
        }
    }
}