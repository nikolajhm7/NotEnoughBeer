using UnityEngine;

public class MashingMachineInteractable : BrewingMachineBase
{
    protected override Batch.Stage RequiredStage => Batch.Stage.Mashing;

    protected override bool HasIngredients()
    {
        
        return true;
    }

    protected override void ConsumeIngredients()
    {
        
    }
}
