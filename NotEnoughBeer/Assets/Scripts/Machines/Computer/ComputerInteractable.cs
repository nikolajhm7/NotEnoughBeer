using UnityEngine;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [TextArea] public string InteractionDescription = "Use Computer (Shop)";
    public int priority = 0;

    [SerializeField] private SimpleShopUI_List shopUI; // or SimpleShopUI_MachinePlacer if you switched

    public int Priority => priority;
    public string GetInteractionDescription(PlayerInteractor context) => InteractionDescription;
    public bool CanInteract(PlayerInteractor context) => true;

    void Awake()
    {
        if (!shopUI)
            shopUI = FindObjectOfType<SimpleShopUI_List>(true); // finds even inactive panels

        if (!shopUI)
            Debug.LogWarning("[Computer] Couldn't find SimpleShopUI_List in the scene. Did the script compile and is it on your panel?");
    }

    public void Interact(PlayerInteractor ctx)
    {
        Debug.Log("[Computer] Interact called.");
        if (!shopUI)
        {
            Debug.LogWarning("[Computer] shopUI is NOT assigned.");
            return;
        }

        Debug.Log($"[Computer] Toggling shop. IsOpen={shopUI.IsOpen}");
        if (shopUI.IsOpen) shopUI.Hide();
        else shopUI.Open();
    }
}
