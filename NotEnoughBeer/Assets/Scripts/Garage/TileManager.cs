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
    public Floor floorPrefab;
    public Wall wallPrefab;

    [Header("Parents")]
    public Transform floorContainer;
    public Transform wallsContainer;

    [Header("Runtime")]
    public bool generateOnPlay = true;

    [Header("Wall Settings")]
    public float wallHeight = 1f;
    public float wallThickness = 0.1f;

    public Dictionary<Vector2Int, Floor> Tiles { get; private set; } = new();

    public readonly List<Renderer> southWalls = new();
    public readonly List<Renderer> northWalls = new();
    public readonly List<Renderer> westWalls  = new();
    public readonly List<Renderer> eastWalls  = new();

    void Start()
    {
        if (generateOnPlay) Generate();
    }

    [ContextMenu("Generate Grid")]
    public void Generate()
    {
        Clear();
        if (!floorPrefab) { Debug.LogWarning("Assign a Tile prefab."); return; }
        if (!wallPrefab) { Debug.LogError("Assign Wall prefab."); return; }

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var grid = new Vector2Int(x, y);
            var world = GridToWorld(grid);

            var t = Instantiate(floorPrefab, world, Quaternion.identity, floorContainer ? floorContainer : transform);
            t.name = $"Tile_{x}_{y}";
            t.Init(grid);

            Tiles[grid] = t;
        }

        BuildPerimeterWalls();
    }

    public void Clear()
    {
        foreach (Transform c in (floorContainer ? floorContainer : transform))
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

    void BuildPerimeterWalls()
    {
        {
            float z = origin.z - tileSize * 0.5f;
            for (int x = 0; x < width; x++)
            {
                float cx = origin.x + x * tileSize;
                var pos = new Vector3(cx, wallHeight * 0.5f, z);
                var r = SpawnWall(pos, Quaternion.identity);
                southWalls.Add(r);
            }
        }

        {
            float z = origin.z + (height * tileSize - tileSize * 0.5f);
            for (int x = 0; x < width; x++)
            {
                float cx = origin.x + x * tileSize;
                var pos = new Vector3(cx, wallHeight * 0.5f, z);
                var r = SpawnWall(pos, Quaternion.identity);
                northWalls.Add(r);
            }
        }

        {
            float x = origin.x - tileSize * 0.5f;
            for (int y = 0; y < height; y++)
            {
                float cz = origin.z + y * tileSize;
                var pos = new Vector3(x, wallHeight * 0.5f, cz);
                var r = SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f));
                westWalls.Add(r);
            }
        }

        {
            float x = origin.x + (width * tileSize - tileSize * 0.5f);
            for (int y = 0; y < height; y++)
            {
                float cz = origin.z + y * tileSize;
                var pos = new Vector3(x, wallHeight * 0.5f, cz);
                var r = SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f));
                eastWalls.Add(r);
            }
        }
    }

    Renderer SpawnWall(Vector3 pos, Quaternion rot)
    {
        var w = Instantiate(wallPrefab, pos, rot, wallsContainer ? wallsContainer : transform);
        w.transform.localScale = new Vector3(tileSize, wallHeight, wallThickness);
        var r = w.GetComponentInChildren<Renderer>();
        return r;
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