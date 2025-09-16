using UnityEngine;

public class Floor : MonoBehaviour
{
    public Vector2Int GridPosition;

    [SerializeField] private Renderer _tileRenderer;

    MaterialPropertyBlock mpb;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int ColorID     = Shader.PropertyToID("_Color");

    Color baseColor;
    
    void Awake()
    {
        if (!_tileRenderer)
            _tileRenderer = GetComponent<Renderer>();

        mpb  = new MaterialPropertyBlock();
        
        var mat = _tileRenderer.sharedMaterial;

        baseColor = mat && mat.HasProperty(BaseColorID) ? mat.GetColor(BaseColorID)
                  : mat && mat.HasProperty(ColorID)     ? mat.GetColor(ColorID)
                  : Color.white;
    }

    public void Init(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }

    public void SetTint(Color? tint)
    {
        if (!_tileRenderer) return;
        
        _tileRenderer.GetPropertyBlock(mpb);
        
        var c = tint.HasValue ? tint.Value : baseColor;

        if (_tileRenderer.sharedMaterial && _tileRenderer.sharedMaterial.HasProperty(BaseColorID)) mpb.SetColor(BaseColorID, c);
        else mpb.SetColor(ColorID, c);
        
        _tileRenderer.SetPropertyBlock(mpb);
    }
}
