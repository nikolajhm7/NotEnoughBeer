using System.Collections.Generic;
using UnityEngine;

public class WallCuller : MonoBehaviour
{
    public GridManager grid;
    public PlayerMovement mover;

    List<Renderer> lastHiddenGroup;

    void Reset()
    {
        if (!grid) grid = FindObjectOfType<GridManager>();
        if (!mover) mover = GetComponent<PlayerMovement>();
    }

    void LateUpdate()
    {
        if (!grid || !mover) return;

        Vector2Int fwd = mover.CurrentFacingDir;
        Vector2Int back = -fwd;

        List<Renderer> group = GetPerimeterGroupForDirection(back);

        if (lastHiddenGroup != null)
            SetGroup(groupRenderers: lastHiddenGroup, enabledState: true);

        if (group != null)
        {
            SetGroup(group, false);
            lastHiddenGroup = group;
        }
        else
        {
            lastHiddenGroup = null;
        }
    }

    List<Renderer> GetPerimeterGroupForDirection(Vector2Int d)
    {
        if (d == Vector2Int.up)    return grid.northWalls;
        if (d == Vector2Int.down)  return grid.southWalls;
        if (d == Vector2Int.right) return grid.eastWalls;
        if (d == Vector2Int.left)  return grid.westWalls;
        return null;
    }

    static void SetGroup(List<Renderer> groupRenderers, bool enabledState)
    {
        foreach (var r in groupRenderers)
            if (r) r.enabled = enabledState;
    }
}
