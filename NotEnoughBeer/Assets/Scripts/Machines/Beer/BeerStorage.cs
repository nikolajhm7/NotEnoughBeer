using UnityEngine;

public class BeerStorage : MonoBehaviour
{
    public static BeerStorage Instance;

    public enum BeerRarity
    {
        Common,
        Uncommon,
        Rare,
        Mythical,
        Legendary
    }

    [Header("Prices per rarity (per bottle)")]
    public int commonPrice = 10;
    public int uncommonPrice = 15;
    public int rarePrice = 25;
    public int mythicalPrice = 40;
    public int legendaryPrice = 60;

    // index = (int)BeerRarity
    int[] _rarityCounts = new int[5];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Debug / fallback: adds common beer
    public void AddBeer(int amount)
    {
        AddBeer(amount, BeerRarity.Common);
    }

    public void AddBeer(int amount, BeerRarity rarity)
    {
        if (amount <= 0) return;
        _rarityCounts[(int)rarity] += amount;
    }

    public int TotalBottles
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < _rarityCounts.Length; i++)
                sum += _rarityCounts[i];
            return sum;
        }
    }

    public int SellAll(out int totalBottles)
    {
        totalBottles = TotalBottles;
        if (totalBottles <= 0)
            return 0;

        int totalValue =
            _rarityCounts[(int)BeerRarity.Common] * commonPrice +
            _rarityCounts[(int)BeerRarity.Uncommon] * uncommonPrice +
            _rarityCounts[(int)BeerRarity.Rare] * rarePrice +
            _rarityCounts[(int)BeerRarity.Mythical] * mythicalPrice +
            _rarityCounts[(int)BeerRarity.Legendary] * legendaryPrice;

        // clear inventory
        for (int i = 0; i < _rarityCounts.Length; i++)
            _rarityCounts[i] = 0;

        return totalValue;
    }
}
