using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    public GridManager Grid;
    public PlayerMovement Movement;
    public GameObject InteractionRoot;
    public TextMeshProUGUI InteractionText;

    [Header("Input")]
    public Key InteractKey = Key.F;
    public Key SecondaryKey = Key.R;

    [Header("Behavior")]
    public bool HideInteractionWhenPaused = true;

    private IInteractable current;

    private void Update()
    {
        if (!Grid || !Movement) return;

        if (InteractionRoot && HideInteractionWhenPaused && Time.timeScale == 0)
        {
            InteractionRoot.SetActive(false);
            return;
        }

        var targetCell = Movement.CurrentCell + Movement.CurrentFacingDir;

        current = Grid.GetInteractablesAt(targetCell)
            .Where(x => x != null && x.CanInteract(this))
            .OrderByDescending(x => x.Priority)
            .FirstOrDefault();

        if (current != null)
        {
            if (InteractionRoot) InteractionRoot.SetActive(true);
            SetInteractionText(current);
        }
        else
        {
            if (InteractionRoot) InteractionRoot.SetActive(false);
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        
        if (kb[InteractKey].wasPressedThisFrame)
        {
            current?.Interact(this);
        }

        
        if (kb[SecondaryKey].wasPressedThisFrame)
        {
            if (current is StorageContainerInteractable storage)
            {
                storage.ShowStorageContents_Public();
            }
        }
    }

    private void SetInteractionText(IInteractable interactable)
    {
        if (!InteractionText) return;

        if (interactable == null)
        {
            InteractionText.gameObject.SetActive(false);
            return;
        }

        string text = $"[{InteractKey}] - {interactable.GetInteractionDescription(this)}";

        
        if (interactable is StorageContainerInteractable)
        {
            text += $"\n[{SecondaryKey}] - Show storage";
        }

        InteractionText.text = text;
        InteractionText.gameObject.SetActive(true);
    }

}
