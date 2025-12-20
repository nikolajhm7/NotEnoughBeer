using TMPro;
using UnityEngine;

public class InventoryItemRow : MonoBehaviour
{
    public TMP_Text itemText;

    public void Set(ItemId id, int amount)
    {
        itemText.text = $"{id.ToString()} x{amount}";
    }
}
