using UnityEngine;

public class MaltingMachineInteractable : BrewingMachineBase
{
    [Header("Malting Settings")]
    public int barleyPerBatch = 5;

    protected override Batch.Stage RequiredStage => Batch.Stage.Malting;

    protected override bool HasIngredients()
    {
        return IngredientStorage.Instance != null &&
               IngredientStorage.Instance.Barley >= barleyPerBatch;
    }

    protected override void ConsumeIngredients()
    {
        IngredientStorage.Instance?.UseBarley(barleyPerBatch);
    }
}
