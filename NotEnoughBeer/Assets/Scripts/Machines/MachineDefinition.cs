using UnityEngine;

[CreateAssetMenu(menuName="Machines/Definition", fileName="MachineDefinition")]
public class MachineDefinition : ScriptableObject
{
    public string id = "machine";
    public GameObject prefab;

    [Header("Footprint (grid offsets occupied by the machine)")]
    public Vector2Int[] occupiedOffsets = { Vector2Int.zero };

    [Header("Affected tiles (relative to machine forward)")]
    public Vector2Int[] affectedOffsets = { new(0,1), new(0,2), new(0,3) }; // default 3 tiles in front of the palyer

    [Header("Preview tint colors")]
    public Color occupiedOk   = new(0.35f, 0.9f, 0.35f);
    public Color occupiedBad  = new(0.9f, 0.35f, 0.35f);
    public Color affectedTint = new(0.35f, 0.7f, 1f, 0.9f);
}
