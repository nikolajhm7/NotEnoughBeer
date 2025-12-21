using System.Collections.Generic;
using System.Linq;
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
    public int wallLevels = 3;
    public float wallThicknessOffset = 0.1f;
    public float wallHeightOffset = 0.4f;

    // Tiles
    public Dictionary<Vector2Int, Floor> Tiles { get; private set; } = new();
    readonly HashSet<Vector2Int> occupied = new();

    // Walls
    public readonly List<Renderer> southWalls = new();
    public readonly List<Renderer> northWalls = new();
    public readonly List<Renderer> westWalls  = new();
    public readonly List<Renderer> eastWalls  = new();

    // Interactables
    private readonly Dictionary<Vector2Int, List<IInteractable>> interactablesByCell = new();
    private readonly Dictionary<IInteractable, List<Vector2Int>> cellsByInteractable = new();

    void Start()
    {
        if (generateOnPlay) Generate();
    }

    #region WorldGeneration
    [ContextMenu("Generate Grid")]
    public void Generate()
    {
        var clearState = Clear();
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

    public ClearState Clear()
    {
        var floorParent = floorContainer ? floorContainer : transform;

        foreach (Transform c in floorParent)
        {
            if (Application.isEditor)
                DestroyImmediate(c.gameObject);
            else
                Destroy(c.gameObject);
        }

        var wallsParent = wallsContainer ? wallsContainer : transform;

        foreach (Transform c in wallsParent)
        {
            if (Application.isEditor)
                DestroyImmediate(c.gameObject);
            else
                Destroy(c.gameObject);
        }

        // EXTRA: destroy ANY Wall component anywhere (old grids, wrong parents, etc.)
        var strayWalls = FindObjectsOfType<Wall>();
        foreach (var w in strayWalls)
        {
            if (Application.isEditor)
                DestroyImmediate(w.gameObject);
            else
                Destroy(w.gameObject);
        }

        var clearState = new ClearState();
        foreach (var occupiedCell in occupied)
        {
            clearState.OccupiedCells[occupiedCell] = true;
        }

        Tiles.Clear();
        occupied.Clear();
        southWalls.Clear(); northWalls.Clear(); westWalls.Clear(); eastWalls.Clear();

        interactablesByCell.Clear();
        cellsByInteractable.Clear();

        return clearState;
    }


    void BuildPerimeterWalls()
    {
        southWalls.Clear(); northWalls.Clear(); westWalls.Clear(); eastWalls.Clear();

        float zSouth = origin.z - tileSize * 0.5f - wallThicknessOffset;
        float zNorth = origin.z + (height * tileSize - tileSize * 0.5f) + wallThicknessOffset;

        float xWest = origin.x - tileSize * 0.5f - wallThicknessOffset;
        float xEast = origin.x + (width * tileSize - tileSize * 0.5f) + wallThicknessOffset;

        for (int h = 0; h < wallLevels; h++)
        {
            float y = h + wallHeightOffset;

            // --- South (bottom edge), length along +X
            for (int x = 0; x < width; x++)
            {
                float cx = origin.x + x * tileSize;
                var r = SpawnWallTile(new Vector3(cx, y, zSouth), Quaternion.Euler(-90f, 0f, 0f));
                southWalls.Add(r);
            }

            // --- North (top edge), length along +X
            for (int x = 0; x < width; x++)
            {
                float cx = origin.x + x * tileSize;
                var r = SpawnWallTile(new Vector3(cx, y, zNorth), Quaternion.Euler(-90f, 0f, 0f));
                northWalls.Add(r);
            }

            // --- West (left edge), rotate so length runs along +Z
            for (int yIdx = 0; yIdx < height; yIdx++)
            {
                float cz = origin.z + yIdx * tileSize;
                var r = SpawnWallTile(new Vector3(xWest, y, cz), Quaternion.Euler(-90f, 90f, 0f));
                westWalls.Add(r);
            }

            // --- East (right edge), rotate so length runs along +Z
            for (int yIdx = 0; yIdx < height; yIdx++)
            {
                float cz = origin.z + yIdx * tileSize;
                var r = SpawnWallTile(new Vector3(xEast, y, cz), Quaternion.Euler(-90f, 90f, 0f));
                eastWalls.Add(r);
            }
        }
    }

    Renderer SpawnWallTile(Vector3 pos, Quaternion rot)
    {
        var w = Instantiate(wallPrefab, pos, rot, wallsContainer ? wallsContainer : transform);
        return w.GetComponentInChildren<Renderer>();
    }

    #endregion WorldGeneration

    #region Helpers
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

        origin -= new Vector3(addLeft * tileSize, 0f, addDown * tileSize);

        width = newWidth;
        height = newHeight;

        var clearState = new ClearState
        {
            OccupiedCells = occupied.ToDictionary(c => c, c => true)
        };

        Generate();
        ReregisterAllInteractables(clearState);
    }



    public bool IsOccupied(Vector2Int g) => occupied.Contains(g);

    public void SetOccupied(IEnumerable<Vector2Int> cells, bool value)
    {
        foreach (var c in cells)
        {
            if (value) occupied.Add(c);
            else occupied.Remove(c);
        }
    }

    public bool TryGetTile(Vector2Int g, out Floor t) => Tiles.TryGetValue(g, out t);

    public bool IsWalkable(Vector2Int c) => IsInside(c) && !IsOccupied(c);

    public void ClearAllTints()
    {
        foreach (var kv in Tiles) kv.Value.SetTint(null);
    }

    public float GetFloorTopY(Vector2Int cell)
    {
        if (TryGetTile(cell, out var f))
        {
            var r = f.GetComponentInChildren<Renderer>();
            if (r) return r.bounds.max.y;
        }

        return 0f;
    }

    public void ReregisterAllInteractables(ClearState clearState = null)
    {
        if (clearState != null)
        {
            foreach (var kv in clearState.OccupiedCells)
            {
                if (kv.Value) occupied.Add(kv.Key);
            }
        }

        interactablesByCell.Clear();
        cellsByInteractable.Clear();

        var all = FindObjectsOfType<MonoBehaviour>();
        foreach (var mb in all)
        {
            if (mb is not IInteractable interactable)
                continue;

            Vector3 pos = mb.transform.position;
            Vector2Int cell = WorldToGrid(pos);

            if (IsInside(cell))
            {
                RegisterInteractableCells(new[] { cell }, interactable);
            }
        }
    }

    public float GetObjectExtentsY(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();

        if (rs.Length == 0) return 0f;

        var b = rs[0].bounds;

        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b.extents.y;
    }

    #endregion Helpers

    #region Interactables

    public void RegisterInteractableCells(IEnumerable<Vector2Int> cells, IInteractable interactable)
    {
        if (interactable == null || cells == null) return;

        if (!cellsByInteractable.TryGetValue(interactable, out var list))
        {
            list = new List<Vector2Int>();
            cellsByInteractable[interactable] = list;
        }

        foreach (var c in cells)
        {
            if (!interactablesByCell.TryGetValue(c, out var l))
            {
                l = new List<IInteractable>();
                interactablesByCell[c] = l;
            }

            if (!l.Contains(interactable)) l.Add(interactable);

            if (!list.Contains(c)) list.Add(c);
        }
    }

    public void UnregisterInteractable(IInteractable interactable)
    {
        if (interactable == null) return;

        if (!cellsByInteractable.TryGetValue(interactable, out var list)) return;

        foreach (var c in list)
        {
            if (interactablesByCell.TryGetValue(c, out var l))
            {
                l.Remove(interactable);

                if (l.Count == 0) interactablesByCell.Remove(c);
            }
        }
        cellsByInteractable.Remove(interactable);
    }

    public IEnumerable<IInteractable> GetInteractablesAt(Vector2Int cell)
    {
        return interactablesByCell.TryGetValue(cell, out var l) ? l : Enumerable.Empty<IInteractable>();
    }

    #endregion Interactables

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

public class ClearState
{
    public Dictionary<Vector2Int, bool> OccupiedCells = new();
}