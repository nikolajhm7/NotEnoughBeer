public interface IInteractable
{
  
    int Priority { get; }

    
    bool CanInteract(PlayerInteractor context);

    
    
    string GetInteractionDescription(PlayerInteractor context);

   
    void Interact(PlayerInteractor context);
}