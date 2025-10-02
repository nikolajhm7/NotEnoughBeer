public interface IInteractable
{
    /// <summary>
    /// Priority of this interactable when multiple are in range. Higher priority interactables are chosen first. (to break ties)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether the player can currently interact with this object.
    /// </summary>
    /// <returns></returns>
    bool CanInteract(PlayerInteractor context);

    /// <summary>
    /// Description of the interaction, shown in the UI when looking at the object.
    /// </summary>
    /// <returns></returns>
    string GetInteractionDescription(PlayerInteractor context);

    /// <summary>
    /// Called when the player interacts with this object.
    /// </summary>
    void Interact(PlayerInteractor context);
}