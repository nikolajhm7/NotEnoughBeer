using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // metadata
    public string Version = "0.1.0";
    public string SavedAtIsoUtc;

    // grid info
    public int GridWidth, GridHeight;
    public Vector3 Origin;

    // player position
    public int PlayerX, PlayerY;
    public int PlayerFacingIndex;

    // currency
    public int CurrencyAmount;

    // machines
    public List<MachineRecord> Machines = new();
}

[Serializable]
public class MachineRecord
{
    public string DefinitionId;
    public int AnchorX, AnchorY;
    public int FacingIndex;
    public float YOffset;
}
