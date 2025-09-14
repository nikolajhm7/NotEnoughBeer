using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    public int width = 10;
    public int height = 10;
    public float tileSize = 1f;
    public Vector3 origin = Vector3.zero;

    [Header("References")]
    public Tile tilePrefab;
    public Transform container;

    [Header("Runtime")]
    public bool generateOnPlay = true;

    public Dictionary<Vector2Int, Tile> Tiles { get; private set; } = new();

    void Start()
    {
        if (generateOnPlay) Generate();
    }

    [ContextMenu("Generate Grid")]
    public void Generate()
    {
        Clear();
        if (!tilePrefab) { Debug.LogWarning("Assign a Tile prefab."); return; }

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var grid = new Vector2Int(x, y);
            var world = GridToWorld(grid);

            var t = Instantiate(tilePrefab, world, Quaternion.identity, container ? container : transform);
            t.name = $"Tile_{x}_{y}";
            t.Init(grid);

            Tiles[grid] = t;
        }
    }

    public void Clear()
    {
        foreach (Transform c in (container ? container : transform))
            if (Application.isEditor) DestroyImmediate(c.gameObject);
            else Destroy(c.gameObject);
        Tiles.Clear();
    }

    public Vector3 GridToWorld(Vector2Int grid)
        => origin + new Vector3(grid.x * tileSize, 0f, grid.y * tileSize);

    public Vector2Int WorldToGrid(Vector3 world)
    {
        Vector3 local = world - origin;
        return new Vector2Int(Mathf.RoundToInt(local.x / tileSize), Mathf.RoundToInt(local.z / tileSize));
    }

    public bool IsInside(Vector2Int grid) => grid.x >= 0 && grid.y >= 0 && grid.x < width && grid.y < height;

    public void Expand(int addRight, int addLeft, int addUp, int addDown)
    {
        int newWidth = width + addRight + addLeft;
        int newHeight = height + addUp + addDown;

        // Shift origin if we add to the left/down
        origin -= new Vector3(addLeft * tileSize, 0f, addDown * tileSize);

        width = newWidth;
        height = newHeight;
        Generate();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        for (int y = 0; y <= height; y++)
            Gizmos.DrawLine(GridToWorld(new Vector2Int(0, y)), GridToWorld(new Vector2Int(width, y)));
        for (int x = 0; x <= width; x++)
            Gizmos.DrawLine(GridToWorld(new Vector2Int(x, 0)), GridToWorld(new Vector2Int(x, height)));
    }
#endif
}