using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform contentRoot;
    public InventoryItemRow rowPrefab;

    Dictionary<ItemId, InventoryItemRow> rows =
        new Dictionary<ItemId, InventoryItemRow>();

    void Start()
    {
        var inv = PocketInventory.Instance.Inv;
        inv.OnChanged += OnInventoryChanged;

        RefreshAll();
    }

    void OnDestroy()
    {
        if (PocketInventory.Instance != null)
            PocketInventory.Instance.Inv.OnChanged -= OnInventoryChanged;
    }

    void RefreshAll()
    {
        foreach (var stack in PocketInventory.Instance.Inv.ToStacks())
        {
            OnInventoryChanged(stack.Id, stack.Amount);
        }
    }

    void OnInventoryChanged(ItemId id, int amount)
    {
        if (amount <= 0)
        {
            if (rows.TryGetValue(id, out var row))
            {
                Destroy(row.gameObject);
                rows.Remove(id);
            }
            return;
        }

        if (!rows.TryGetValue(id, out var itemRow))
        {
            itemRow = Instantiate(rowPrefab, contentRoot);
            rows[id] = itemRow;
        }

        itemRow.Set(id, amount);
    }
}
