using TMPro;
using UnityEngine;

public class PaintLayer : MonoBehaviour
{
    private RenderTexture _renderTexture;
    private Material _material;
    private bool _temporary;

    public RenderTexture RenderTexture => _renderTexture;
    public Vector2Int Size => new Vector2Int(RenderTexture.width, RenderTexture.height);
    public Material Material => _material;

    public void Initialize(Vector2Int size, Material material, bool temporary)
    {
        _temporary = temporary;
        if (_temporary)
        {
            gameObject.name += "(temporary)";
        }
        transform.localScale = new Vector3((float)size.x / size.y, 1f);

        _renderTexture = temporary
            ? RenderTexture.GetTemporary(size.x, size.y, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear)
            : new RenderTexture(size.x, size.y, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _material = Instantiate(material);
        _material.mainTexture = _renderTexture;
        meshRenderer.sharedMaterial = _material;
        
    }

    public void OnDestroy()
    {
        Destroy(_material);
        if (_temporary)
        {
            RenderTexture.ReleaseTemporary(_renderTexture);
        }
        else
        {
            _renderTexture.Release();
        }
    }
}