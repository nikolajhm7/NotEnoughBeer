using UnityEngine;

public class StorageContainerInteractable : MonoBehaviour, IInteractable
{
    public int Priority => 5;

    [SerializeField] private StorageContainer container;

    void Awake()
    {
        if (container == null)
            container = GetComponent<StorageContainer>();
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return container != null && container.Inv != null;
    }

    public string GetInteractionDescription(PlayerInteractor interactor)
    {
        return "Deposit all\nShift+F - Withdraw materials";
    }

    // 🔹 CALLED BY PlayerInteractor (R key)
    public void ShowStorageContents_Public()
    {
        ShowStorageContents();
    }

    void ShowStorageContents()
    {
        if (container == null || container.Inv == null)
        {
            NotificationService.Instance?.Show("Storage is missing.");
            return;
        }

        var inv = container.Inv;

        if (inv.TotalUnits <= 0)
        {
            NotificationService.Instance?.Show("Storage: Empty");
            return;
        }

        var stacks = inv.ToStacks();
        stacks.Sort((a, b) => a.Id.CompareTo(b.Id));

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Storage:");

        foreach (var s in stacks)
            sb.AppendLine($"{s.Id}: {s.Amount}");

        sb.AppendLine($"({inv.TotalUnits}/{inv.Capacity})");

        NotificationService.Instance?.Show(sb.ToString());
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (PocketInventory.Instance == null)
        {
            NotificationService.Instance?.Show("No PocketInventory in scene.");
            return;
        }

        bool shift = IsShiftHeld();

        int moved = shift ? WithdrawMaterialsOnly() : DepositAll();

        if (shift)
            NotificationService.Instance?.Show(
                moved > 0 ? $"Withdrew {moved} materials." : "Nothing to withdraw (or pocket full)."
            );
        else
            NotificationService.Instance?.Show(
                moved > 0 ? $"Deposited {moved} items." : "Nothing to deposit (or container full)."
            );
    }

    int DepositAll()
    {
        var pocket = PocketInventory.Instance.Inv;
        int moved = 0;

        moved += Move(pocket, container.Inv, ItemId.Barley);
        moved += Move(pocket, container.Inv, ItemId.Yeast);
        moved += Move(pocket, container.Inv, ItemId.Bottles);

        moved += Move(pocket, container.Inv, ItemId.Beer_Common);
        moved += Move(pocket, container.Inv, ItemId.Beer_Uncommon);
        moved += Move(pocket, container.Inv, ItemId.Beer_Rare);
        moved += Move(pocket, container.Inv, ItemId.Beer_Mythical);
        moved += Move(pocket, container.Inv, ItemId.Beer_Legendary);

        return moved;
    }

    int WithdrawMaterialsOnly()
    {
        var pocket = PocketInventory.Instance.Inv;
        int moved = 0;

        moved += Move(container.Inv, pocket, ItemId.Barley);
        moved += Move(container.Inv, pocket, ItemId.Yeast);
        moved += Move(container.Inv, pocket, ItemId.Bottles);

        return moved;
    }

    static int Move(Inventory from, Inventory to, ItemId id)
    {
        int have = from.Get(id);
        if (have <= 0) return 0;

        int canFit = Mathf.Min(have, to.FreeUnits);
        if (canFit <= 0) return 0;

        if (!from.TryRemove(id, canFit)) return 0;

        if (!to.TryAdd(id, canFit))
        {
            from.TryAdd(id, canFit); // rollback
            return 0;
        }

        return canFit;
    }

    static bool IsShiftHeld()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return false;
        return kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
    }
}
