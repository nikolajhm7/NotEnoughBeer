using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // metadata
    public string Version = "0.2.0";
    public string SavedAtIsoUtc;

    // tutorials
    public bool IntroCompleted;
    public bool TutorialShown;

    // grid info
    public int GridWidth, GridHeight;
    public Vector3 Origin;

    // player position
    public int PlayerX, PlayerY;
    public int PlayerFacingIndex;

    // currency
    public int CurrencyAmount;

    // machines (existing)
    public List<MachineRecord> Machines = new();

    // NEW: pocket inventory
    public int PocketCapacity = 0;
    public List<ItemStack> PocketItems = new();

    // NEW: container inventories
    public List<ContainerSave> Containers = new();

    public int TotalSales;
}

[Serializable]
public class MachineRecord
{
    public string DefinitionId;
    public int AnchorX, AnchorY;
    public int FacingIndex;
    public float YOffset;
}

/// <summary>
/// Simple stack record for saving inventory.
/// Must match your Inventory.cs ItemStack (same fields).
/// </summary>
[Serializable]



public class ContainerSave
{
    // identify which placed machine this is
    public string DefinitionId;
    public int AnchorX, AnchorY;
    public int FacingIndex;

    // container settings/data
    public int Capacity;
    public List<ItemStack> Items = new();
}
