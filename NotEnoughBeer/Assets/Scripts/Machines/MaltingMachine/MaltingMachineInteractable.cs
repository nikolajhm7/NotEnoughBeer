using UnityEngine;

public class MaltingMachineInteractable : BrewingMachineBase
{
    [Header("Malting Settings")]
    public int barleyPerBatch = 5;

    protected override Batch.Stage RequiredStage => Batch.Stage.Malting;

    protected override bool HasIngredients()
    {
        return PocketInventory.Instance != null &&
               PocketInventory.Instance.Inv.Get(ItemId.Barley) >= barleyPerBatch;
    }

    protected override void ConsumeIngredients()
    {
        PocketInventory.Instance.Inv.TryRemove(ItemId.Barley, barleyPerBatch);
    }
}
