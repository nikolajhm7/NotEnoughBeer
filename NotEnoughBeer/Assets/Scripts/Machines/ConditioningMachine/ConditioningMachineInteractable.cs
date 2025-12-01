using UnityEngine;

public class ConditioningMachineInteractable : BrewingMachineBase
{
    protected override Batch.Stage RequiredStage => Batch.Stage.Conditioning;

    protected override bool HasIngredients()
    {
        // No extra ingredients for now
        return true;
    }

    protected override void ConsumeIngredients()
    {
        // Later: additives, etc.
    }
}
