using UnityEngine;

public class Floor : MonoBehaviour
{
    public Vector2Int GridPosition;
    public bool IsOccuppied = false;

    [SerializeField] private Renderer _tileRenderer;
    
    void Awake()
    {
        if (!_tileRenderer)
            _tileRenderer = GetComponent<Renderer>();
    }

    public void Init(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }
}
