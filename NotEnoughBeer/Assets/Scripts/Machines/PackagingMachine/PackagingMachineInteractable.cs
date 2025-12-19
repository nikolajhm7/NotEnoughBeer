using UnityEngine;

public class PackagingMachineInteractable : BrewingMachineBase
{
    [Header("Packaging Settings")]
    public int bottlesPerBatch = 12;

    protected override Batch.Stage RequiredStage => Batch.Stage.Packaging;

    protected override bool HasIngredients()
    {
        return PocketInventory.Instance != null &&
               PocketInventory.Instance.Inv.Get(ItemId.Bottles) >= bottlesPerBatch;
    }

    protected override void ConsumeIngredients()
    {
        PocketInventory.Instance.Inv.TryRemove(ItemId.Bottles, bottlesPerBatch);
    }

    protected override void OnStageFinished()
    {
        var bm = BrewingBatchManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("[Packaging] BrewingBatchManager.Instance is null.");
            return;
        }

        var rarity = bm.GetRarity();
        var beerId = BeerItemForRarity(rarity);

        // pocket first
        if (PocketInventory.Instance != null && PocketInventory.Instance.Inv.TryAdd(beerId, bottlesPerBatch))
        {
            Debug.Log($"[Packaging] Added {bottlesPerBatch} {rarity} beer to pocket.");
            return;
        }

        // then containers
        var container = FindFirstContainerWithSpace(bottlesPerBatch);
        if (container != null && container.Inv.TryAdd(beerId, bottlesPerBatch))
        {
            Debug.Log($"[Packaging] Pocket full. Added {bottlesPerBatch} {rarity} beer to container.");
            return;
        }

        NotificationService.Enqueue($"Added {bottlesPerBatch} {rarity} beer!");
        Debug.LogWarning("[Packaging] No space in pocket or containers.");
    }

    static StorageContainer FindFirstContainerWithSpace(int amount)
    {
        if (StorageRegistry.Instance == null) return null;

        foreach (var c in StorageRegistry.Instance.Containers)
        {
            if (!c) continue;
            if (c.Inv != null && c.Inv.CanAdd(amount))
                return c;
        }
        return null;
    }

    static ItemId BeerItemForRarity(BeerStorage.BeerRarity r)
    {
        switch (r)
        {
            case BeerStorage.BeerRarity.Common: return ItemId.Beer_Common;
            case BeerStorage.BeerRarity.Uncommon: return ItemId.Beer_Uncommon;
            case BeerStorage.BeerRarity.Rare: return ItemId.Beer_Rare;
            case BeerStorage.BeerRarity.Mythical: return ItemId.Beer_Mythical;
            case BeerStorage.BeerRarity.Legendary: return ItemId.Beer_Legendary;
            default: return ItemId.Beer_Common;
        }
    }
}
