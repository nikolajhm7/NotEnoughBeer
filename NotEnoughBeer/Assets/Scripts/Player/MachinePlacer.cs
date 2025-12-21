using System;
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

    int manualRotation = 0;
    bool isEquipped = false;

    void Update()
    {
        var kb = Keyboard.current; var mouse = Mouse.current;
        if (kb == null || mouse == null || grid == null || mover == null) return;

        if (kb.bKey.wasPressedThisFrame)
        {
            if (isEquipped)
            {
                Unequip();
            }
            else
            {
                Equip(defaultMachine);
            }
        }

        if (kb.rKey.wasPressedThisFrame)
        {
            Debug.Log("Rotating placement preview.");
            manualRotation = (manualRotation + 1) % 4;
        }

        if (currentDef != null)
        {
            UpdatePreviewAndHighlights();

            if (kb.spaceKey.wasPressedThisFrame || mouse.leftButton.wasPressedThisFrame)
                TryPlace();
        }
    }

   
    public void EquipFromUI(MachineDefinition def)
    {
        Equip(def);
    }


    #region Equip/Unequip

    void Equip(MachineDefinition def)
    {
        ClearHighlights();
        currentDef = def;
        if (!currentDef || !currentDef.prefab) { Debug.LogWarning("No machine to equip."); return; }

        if (ghost) Destroy(ghost);
        ghost = Instantiate(currentDef.prefab);
        SetGhostVisual(ghost, true);

        isEquipped = true;
    }

    void Unequip()
    {
        ClearHighlights();
        currentDef = null;
        if (ghost) Destroy(ghost);
        ghost = null;

        isEquipped = false;
    }

    void UpdatePreviewAndHighlights()
    {
        var anchor = mover.CurrentCell + mover.CurrentFacingDir;
        int fi = (FacingToIndex(mover.CurrentFacingDir) + manualRotation) % 4;

        // position/rotate ghost
        if (ghost)
        {
            ghost.transform.rotation = Quaternion.Euler(0f, fi * 90f, 0f);

            float y = grid.GetFloorTopY(anchor) + grid.GetObjectExtentsY(ghost);

            Vector3 baseXZ = grid.GridToWorld(anchor);
            Vector2 center = GetFootprintCenter(currentDef.occupiedOffsets, fi);

            Vector3 worldOffset = new(center.x * grid.tileSize,
                                              0f,
                                              center.y * grid.tileSize);

            Vector3 finalPos = baseXZ + worldOffset;
            finalPos.y = y;

            ghost.transform.position = finalPos;
        }

        // compute cells
        BuildRotatedCells(currentDef.occupiedOffsets, anchor, fi, occCells);
        BuildRotatedCells(currentDef.affectedOffsets, anchor, fi, affCells);
        FindInvalidOccupied(occCells, invalidOcc);

        // draw highlights (clear once per frame, then layer colors)
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

    #endregion Equip/Unequip

    #region Placement
    bool TryPlace()
    {
        var anchor = mover.CurrentCell + mover.CurrentFacingDir;
        int fi = (FacingToIndex(mover.CurrentFacingDir) + manualRotation) % 4;

        var baseXZ = grid.GridToWorld(anchor);

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

        Place(anchor, baseXZ, fi);

        Unequip();

        return true;
    }

    void Place(Vector2Int anchor, Vector3 baseXZ, int facingIndex)
    {
        Currency.Instance.SpendCurrency(currentDef.cost);


        // place
        var go = Instantiate(currentDef.prefab,
                             baseXZ,
                             Quaternion.Euler(0f, facingIndex * 90f, 0f));

        float placeY = grid.GetFloorTopY(anchor) + grid.GetObjectExtentsY(go);

        Vector2 center = GetFootprintCenter(currentDef.occupiedOffsets, facingIndex);

        Vector3 worldOffset = new(center.x * grid.tileSize,
                                          0f,
                                          center.y * grid.tileSize);

        Vector3 finalPos = baseXZ + worldOffset;
        finalPos.y = placeY;

        go.transform.position = finalPos;

        var inst = go.AddComponent<MachineInstance>();
        inst.def = currentDef;
        inst.anchorCell = anchor;
        inst.facingIndex = facingIndex;
        inst.YOffset = placeY - baseXZ.y;

        inst.occupiedCells.AddRange(occCells);
        BuildRotatedCells(currentDef.affectedOffsets, anchor, facingIndex, affCells);
        inst.affectedCells.AddRange(affCells);

        grid.SetOccupied(occCells, true);

        var interactCells = inst.affectedCells.Count > 0 ? inst.affectedCells : new List<Vector2Int> { anchor };
        var interactables = go.GetComponentsInChildren<IInteractable>();

        foreach (var interactable in interactables)
        {
            grid.RegisterInteractableCells(interactCells, interactable);
        }

        SFXManager.Instance.Play(SFX.PlaceMachine);
    }

    #endregion Placement

    #region Helpers
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

    Bounds GetPrefabXZBounds(GameObject prefab)
    {
        var renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers)
            b.Encapsulate(r.bounds);

        return b;
    }

    Vector2 GetFootprintCenter(Vector2Int[] offsets, int facingIndex)
    {
        float sumX = 0f, sumY = 0f;

        foreach (var offset in offsets)
        {
            Vector2Int v = offset;
            for (int i = 0; i < facingIndex; i++)
            {
                v = new Vector2Int(v.y, -v.x); // 90° cw
            }

            sumX += v.x;
            sumY += v.y;
        }

        int count = offsets.Length;
        return new Vector2(sumX / count, sumY / count);
    }
    #endregion Helpers
}
