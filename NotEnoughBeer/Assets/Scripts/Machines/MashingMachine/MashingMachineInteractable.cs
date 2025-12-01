using UnityEngine;

public class MashingMachineInteractable : BrewingMachineBase
{
    protected override Batch.Stage RequiredStage => Batch.Stage.Mashing;

    protected override bool HasIngredients()
    {
        // No extra ingredients for now – always ok
        return true;
    }

    protected override void ConsumeIngredients()
    {
        // Later: consume water/fuel if you add them
    }
}
