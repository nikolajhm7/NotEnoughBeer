using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MachinePlacer : MonoBehaviour
{
    [Header("Refs")]
    public GridManager grid;
    public PlayerMovement mover;

    [Header("Placement")]
    public MachineDefinition defaultMachine;
    public float previewHeight = 0.5f;
    public float previewAlpha = 0.6f;

    GameObject ghost;
    MachineDefinition currentDef;

    readonly List<Vector2Int> occCells = new();
    readonly List<Vector2Int> affCells = new();
    readonly List<Vector2Int> invalidOcc = new();

    void Update()
    {
        var kb = Keyboard.current; var mouse = Mouse.current;
        if (kb == null || mouse == null || grid == null || mover == null) return;

        if (kb.fKey.wasPressedThisFrame) Equip(defaultMachine);

        if (kb.gKey.wasPressedThisFrame && Currency.Instance != null)
        {
            Currency.Instance?.AddCurrency(10);
        }

        if (currentDef != null)
        {
            UpdatePreviewAndHighlights();

            if (kb.spaceKey.wasPressedThisFrame || mouse.leftButton.wasPressedThisFrame)
                TryPlace();
        }
    }

    void Equip(MachineDefinition def)
    {
        ClearHighlights();
        currentDef = def;
        if (!currentDef || !currentDef.prefab) { Debug.LogWarning("No machine to equip."); return; }

        if (ghost) Destroy(ghost);
        ghost = Instantiate(currentDef.prefab);
        SetGhostVisual(ghost, true);
    }

    void Unequip()
    {
        ClearHighlights();
        currentDef = null;
        if (ghost) Destroy(ghost);
        ghost = null;
    }

    void UpdatePreviewAndHighlights()
    {
        var anchor = mover.CurrentCell + mover.CurrentFacingDir;
        int fi = FacingToIndex(mover.CurrentFacingDir);

        // position/rotate ghost
        if (ghost)
        {
            ghost.transform.position = grid.GridToWorld(anchor) + Vector3.up * previewHeight;
            ghost.transform.rotation = Quaternion.Euler(0f, fi * 90f, 0f);
        }

        // --- compute cells
        BuildRotatedCells(currentDef.occupiedOffsets, anchor, fi, occCells);
        BuildRotatedCells(currentDef.affectedOffsets, anchor, fi, affCells);
        FindInvalidOccupied(occCells, invalidOcc);

        // --- draw highlights (clear once per frame, then layer colors)
        grid.ClearAllTints();

        // Occupied footprint tiles
        foreach (var c in occCells)
        {
            bool bad = invalidOcc.Contains(c);
            if (grid.TryGetTile(c, out var f))
            {
                // red if invalid, green if valid
                f.SetTint(bad ? currentDef.occupiedBad : currentDef.occupiedOk);
            }
        }

        // Affected tiles (only apply if NOT already a footprint tile)
        foreach (var c in affCells)
        {
            if (occCells.Contains(c)) continue; // skip footprint
            if (grid.IsInside(c) && grid.TryGetTile(c, out var f))
            {
                f.SetTint(currentDef.affectedTint);
            }
        }
    }

    bool TryPlace()
    {
        var anchor = mover.CurrentCell + mover.CurrentFacingDir;
        int fi = FacingToIndex(mover.CurrentFacingDir);

        BuildRotatedCells(currentDef.occupiedOffsets, anchor, fi, occCells);
        invalidOcc.Clear();
        FindInvalidOccupied(occCells, invalidOcc);

        if (invalidOcc.Count > 0) return false;
        
        if (HasEnoughGoldForCurrent() == false) 
        {
            Debug.Log("Not enough money to place " + currentDef.id + $" (cost: {currentDef.cost}).");

            Unequip();
            return false;
        }

        Currency.Instance.SpendCurrency(currentDef.cost);

        // place
        var go = Instantiate(currentDef.prefab,
                             grid.GridToWorld(anchor),
                             Quaternion.Euler(0f, fi * 90f, 0f));

        var inst = go.AddComponent<MachineInstance>();
        inst.def = currentDef;
        inst.anchorCell = anchor;
        inst.facingIndex = fi;
        inst.occupiedCells.AddRange(occCells);
        BuildRotatedCells(currentDef.affectedOffsets, anchor, fi, affCells);
        inst.affectedCells.AddRange(affCells);

        grid.SetOccupied(occCells, true);

        Unequip();

        return true;
    }

    // ----------------- helpers -----------------

    bool HasEnoughGoldForCurrent()
        => Currency.Instance != null && currentDef != null && Currency.Instance.CurrencyAmount >= currentDef.cost;


    void BuildRotatedCells(Vector2Int[] offsets, Vector2Int anchor, int facingIndex, List<Vector2Int> outList)
    {
        outList.Clear();
        foreach (var o in offsets)
        {
            Vector2Int v = o;
            for (int i = 0; i < ((facingIndex % 4) + 4) % 4; i++)
                v = new Vector2Int(v.y, -v.x); // 90° cw
            outList.Add(anchor + v);
        }
    }

    void FindInvalidOccupied(List<Vector2Int> cells, List<Vector2Int> invalid)
    {
        invalid.Clear();
        foreach (var c in cells)
            if (!grid.IsInside(c) || grid.IsOccupied(c))
                invalid.Add(c);
    }

    void SetGhostVisual(GameObject go, bool isGhost)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                if (isGhost && mat.HasProperty("_Color"))
                {
                    var c = mat.color; c.a = previewAlpha; mat.color = c;
                }
            }
        }
    }

    void ClearHighlights() => grid.ClearAllTints();

    int FacingToIndex(Vector2Int dir)
        => (dir == Vector2Int.up) ? 0 :
           (dir == Vector2Int.right) ? 1 :
           (dir == Vector2Int.down) ? 2 : 3;
}
