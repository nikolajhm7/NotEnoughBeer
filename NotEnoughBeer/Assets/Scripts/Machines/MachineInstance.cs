using System.Collections.Generic;
using UnityEngine;

public class MachineInstance : MonoBehaviour
{
    public MachineDefinition def;
    public Vector2Int anchorCell;
    public int facingIndex;
    public List<Vector2Int> occupiedCells = new();
    public List<Vector2Int> affectedCells = new();

    // persisted yOffset for save/load
    public float YOffset;
}
