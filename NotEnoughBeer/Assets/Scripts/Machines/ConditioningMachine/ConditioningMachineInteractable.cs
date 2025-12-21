using UnityEngine;

public class ConditioningMachineInteractable : BrewingMachineBase
{
    protected override Batch.Stage RequiredStage => Batch.Stage.Conditioning;

    protected override bool HasIngredients()
    {
       
        return true;
    }

    protected override void ConsumeIngredients()
    {
        
    }
}
