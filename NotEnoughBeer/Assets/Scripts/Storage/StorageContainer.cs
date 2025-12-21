using UnityEngine;

public class StorageContainer : MonoBehaviour
{
    [Header("Container limit")]
    public int capacity = 300;

    public Inventory Inv { get; private set; }

    void Awake()
    {
        Inv = new Inventory(capacity);
    }

    
    public void SetCapacity(int newCapacity)
    {
        capacity = Mathf.Max(0, newCapacity);

        
        if (Inv != null)
            Inv.SetCapacity(capacity);
        else
            Inv = new Inventory(capacity);
    }
}
