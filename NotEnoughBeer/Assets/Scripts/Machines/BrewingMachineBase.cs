using UnityEngine;

public abstract class BrewingMachineBase : MonoBehaviour, IInteractable
{
    [Header("Interaction Texts")]
    [TextArea] public string interactionDescription = "Use machine";
    [TextArea] public string missingIngredientsDescription = "You don't have the required ingredients.";
    [TextArea] public string wrongStageDescription = "The batch is not ready for this step.";

    [Header("Minigame / Settings")]
    public int priority = 0;
    public string minigameSceneName;          // leave empty if you don't have a minigame yet
    public float autoScoreIfNoMinigame = 20f; // score used when no scene is set

    public int Priority => priority;

    public string GetInteractionDescription(PlayerInteractor ctx)
    {
        if (!HasIngredients())
            return missingIngredientsDescription;

        if (!IsCorrectStage())
            return wrongStageDescription;

        return interactionDescription;
    }

    // Always allow highlighting so PlayerInteractor can show text
    public virtual bool CanInteract(PlayerInteractor ctx) => true;

    public void Interact(PlayerInteractor ctx)
    {
        // Block doing anything, but still allow UI text
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

        // Malting starts a new batch
        if (RequiredStage == Batch.Stage.Malting &&
            !BrewingBatchManager.Instance.HasActiveBatch)
        {
            BrewingBatchManager.Instance.StartNewBatch();
        }

        ConsumeIngredients();

        if (!string.IsNullOrEmpty(minigameSceneName) && MinigameBridge.Instance != null)
        {
            MinigameBridge.Instance.StartMinigame(
                minigameSceneName,
                OnMinigameComplete
            );
        }
        else
        {
            // No minigame yet – auto-complete
            Debug.Log($"[{name}] No minigame set, auto-completing with score {autoScoreIfNoMinigame}");
            OnMinigameComplete(autoScoreIfNoMinigame);
        }
    }

    void OnMinigameComplete(float score)
    {
        BrewingBatchManager.Instance.AddScore(score);
        BrewingBatchManager.Instance.AdvanceStage();
        OnStageFinished();
    }

    bool IsCorrectStage()
    {
        // Malting can always start a batch (if ingredients exist)
        if (RequiredStage == Batch.Stage.Malting)
            return true;

        var batch = BrewingBatchManager.Instance?.CurrentBatch;
        if (batch == null || batch.IsFinished) return false;

        return batch.CurrentStage == RequiredStage;
    }

    protected abstract Batch.Stage RequiredStage { get; }
    protected abstract bool HasIngredients();
    protected abstract void ConsumeIngredients();
    protected virtual void OnStageFinished() { }
}
