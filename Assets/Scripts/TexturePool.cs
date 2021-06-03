using System.Collections.Generic;
using UnityEngine;

public class TexturePool
{
    public readonly int Capacity;
    public readonly int Width;
    public readonly int Height;

    private List<Texture2D> _available;
    private List<Texture2D> _used;
        
    public int UsedCount => _used.Count;
        
    public TexturePool(int capacity, int width, int height)
    {
        Capacity = capacity;
        Width = width;
        Height = height;

        _used = new List<Texture2D>(capacity);
        _available = new List<Texture2D>(capacity);
    }

    public Texture2D Get()
    {
        if (UsedCount == Capacity)
        {
            return null;
        }

        Texture2D texture;

        if (_available.Count > 0)
        {
            var index = _available.Count - 1;
            texture = _available[index];
            _available.RemoveAt(index);
        }
        else
        {
            texture = new Texture2D(Width, Height);
        }
        _used.Add(texture);
        return texture;
    }

    public void Return(Texture2D texture)
    {
        if (!_used.Remove(texture))
        {
            return;
        }
        _available.Add(texture);
    }
}