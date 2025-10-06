using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Item", fileName = "NewShopItem")]
public class ShopItem : ScriptableObject
{
    [Header("Display")]
    public string displayName = "Beermaker9000";
    public Sprite icon;

    [Header("Economy")]
    [Min(0)] public int price = 500;

    [Header("Prefab to spawn")]
    public GameObject prefab;
}
