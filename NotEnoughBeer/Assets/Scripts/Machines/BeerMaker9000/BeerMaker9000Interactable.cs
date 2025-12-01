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
    public float maxQuality = 60f; // < 70 => no Mythical/Legendary

    public int Priority => priority;
    public string GetInteractionDescription(PlayerInteractor context) => InteractionDescription;
    public bool CanInteract(PlayerInteractor context) => true;

    public void Interact(PlayerInteractor context)
    {
        if (IngredientStorage.Instance == null || BeerStorage.Instance == null)
        {
            Debug.LogWarning("[BeerMaker9000] Missing IngredientStorage or BeerStorage.");
            return;
        }

        var ing = IngredientStorage.Instance;

        if (ing.Barley < barleyPerBatch ||
            ing.Yeast < yeastPerBatch ||
            ing.Bottles < bottlesPerBatch)
        {
            Debug.Log("[BeerMaker9000] Not enough ingredients (needs barley, yeast and bottles).");
            return;
        }

        // Consume ingredients
        ing.UseBarley(barleyPerBatch);
        ing.UseYeast(yeastPerBatch);
        ing.UseBottles(bottlesPerBatch);

        // Roll a worse quality than the real pipeline can get
        float quality = Random.Range(minQuality, maxQuality);

        var rarity = QualityToRarity(quality);

        BeerStorage.Instance.AddBeer(bottlesPerBatch, rarity);

        Debug.Log($"[BeerMaker9000] Brewed {bottlesPerBatch} bottles of {rarity} beer (quality {quality:F1}).");
    }

    BeerStorage.BeerRarity QualityToRarity(float q)
    {
        // Same thresholds as BrewingBatchManager, but BeerMaker9000 can't even reach the top end
        if (q >= 90) return BeerStorage.BeerRarity.Legendary;
        if (q >= 70) return BeerStorage.BeerRarity.Mythical;
        if (q >= 50) return BeerStorage.BeerRarity.Rare;
        if (q >= 30) return BeerStorage.BeerRarity.Uncommon;
        return BeerStorage.BeerRarity.Common;
    }
}
