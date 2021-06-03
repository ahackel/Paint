using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class LayerManager : MonoBehaviour
{
    public Material LayerMaterial;

    private List<PaintLayer> _layers = new List<PaintLayer>();
    private int _currentLayerIndex = -1;
    
    public PaintLayer CurrentLayer => _currentLayerIndex < 0 ? null : _layers[_currentLayerIndex];
    public PaintLayer BaseLayer => _layers[0];
    public PaintLayer TopMostLayer => _layers[_layers.Count - 1];
    public int CurrentLayerIndex => _currentLayerIndex;

    public PaintLayer Create(string layerName, Vector2Int size, bool temporary = false)
    {
        var layerObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        layerObject.name = layerName;
        layerObject.transform.SetParent(transform, false);
        var layer = layerObject.AddComponent<PaintLayer>();
        layer.Initialize(size, LayerMaterial, temporary);
        var layerIndex = _currentLayerIndex + 1;
        _layers.Insert(layerIndex, layer);

        if (CurrentLayer == null)
        {
            _currentLayerIndex = layerIndex;
        }
        UpdateLayerOrder();

        return layer;
    }

    public void Remove(PaintLayer layer)
    {
        int index = _layers.IndexOf(layer);
        if (index == -1)
        {
            return;
        }

        if (CurrentLayer == layer)
        {
            if (_layers.Count == 1)
            {
                _currentLayerIndex = -1;
            }
            else
            {
                _currentLayerIndex = index == 0 ? 1 : index - 1;
            }
        }

        _layers.Remove(layer);
        Destroy(layer.gameObject);
        UpdateLayerOrder();
    }

    public void MergeDown(PaintLayer layer)
    {
        int index = _layers.IndexOf(layer);
        if (index < 1)
        {
            return;
        }
        
        Graphics.Blit(layer.RenderTexture, _layers[index - 1].RenderTexture, layer.Material);
        Remove(layer);
    }

    public void Clear()
    {
        for (var i = _layers.Count - 1; i >= 0; i--)
        {
            Remove(_layers[i]);
        }
    }

    private void UpdateLayerOrder()
    {
        for (var i = 0; i < _layers.Count; i++)
        {
            _layers[i].Material.renderQueue = i;
            _layers[i].transform.SetSiblingIndex(i);
        }
    }

    public Texture2D GetImageThumbnail()
    {
        var renderTexture = RenderTexture.GetTemporary(PaintUtils.ThumbnailWidth, PaintUtils.ThumbnailHeight, 0);
        renderTexture.filterMode = FilterMode.Trilinear;
        for (var i = 0; i < _layers.Count; i++)
        {
            Graphics.Blit(_layers[i].RenderTexture, renderTexture, _layers[i].Material);
        }

        var resizedTexture = new Texture2D(PaintUtils.ThumbnailWidth, PaintUtils.ThumbnailHeight);
        renderTexture.CopyToTexture(resizedTexture);
        RenderTexture.ReleaseTemporary(renderTexture);
        return resizedTexture;
    }
}