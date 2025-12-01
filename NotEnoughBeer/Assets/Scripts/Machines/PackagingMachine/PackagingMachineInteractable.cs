using UnityEngine;

public class PackagingMachineInteractable : BrewingMachineBase
{
    [Header("Packaging Settings")]
    public int bottlesPerBatch = 12;

    protected override Batch.Stage RequiredStage => Batch.Stage.Packaging;

    protected override bool HasIngredients()
    {
        if (IngredientStorage.Instance == null) return true;
        return IngredientStorage.Instance.Bottles >= bottlesPerBatch;
    }

    protected override void ConsumeIngredients()
    {
        IngredientStorage.Instance?.UseBottles(bottlesPerBatch);
    }

    protected override void OnStageFinished()
    {
        if (BeerStorage.Instance == null)
        {
            Debug.LogWarning("[Packaging] BeerStorage.Instance is null.");
            return;
        }

        var rarity = BrewingBatchManager.Instance.GetRarity();
        BeerStorage.Instance.AddBeer(bottlesPerBatch, rarity);

        Debug.Log($"[Packaging] Finished batch ({rarity}) and added {bottlesPerBatch} bottles.");
    }
}
