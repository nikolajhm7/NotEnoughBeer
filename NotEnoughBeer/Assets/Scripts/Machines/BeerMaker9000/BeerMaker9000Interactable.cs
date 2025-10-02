using UnityEngine;

public class BeerMaker9000Interactable : MonoBehaviour, IInteractable
{
    [TextArea] public string InteractionDescription = "Use BeerMaker9000";
    public int priority = 0;

    public int Priority => priority;
    public string GetInteractionDescription(PlayerInteractor context) => InteractionDescription;

    public bool CanInteract(PlayerInteractor context) => true;

    public void Interact(PlayerInteractor context)
    {
        Debug.Log("[BeerMaker9000] Interacted!");
    }
}
