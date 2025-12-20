using UnityEngine;

public class PocketInventory : MonoBehaviour
{
    public static PocketInventory Instance;

    [Header("Pocket limit")]
    public int capacity = 60;

    public Inventory Inv { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Inv = new Inventory(capacity);
    }

    private void Start()
    {
        Inv.OnChanged += OnInventoryChanged;
    }

    void OnInventoryChanged(ItemId id, int amount)
    {
        Debug.Log($"[PocketInventory] Inventory changed: {id} now has {amount} units.");
    }

    public void SetCapacity(int c)
    {
        capacity = Mathf.Max(0, c);
        Inv.SetCapacity(capacity);
    }
}
