using UnityEngine;

public class BeerMaker9000Interactable : MonoBehaviour, IInteractable
{
    [TextArea] public string InteractionDescription = "Brew (bad) beer with BeerMaker 9000";
    public int priority = 0;

    [Header("Ingredients per batch")]
    public int barleyPerBatch = 5;
    public int yeastPerBatch = 1;
    public int bottlesPerBatch = 12;

    [Header("Quality range (0–100)")]
    public float minQuality = 5f;
    public float maxQuality = 60f;

    public int Priority => priority;
    public string GetInteractionDescription(PlayerInteractor context) => InteractionDescription;
    public bool CanInteract(PlayerInteractor context) => true;

    public void Interact(PlayerInteractor context)
    {
        if (PocketInventory.Instance == null)
        {
            NotificationService.Instance?.Show("Missing PocketInventory.");
            return;
        }

        var inv = PocketInventory.Instance.Inv;

        if (inv.Get(ItemId.Barley) < barleyPerBatch ||
            inv.Get(ItemId.Yeast) < yeastPerBatch ||
            inv.Get(ItemId.Bottles) < bottlesPerBatch)
        {
            NotificationService.Instance?.Show("Not enough ingredients to brew beer.");
            return;
        }

        
        inv.TryRemove(ItemId.Barley, barleyPerBatch);
        inv.TryRemove(ItemId.Yeast, yeastPerBatch);
        inv.TryRemove(ItemId.Bottles, bottlesPerBatch);

        float quality = Random.Range(minQuality, maxQuality);
        var rarity = QualityToRarity(quality);
        var beerId = BeerItemForRarity(rarity);

      
        if (inv.TryAdd(beerId, bottlesPerBatch))
        {
            NotificationService.Instance?.Show($"+{bottlesPerBatch} {rarity} beer (quality {quality:F1})", MapRarityToColor(rarity));
        }
        else
        {
            
            var container = FindFirstContainerWithSpace(bottlesPerBatch);
            if (container != null && container.Inv.TryAdd(beerId, bottlesPerBatch))
            {
                NotificationService.Instance?.Show($"+{bottlesPerBatch} {rarity} beer → container", MapRarityToColor(rarity));
            }
            else
            {
                NotificationService.Instance?.Show("No space for beer! Pocket + containers are full.");
                
                inv.TryAdd(ItemId.Barley, barleyPerBatch);
                inv.TryAdd(ItemId.Yeast, yeastPerBatch);
                inv.TryAdd(ItemId.Bottles, bottlesPerBatch);
                return;
            }
        }

        if (SFXManager.Instance != null) SFXManager.Instance.Play(SFX.Pop);
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

    BeerStorage.BeerRarity QualityToRarity(float q)
    {
        if (q >= 90) return BeerStorage.BeerRarity.Legendary;
        if (q >= 70) return BeerStorage.BeerRarity.Mythical;
        if (q >= 50) return BeerStorage.BeerRarity.Rare;
        if (q >= 30) return BeerStorage.BeerRarity.Uncommon;
        return BeerStorage.BeerRarity.Common;
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

    Color MapRarityToColor(BeerStorage.BeerRarity rarity)
    {
        switch (rarity)
        {
            case BeerStorage.BeerRarity.Common: return Color.gray;
            case BeerStorage.BeerRarity.Uncommon: return Color.green;
            case BeerStorage.BeerRarity.Rare: return Color.blue;
            case BeerStorage.BeerRarity.Mythical: return Color.magenta;
            case BeerStorage.BeerRarity.Legendary: return new Color(1f, 0.5f, 0f);
            default: return Color.white;
        }
    }
}
