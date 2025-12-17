using UnityEngine;

public class FermentationMachineInteractable : BrewingMachineBase
{
    [Header("Fermentation Settings")]
    public int yeastPerBatch = 1;

    protected override Batch.Stage RequiredStage => Batch.Stage.Fermentation;

    protected override bool HasIngredients()
    {
        return PocketInventory.Instance != null &&
               PocketInventory.Instance.Inv.Get(ItemId.Yeast) >= yeastPerBatch;
    }

    protected override void ConsumeIngredients()
    {
        PocketInventory.Instance.Inv.TryRemove(ItemId.Yeast, yeastPerBatch);
    }
}
