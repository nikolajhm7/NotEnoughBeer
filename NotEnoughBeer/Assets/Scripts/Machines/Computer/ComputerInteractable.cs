using UnityEngine;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [TextArea] public string InteractionDescription = "Use Computer";
    public int priority = 0;

    public int Priority => priority;
    public string GetInteractionDescription(PlayerInteractor context) => InteractionDescription;

    public bool CanInteract(PlayerInteractor context) => true;

    public void Interact(PlayerInteractor context)
    {
        Debug.Log("[Computer] Interacted!");
    }
}