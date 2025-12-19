using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Inventory
{
    public int Capacity { get; private set; }

    // Unity-safe (no C# 9 new() shorthand)
    private Dictionary<ItemId, int> _counts = new Dictionary<ItemId, int>();

    public event Action<ItemId, int> OnChanged;

    public Inventory(int capacity)
    {
        Capacity = Math.Max(0, capacity);
    }

    public int TotalUnits
    {
        get { return _counts.Values.Sum(); }
    }

    public int FreeUnits
    {
        get { return Math.Max(0, Capacity - TotalUnits); }
    }

    public int Get(ItemId id)
    {
        int v;
        return _counts.TryGetValue(id, out v) ? v : 0;
    }

    public bool CanAdd(int amount)
    {
        return amount <= FreeUnits;
    }

    public bool TryAdd(ItemId id, int amount)
    {
        if (amount <= 0) return false;
        if (!CanAdd(amount)) return false;

        _counts[id] = Get(id) + amount;
        if (OnChanged != null) OnChanged(id, Get(id));
        return true;
    }

    public bool TryRemove(ItemId id, int amount)
    {
        if (amount <= 0) return false;

        int have = Get(id);
        if (have < amount) return false;

        int next = have - amount;
        if (next == 0) _counts.Remove(id);
        else _counts[id] = next;

        if (OnChanged != null) OnChanged(id, Get(id));
        return true;
    }

    // Save/load helpers
    public List<ItemStack> ToStacks()
    {
        var list = new List<ItemStack>();
        foreach (var kv in _counts)
        {
            list.Add(new ItemStack { Id = kv.Key, Amount = kv.Value });
        }
        return list;
    }

    public void LoadFromStacks(IEnumerable<ItemStack> stacks)
    {
        _counts.Clear();
        if (stacks == null) return;

        foreach (var s in stacks)
        {
            if (s.Amount <= 0) continue;
            _counts[s.Id] = s.Amount;
            if (OnChanged != null) OnChanged(s.Id, s.Amount);
        }
    }

    public void SetCapacity(int newCapacity)
    {
        Capacity = Math.Max(0, newCapacity);
    }
}

[Serializable]
public class ItemStack
{
    public ItemId Id;
    public int Amount;
}
