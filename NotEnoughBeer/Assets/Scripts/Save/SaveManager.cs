using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Buffers.Text;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("New Game Settings")]
    public string StarterMachineId = "computer";
    public MachineDefinition StarterMachineDef;
    public int StartingCurrency = 100;

    // References
    public GridManager Grid;
    public PlayerMovement Player;
    public Currency Currency;
    public List<MachineDefinition> MachineDefinitions;

    // Save slots
    public static int CurrentSlot = 1;
    public bool AutoLoadOnStart = true;
    public int TotalSales { get; private set; }

    // Data
    Dictionary<string, MachineDefinition> DefinitionsById;
    public bool IntroCutscenePlayed { get; private set; }
    public bool TutorialShown { get; private set; }

    private void Awake()
    {
        Instance = this;

        DefinitionsById = new Dictionary<string, MachineDefinition>();

        foreach (var definition in MachineDefinitions)
        {
            if (!string.IsNullOrEmpty(definition.id))
            {
                DefinitionsById[definition.id] = definition;
            }
        }

        if (Grid) Grid.generateOnPlay = false;

        if (AutoLoadOnStart)
        {
            Load();
        }
    }


    public void AddSales(int amount)
    {
        if (amount <= 0) return;
        TotalSales += amount;
    }
    public void SaveAndReload()
    {
        Save();
        Load();
    }

    string SavePath => GetSlotPath(CurrentSlot);
    public static string GetSlotPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot{Mathf.Clamp(slot, 1, 3)}.json");

    public void Save()
    {
        var data = new SaveData
        {
            // Use roundtrip-ish format so parsing is stable
            SavedAtIsoUtc = DateTime.UtcNow.ToString("o"),

            IntroCompleted = IntroCutscenePlayed,

            GridWidth = Grid.width,
            GridHeight = Grid.height,
            Origin = Grid.origin,

            PlayerX = Player.CurrentCell.x,
            PlayerY = Player.CurrentCell.y,
            PlayerFacingIndex = Player.FacingIndex,

            CurrencyAmount = Currency ? Currency.CurrencyAmount : 0,

            // NEW: pocket inventory
            PocketCapacity = PocketInventory.Instance ? PocketInventory.Instance.capacity : 0,
            PocketItems = PocketInventory.Instance ? PocketInventory.Instance.Inv.ToStacks() : new List<ItemStack>(),

            // NEW: containers (filled below)
            Containers = new List<ContainerSave>(),
            TotalSales = TotalSales
        };

        var machineInstances = FindObjectsByType<MachineInstance>(FindObjectsSortMode.None);

        foreach (var machineInstance in machineInstances)
        {
            string defId = machineInstance.def ? machineInstance.def.id : "";

            data.Machines.Add(new MachineRecord
            {
                DefinitionId = defId,
                AnchorX = machineInstance.anchorCell.x,
                AnchorY = machineInstance.anchorCell.y,
                FacingIndex = machineInstance.facingIndex,
                YOffset = machineInstance.YOffset,
            });

            // NEW: if this machine contains a StorageContainer, save its inventory
            var container = machineInstance.GetComponentInChildren<StorageContainer>();
            if (container != null && container.Inv != null)
            {
                data.Containers.Add(new ContainerSave
                {
                    DefinitionId = defId,
                    AnchorX = machineInstance.anchorCell.x,
                    AnchorY = machineInstance.anchorCell.y,
                    FacingIndex = machineInstance.facingIndex,

                    Capacity = container.capacity,
                    Items = container.Inv.ToStacks()
                });
            }
        }

        var json = JsonUtility.ToJson(data, prettyPrint: true);

        Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
        File.WriteAllText(SavePath, json);

        Debug.Log($"[Save] Slot {CurrentSlot}: {data.Machines.Count} machines, {data.Containers.Count} containers -> {SavePath}");
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log($"[Load] Slot {CurrentSlot} empty (no file). Starting fresh.");

            InitializeFreshWorld();
            Save();

            return;
        }

        ClearAllMachinesAndOccupancy();

        var json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<SaveData>(json);
        TotalSales = data.TotalSales;
        IntroCutscenePlayed = data.IntroCompleted;
        TutorialShown = data.TutorialShown;

        Grid.Clear();

        Grid.width = data.GridWidth;
        Grid.height = data.GridHeight;
        Grid.origin = data.Origin;

        Grid.Generate();

        // Recreate machines
        foreach (var rec in data.Machines)
        {
            if (!DefinitionsById.TryGetValue(rec.DefinitionId, out var def) || def.prefab == null)
            {
                Debug.LogWarning($"Unknown machine def id '{rec.DefinitionId}', skipping.");
                continue;
            }

            var anchor = new Vector2Int(rec.AnchorX, rec.AnchorY);
            var rot = Quaternion.Euler(0f, rec.FacingIndex * 90f, 0f);
            var pos = Grid.GridToWorld(anchor);

            var baseXZ = Grid.GridToWorld(anchor);

            var go = Instantiate(def.prefab,
                     baseXZ,
                     Quaternion.Euler(0f, rec.FacingIndex * 90f, 0f));

            float placeY = Grid.GetFloorTopY(anchor) + Grid.GetObjectExtentsY(go);

            Vector2 center = GetFootprintCenter(def.occupiedOffsets, rec.FacingIndex);

            Vector3 worldOffset = new(center.x * Grid.tileSize,
                                          0f,
                                          center.y * Grid.tileSize);

            Vector3 finalPos = baseXZ + worldOffset;
            finalPos.y = placeY;

            go.transform.position = finalPos;

            var inst = go.AddComponent<MachineInstance>();
            inst.def = def;
            inst.anchorCell = anchor;
            inst.facingIndex = rec.FacingIndex;
            inst.YOffset = placeY - baseXZ.y;

            // Rebuild occupied/affected cell lists
            var occ = RotateOffsets(def.occupiedOffsets, anchor, rec.FacingIndex);
            var aff = RotateOffsets(def.affectedOffsets, anchor, rec.FacingIndex);
            inst.occupiedCells.AddRange(occ);
            inst.affectedCells.AddRange(aff);

            // Rebuild occupancy map
            Grid.SetOccupied(occ, true);

            var interactCells = inst.affectedCells.Count > 0 ? inst.affectedCells : new List<Vector2Int> { anchor };
            var interactables = go.GetComponentsInChildren<IInteractable>();

            foreach (var ia in interactables)
            {
                Grid.RegisterInteractableCells(interactCells, ia);
            }

            // NEW: restore container contents if this machine has StorageContainer
            var container = go.GetComponentInChildren<StorageContainer>();
            if (container != null && container.Inv != null)
            {
                var saved = FindContainerSave(data, rec.DefinitionId, anchor, rec.FacingIndex);
                if (saved != null)
                {
                    if (saved.Capacity > 0)
                        container.SetCapacity(saved.Capacity);


                    // If your StorageContainer builds Inventory in Awake() using capacity,
                    // you may want a method that rebuilds inventory when capacity changes.
                    // For now: keep current Inventory instance and just load stacks.
                    container.Inv.LoadFromStacks(saved.Items);
                }
            }
        }

        Debug.Log($"Loaded {data.Machines.Count} machines from {SavePath}");

        // Restore Currency
        if (Currency)
        {
            Currency.SetCurrency(data.CurrencyAmount);
        }

        // Restore Player
        if (Player)
        {
            var c = new Vector2Int(
                Mathf.Clamp(data.PlayerX, 0, Grid.width - 1),
                Mathf.Clamp(data.PlayerY, 0, Grid.height - 1)
            );

            Player.TeleportToCell(c);
            Player.SetFacingIndex(data.PlayerFacingIndex);
        }

        // NEW: restore pocket inventory
        if (PocketInventory.Instance != null)
        {
            if (data.PocketCapacity > 0)
                PocketInventory.Instance.SetCapacity(data.PocketCapacity);

            PocketInventory.Instance.Inv.LoadFromStacks(data.PocketItems);
        }
    }

    ContainerSave FindContainerSave(SaveData data, string defId, Vector2Int anchor, int facingIndex)
    {
        if (data == null || data.Containers == null) return null;

        for (int i = 0; i < data.Containers.Count; i++)
        {
            var c = data.Containers[i];
            if (c == null) continue;

            if (c.DefinitionId == defId &&
                c.AnchorX == anchor.x && c.AnchorY == anchor.y &&
                c.FacingIndex == facingIndex)
                return c;
        }

        return null;
    }

    void ClearAllMachinesAndOccupancy()
    {
        // free from grid first
        var instances = FindObjectsByType<MachineInstance>(FindObjectsSortMode.None);
        foreach (var mi in instances)
        {
            Grid.SetOccupied(mi.occupiedCells, false);

            var interactables = mi.GetComponentsInChildren<IInteractable>();
            foreach (var ia in interactables)
            {
                Grid.UnregisterInteractable(ia);
            }
        }

        // then destroy gameobjects
        foreach (var mi in instances)
        {
            Destroy(mi.gameObject);
        }
    }

    void InitializeFreshWorld()
    {
        Grid.width = 5;
        Grid.height = 5;
        Grid.origin = Vector3.zero;
        Grid.Generate();

        MachineDefinition def = null;
        if (StarterMachineDef)
        {
            def = StarterMachineDef;
        }
        else if (!string.IsNullOrEmpty(StarterMachineId) && DefinitionsById != null)
        {
            DefinitionsById.TryGetValue(StarterMachineId, out def);
        }

        if (!def || !def.prefab)
        {
            Debug.LogError("[NewGame] Starter machine definition missing or has no prefab.");
            return;
        }

        var anchor = new Vector2Int(Player.startCell.x, Grid.height - 1);

        var facingIndex = FacingIndexToward(anchor, new Vector2Int(Grid.width / 2, Grid.height / 2));

        // Spawn starter machine
        var pos = Grid.GridToWorld(anchor);
        var rot = Quaternion.Euler(0f, facingIndex * 90f, 0f);
        var go = Instantiate(def.prefab, pos, rot);

        float placeY = Grid.GetFloorTopY(anchor) + Grid.GetObjectExtentsY(go);
        go.transform.position = new(pos.x, placeY, pos.z);

        var inst = go.AddComponent<MachineInstance>();
        inst.def = def;
        inst.anchorCell = anchor;
        inst.facingIndex = facingIndex;
        inst.YOffset = placeY - pos.y;

        var occ = RotateOffsets(def.occupiedOffsets, anchor, facingIndex);
        var aff = RotateOffsets(def.affectedOffsets, anchor, facingIndex);
        inst.occupiedCells.AddRange(occ);
        inst.affectedCells.AddRange(aff);
        Grid.SetOccupied(occ, true);

        var interactCells = inst.affectedCells.Count > 0 ? inst.affectedCells : new List<Vector2Int> { anchor };
        var interactables = go.GetComponentsInChildren<IInteractable>();

        foreach (var ia in interactables)
        {
            Grid.RegisterInteractableCells(interactCells, ia);
        }

        if (Player)
        {
            Player.TeleportToCell(Player.startCell);

            if (Currency) Currency.SetCurrency(StartingCurrency);

            Player.SetFacingIndex(FacingIndexToward(Player.startCell, anchor));
        }

        // NEW: initialize pocket (optional)
        if (PocketInventory.Instance != null)
        {
            // leave whatever is set in inspector, but ensure Inventory exists/capacity correct
            PocketInventory.Instance.SetCapacity(PocketInventory.Instance.capacity);
        }
    }

    // same rotation helper you use in MachinePlacer
    static List<Vector2Int> RotateOffsets(Vector2Int[] offsets, Vector2Int anchor, int facingIndex)
    {
        var list = new List<Vector2Int>(offsets.Length);
        foreach (var o in offsets)
        {
            var v = o;
            for (int i = 0; i < ((facingIndex % 4) + 4) % 4; i++)
                v = new Vector2Int(v.y, -v.x);
            list.Add(anchor + v);
        }
        return list;
    }

    int FacingIndexToward(Vector2Int from, Vector2Int to)
    {
        var d = to - from;

        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            return d.x >= 0 ? 1 : 3; // east or west
        else
            return d.y >= 0 ? 0 : 2; // north or south
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

    public void MarkCutsceneAsPlayed()
    {
        IntroCutscenePlayed = true;
        Save();
    }

    public void MarkTutorialAsShown()
    {
        TutorialShown = true;
        Save();
    }
}
