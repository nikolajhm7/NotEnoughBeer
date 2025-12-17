using UnityEngine;

public class StorageContainerInteractable : MonoBehaviour, IInteractable
{
    [Header("Refs")]
    public StorageContainer container;

    public int Priority => 3;

    void Awake()
    {
        if (container == null)
            container = GetComponent<StorageContainer>();
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return PocketInventory.Instance != null && container != null && container.Inv != null;
    }

    public void Interact(PlayerInteractor interactor)
    {
        var pocket = PocketInventory.Instance.Inv;
        int moved = 0;

        // Move every item type we currently support
        moved += MoveAll(ItemId.Barley);
        moved += MoveAll(ItemId.Yeast);
        moved += MoveAll(ItemId.Bottles);

        moved += MoveAll(ItemId.Beer_Common);
        moved += MoveAll(ItemId.Beer_Uncommon);
        moved += MoveAll(ItemId.Beer_Rare);
        moved += MoveAll(ItemId.Beer_Mythical);
        moved += MoveAll(ItemId.Beer_Legendary);

        NotificationService.Instance?.Show(moved > 0
            ? $"Deposited {moved} items."
            : "Nothing to deposit (or container full).");

        int MoveAll(ItemId id)
        {
            int have = pocket.Get(id);
            if (have <= 0) return 0;

            int canFit = Mathf.Min(have, container.Inv.FreeUnits);
            if (canFit <= 0) return 0;

            // remove then add (safe)
            if (!pocket.TryRemove(id, canFit)) return 0;
            if (!container.Inv.TryAdd(id, canFit))
            {
                // rollback if needed
                pocket.TryAdd(id, canFit);
                return 0;
            }

            return canFit;
        }
    }

    public string GetInteractionDescription(PlayerInteractor interactor)
        => "Deposit pocket items";
}
