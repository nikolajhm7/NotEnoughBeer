using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleShopUI_List : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] GameObject rootPanel;

    [Header("Buy List (LEFT)")]
    [SerializeField] Transform listContainer;
    [SerializeField] GameObject rowPrefab;
    [SerializeField] List<MachineDefinition> catalog = new List<MachineDefinition>();
    [SerializeField] MachinePlacer placer;

    [Header("Sell (RIGHT) - beer")]
    [SerializeField] TMP_Text beerCountText;
    [SerializeField] TMP_Text unitPriceText;
    [SerializeField] Button sellAllButton;
    [SerializeField] Button addBeerDebugButton;

    [Header("NEW: Sell Filters (optional)")]
    [SerializeField] TMP_Dropdown sellRarityDropdown;     
    [SerializeField] Toggle sellFromPocketToggle;         
    [SerializeField] Toggle sellFromContainersToggle;     

    [Header("HUD")]
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text toastText;

    [Header("Garage Upgrade")]
    [SerializeField] GridManager grid;

    [Header("Save")]
    [SerializeField] SaveManager saveManager;

    public bool IsOpen => rootPanel && rootPanel.activeSelf;
    readonly List<GameObject> _spawnedRows = new();

    void Awake()
    {
        Hide();
        BuildList();
        HookSell();
        RefreshAll();

        BuildSellRarityDropdownIfNeeded();
    }

    public void Open()
    {
        if (rootPanel) rootPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RefreshAll();
    }

    public void Hide()
    {
        if (rootPanel) rootPanel.SetActive(false);
    }

    
    void BuildList()
    {
        foreach (var go in _spawnedRows) Destroy(go);
        _spawnedRows.Clear();

        foreach (var def in catalog)
        {
            if (!def) continue;

            var row = Instantiate(rowPrefab, listContainer);
            _spawnedRows.Add(row);

            var name = row.transform.Find("Name")?.GetComponent<TMP_Text>();
            var price = row.transform.Find("Price")?.GetComponent<TMP_Text>();
            var btn = row.transform.Find("BuyButton")?.GetComponent<Button>();

            var label = def.name;

            if (name) name.text = label;

            int displayCost = def.cost;

            if (!string.IsNullOrEmpty(def.id) && def.id.ToLower() == "garage_upgrade")
                displayCost = GetGarageUpgradeCost();

            if (price) price.text = $"$ {displayCost}";

            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClickCatalogEntry(def));
            }
        }
    }

    void OnClickCatalogEntry(MachineDefinition def)
    {
        if (def == null) return;

        if (Currency.Instance == null)
        {
            Toast("No Currency in scene.");
            return;
        }

        var id = string.IsNullOrEmpty(def.id) ? "" : def.id.ToLower();

        switch (id)
        {
            case "barley": BuyBarley(def); break;
            case "yeast": BuyYeast(def); break;
            case "bottles": BuyBottles(def); break;
            case "garage_upgrade": UpgradeGarageFromDef(def); break;
            default: Equip(def); break;
        }

        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(SFX.Purchase);
    }

    
    void BuyBarley(MachineDefinition def)
    {
        if (!Currency.Instance.SpendCurrency(def.cost))
        {
            Toast("Not enough money.");
            return;
        }

        int amount = 10;

        
        if (PocketInventory.Instance != null)
        {
            if (!PocketInventory.Instance.Inv.TryAdd(ItemId.Barley, amount))
            {
                Toast("Pocket full! Buy/place a container.");
                return;
            }
            Toast($"Bought {amount} barley.");
            RefreshAll();
            return;
        }

        
        if (IngredientStorage.Instance != null)
        {
            IngredientStorage.Instance.AddBarley(amount);
            Toast($"Bought {amount} barley.");
        }
        else Toast("No PocketInventory or IngredientStorage in scene.");

        RefreshAll();
    }

    void BuyYeast(MachineDefinition def)
    {
        if (!Currency.Instance.SpendCurrency(def.cost))
        {
            Toast("Not enough money.");
            return;
        }

        int amount = 5;

        if (PocketInventory.Instance != null)
        {
            if (!PocketInventory.Instance.Inv.TryAdd(ItemId.Yeast, amount))
            {
                Toast("Pocket full! Buy/place a container.");
                return;
            }
            Toast($"Bought {amount} yeast.");
            RefreshAll();
            return;
        }

        if (IngredientStorage.Instance != null)
        {
            IngredientStorage.Instance.AddYeast(amount);
            Toast($"Bought {amount} yeast.");
        }
        else Toast("No PocketInventory or IngredientStorage in scene.");

        RefreshAll();
    }

    void BuyBottles(MachineDefinition def)
    {
        if (!Currency.Instance.SpendCurrency(def.cost))
        {
            Toast("Not enough money.");
            return;
        }

        int amount = 12;

        if (PocketInventory.Instance != null)
        {
            if (!PocketInventory.Instance.Inv.TryAdd(ItemId.Bottles, amount))
            {
                Toast("Pocket full! Buy/place a container.");
                return;
            }
            Toast($"Bought {amount} bottles.");
            RefreshAll();
            return;
        }

        if (IngredientStorage.Instance != null)
        {
            IngredientStorage.Instance.AddBottles(amount);
            Toast($"Bought {amount} bottles.");
        }
        else Toast("No PocketInventory or IngredientStorage in scene.");

        RefreshAll();
    }

    void Equip(MachineDefinition def)
    {
        if (!placer)
        {
            Toast("No MachinePlacer assigned.");
            return;
        }

        placer.EquipFromUI(def);
        Toast($"Equipped {def.name}. Place with Space or LMB.");
        Hide();
    }

    
    void HookSell()
    {
        if (sellAllButton)
        {
            sellAllButton.onClick.RemoveAllListeners();
            sellAllButton.onClick.AddListener(() =>
            {
                if (Currency.Instance == null)
                {
                    Toast("No Currency in scene.");
                    return;
                }

                
                if (PocketInventory.Instance != null || StorageRegistry.Instance != null)
                {
                    var rarity = GetSelectedRarityOrNull(); 
                    bool fromPocket = sellFromPocketToggle ? sellFromPocketToggle.isOn : true;
                    bool fromContainers = sellFromContainersToggle ? sellFromContainersToggle.isOn : true;

                    int sold;
                    int payout;
                    SellBeer_NewSystem(rarity, fromPocket, fromContainers, out sold, out payout);

                    if (sold <= 0)
                    {
                        Toast("No beer to sell.");
                        return;
                    }

                    Currency.Instance.AddCurrency(payout);
                    Toast($"Sold {sold} bottles for ${payout}.");
                    RefreshAll();
                    return;
                }

                
                if (BeerStorage.Instance == null)
                {
                    Toast("No BeerStorage in scene.");
                    return;
                }

                int totalBottles;
                int payoutOld = BeerStorage.Instance.SellAll(out totalBottles);

                if (totalBottles <= 0)
                {
                    Toast("No beer to sell.");
                    return;
                }

                Currency.Instance.AddCurrency(payoutOld);
                Toast($"Sold {totalBottles} bottles for ${payoutOld}.");
                RefreshAll();
            });
        }

        if (addBeerDebugButton)
        {
            addBeerDebugButton.onClick.RemoveAllListeners();
            addBeerDebugButton.onClick.AddListener(() =>
            {
                
                if (PocketInventory.Instance != null)
                {
                    PocketInventory.Instance.Inv.TryAdd(ItemId.Beer_Common, 5);
                    RefreshSellTexts();
                    return;
                }

                
                BeerStorage.Instance?.AddBeer(5);
                RefreshSellTexts();
            });
        }
    }

    void SellBeer_NewSystem(BeerStorage.BeerRarity? rarityFilter, bool fromPocket, bool fromContainers, out int soldTotal, out int payoutTotal)
    {
        soldTotal = 0;
        payoutTotal = 0;

        
        if (!rarityFilter.HasValue)
        {
            SellBeerItem(ItemId.Beer_Common, fromPocket, fromContainers, ref soldTotal, ref payoutTotal);
            SellBeerItem(ItemId.Beer_Uncommon, fromPocket, fromContainers, ref soldTotal, ref payoutTotal);
            SellBeerItem(ItemId.Beer_Rare, fromPocket, fromContainers, ref soldTotal, ref payoutTotal);
            SellBeerItem(ItemId.Beer_Mythical, fromPocket, fromContainers, ref soldTotal, ref payoutTotal);
            SellBeerItem(ItemId.Beer_Legendary, fromPocket, fromContainers, ref soldTotal, ref payoutTotal);
            return;
        }

        var id = BeerItemForRarity(rarityFilter.Value);
        SellBeerItem(id, fromPocket, fromContainers, ref soldTotal, ref payoutTotal);
    }

    void SellBeerItem(ItemId beerId, bool fromPocket, bool fromContainers, ref int soldTotal, ref int payoutTotal)
    {
        int unitPrice = GetUnitPriceForBeerId(beerId);

        if (fromPocket && PocketInventory.Instance != null)
        {
            var inv = PocketInventory.Instance.Inv;
            int have = inv.Get(beerId);
            if (have > 0)
            {
                inv.TryRemove(beerId, have);
                soldTotal += have;
                payoutTotal += have * unitPrice;
            }
        }

        if (fromContainers && StorageRegistry.Instance != null)
        {
            foreach (var c in StorageRegistry.Instance.Containers)
            {
                if (!c) continue;
                var inv = c.Inv;

                int have = inv.Get(beerId);
                if (have <= 0) continue;

                inv.TryRemove(beerId, have);
                soldTotal += have;
                payoutTotal += have * unitPrice;
            }
        }
    }

   
    int GetGarageUpgradeLevel()
    {
        if (!grid) return 0;

        int baseWidth = 5;
        int extra = Mathf.Max(0, grid.width - baseWidth);
        int level = extra / 2;
        return level;
    }

    int GetGarageUpgradeCost()
    {
        int level = GetGarageUpgradeLevel();
        int[] baseCosts = { 400, 500, 700, 900 };

        if (level < baseCosts.Length)
            return baseCosts[level];

        int extraLevels = level - (baseCosts.Length - 1);
        return baseCosts[baseCosts.Length - 1] + 200 * extraLevels;
    }

    void UpgradeGarageFromDef(MachineDefinition def)
    {
        if (!grid)
        {
            Toast("No GridManager assigned.");
            return;
        }

        int cost = GetGarageUpgradeCost();

        if (!Currency.Instance.SpendCurrency(cost))
        {
            Toast($"Not enough money to upgrade garage. Need ${cost}.");
            return;
        }

        grid.Expand(addRight: 2, addLeft: 0, addUp: 2, addDown: 0);

        Toast($"Garage upgraded! New size: {grid.width} x {grid.height} (cost ${cost}).");

        BuildList();
        RefreshAll();
    }

    
    void RefreshAll()
    {
        RefreshMoney();
        RefreshSellTexts();
    }

    void RefreshMoney()
    {
        if (moneyText)
            moneyText.text = Currency.Instance ? $"$ {Currency.Instance.CurrencyAmount}" : "$ 0";
    }

    void RefreshSellTexts()
    {
        if (unitPriceText) unitPriceText.text = "Value varies by rarity";

        if (!beerCountText) return;

        
        if (PocketInventory.Instance != null || StorageRegistry.Instance != null)
        {
            int total =
                GetBeerCount(ItemId.Beer_Common) +
                GetBeerCount(ItemId.Beer_Uncommon) +
                GetBeerCount(ItemId.Beer_Rare) +
                GetBeerCount(ItemId.Beer_Mythical) +
                GetBeerCount(ItemId.Beer_Legendary);

            beerCountText.text = $"{total} units";
            return;
        }

        
        int countOld = BeerStorage.Instance ? BeerStorage.Instance.TotalBottles : 0;
        beerCountText.text = $"{countOld} units";
    }

    int GetBeerCount(ItemId id)
    {
        int total = 0;

        if (PocketInventory.Instance != null)
            total += PocketInventory.Instance.Inv.Get(id);

        if (StorageRegistry.Instance != null)
        {
            foreach (var c in StorageRegistry.Instance.Containers)
                if (c) total += c.Inv.Get(id);
        }

        return total;
    }

    void Toast(string msg)
    {
        if (toastText) toastText.text = msg;
    }

    
    void BuildSellRarityDropdownIfNeeded()
    {
        if (!sellRarityDropdown) return;

        sellRarityDropdown.ClearOptions();
        sellRarityDropdown.AddOptions(new List<string>
        {
            "All",
            "Common",
            "Uncommon",
            "Rare",
            "Mythical",
            "Legendary"
        });
    }

    BeerStorage.BeerRarity? GetSelectedRarityOrNull()
    {
        if (!sellRarityDropdown) return null;

        
        switch (sellRarityDropdown.value)
        {
            case 1: return BeerStorage.BeerRarity.Common;
            case 2: return BeerStorage.BeerRarity.Uncommon;
            case 3: return BeerStorage.BeerRarity.Rare;
            case 4: return BeerStorage.BeerRarity.Mythical;
            case 5: return BeerStorage.BeerRarity.Legendary;
            default: return null;
        }
    }

    static ItemId BeerItemForRarity(BeerStorage.BeerRarity r)
    {
        return r switch
        {
            BeerStorage.BeerRarity.Common => ItemId.Beer_Common,
            BeerStorage.BeerRarity.Uncommon => ItemId.Beer_Uncommon,
            BeerStorage.BeerRarity.Rare => ItemId.Beer_Rare,
            BeerStorage.BeerRarity.Mythical => ItemId.Beer_Mythical,
            BeerStorage.BeerRarity.Legendary => ItemId.Beer_Legendary,
            _ => ItemId.Beer_Common
        };
    }

    int GetUnitPriceForBeerId(ItemId id)
    {
        
        if (BeerStorage.Instance == null) return 0;

        return id switch
        {
            ItemId.Beer_Common => BeerStorage.Instance.commonPrice,
            ItemId.Beer_Uncommon => BeerStorage.Instance.uncommonPrice,
            ItemId.Beer_Rare => BeerStorage.Instance.rarePrice,
            ItemId.Beer_Mythical => BeerStorage.Instance.mythicalPrice,
            ItemId.Beer_Legendary => BeerStorage.Instance.legendaryPrice,
            _ => 0
        };
    }
}
