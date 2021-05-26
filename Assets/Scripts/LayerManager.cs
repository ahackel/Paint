using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    public PaintLayer Current;
    public Material LayerMaterial;

    private List<PaintLayer> _layers = new List<PaintLayer>();

    public PaintLayer Create(string layerName, Vector2Int size, bool temporary = false)
    {
        var layerObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        layerObject.name = layerName;
        layerObject.transform.SetParent(transform, false);
        var layer = layerObject.AddComponent<PaintLayer>();
        layer.Initialize(size, LayerMaterial, temporary);
        _layers.Add(layer);

        if (Current == null)
        {
            Current = layer;
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
            
        if (Current == layer)
        {
            Current = _layers.Count == 1 ? null : _layers[index == 0 ? 1 : index - 1];
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
        }
    }
}