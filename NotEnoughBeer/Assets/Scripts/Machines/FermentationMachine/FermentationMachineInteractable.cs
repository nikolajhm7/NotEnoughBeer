using UnityEngine;

public class FermentationMachineInteractable : BrewingMachineBase
{
    [Header("Fermentation Settings")]
    public int yeastPerBatch = 1;

    protected override Batch.Stage RequiredStage => Batch.Stage.Fermentation;

    protected override bool HasIngredients()
    {
        // Until you properly buy yeast, you can temporarily just return true.
        if (IngredientStorage.Instance == null) return true;

        return IngredientStorage.Instance.Yeast >= yeastPerBatch;
    }

    protected override void ConsumeIngredients()
    {
        IngredientStorage.Instance?.UseYeast(yeastPerBatch);
    }
}
