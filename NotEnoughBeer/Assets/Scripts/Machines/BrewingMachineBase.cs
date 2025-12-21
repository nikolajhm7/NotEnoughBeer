using UnityEngine;

public abstract class BrewingMachineBase : MonoBehaviour, IInteractable
{
    [Header("Interaction Texts")]
    [TextArea] public string interactionDescription = "Use machine";
    [TextArea] public string missingIngredientsDescription = "You don't have the required ingredients.";
    [TextArea] public string wrongStageDescription = "The batch is not ready for this step.";

    [Header("Minigame / Settings")]
    public int priority = 0;
    public string minigameSceneName;          
    public float autoScoreIfNoMinigame = 20f; 

    public int Priority => priority;

    public string GetInteractionDescription(PlayerInteractor ctx)
    {
        if (!HasIngredients())
            return missingIngredientsDescription;

        if (!IsCorrectStage())
            return wrongStageDescription;

        return interactionDescription;
    }

    
    public virtual bool CanInteract(PlayerInteractor ctx) => true;

    public void Interact(PlayerInteractor ctx)
    {
        if (!HasIngredients())
        {
            Debug.Log($"[{name}] Missing ingredients.");
            return;
        }

        if (!IsCorrectStage())
        {
            Debug.Log($"[{name}] Wrong stage for this machine.");
            return;
        }

        var manager = BrewingBatchManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning($"[{name}] No BrewingBatchManager in scene.");
            return;
        }

        if (RequiredStage == Batch.Stage.Malting && !manager.HasActiveBatch)
        {
            manager.StartNewBatch();
        }

        ConsumeIngredients();

        if (!string.IsNullOrEmpty(minigameSceneName) && MinigameBridge.Instance != null)
        {
            MinigameBridge.Instance.StartMinigame(minigameSceneName, OnMinigameComplete);
        }
        else
        {
            Debug.Log($"[{name}] No minigame set, auto-completing with score {autoScoreIfNoMinigame}");
            OnMinigameComplete(autoScoreIfNoMinigame);
        }
    }

    void OnMinigameComplete(float score)
    {
        var manager = BrewingBatchManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning($"[{name}] No BrewingBatchManager when finishing stage.");
            return;
        }

        manager.AddScore(score);
        manager.AdvanceStage();
        OnStageFinished();
    }

    bool IsCorrectStage()
    {
        var manager = BrewingBatchManager.Instance;

        
        if (manager == null)
            return RequiredStage == Batch.Stage.Malting; 

        
        if (RequiredStage == Batch.Stage.Malting)
            return true;

        var batch = manager.CurrentBatch;
        if (batch == null || batch.IsFinished) return false;

        return batch.CurrentStage == RequiredStage;
    }

  


    protected abstract Batch.Stage RequiredStage { get; }
    protected abstract bool HasIngredients();
    protected abstract void ConsumeIngredients();
    protected virtual void OnStageFinished() { }
}
