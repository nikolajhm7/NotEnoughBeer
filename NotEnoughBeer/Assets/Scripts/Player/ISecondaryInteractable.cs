public interface ISecondaryInteractable
{
    string GetSecondaryInteractionDescription(PlayerInteractor interactor);
    void SecondaryInteract(PlayerInteractor interactor);
}
