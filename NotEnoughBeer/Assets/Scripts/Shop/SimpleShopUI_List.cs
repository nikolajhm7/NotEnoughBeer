using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleShopUI_List : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] GameObject rootPanel;               // enable/disable this panel

    [Header("Buy List (LEFT)")]
    [SerializeField] Transform listContainer;            // parent for rows
    [SerializeField] GameObject rowPrefab;               // child names: Name(TMP), Price(TMP), BuyButton(Button)
    [SerializeField] List<MachineDefinition> catalog = new List<MachineDefinition>();
    [SerializeField] MachinePlacer placer;               // drag your MachinePlacer here

    [Header("Sell (RIGHT) - beer")]
    [SerializeField] TMP_Text beerCountText;
    [SerializeField] TMP_Text unitPriceText;
    [SerializeField] Button sellAllButton;
    [SerializeField] Button addBeerDebugButton;

    [Header("HUD")]
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text toastText;

    [Header("Garage Upgrade")]
    [SerializeField] GridManager grid;                   // assign TileManager here in Inspector

    [Header("Garage Upgrade")]
    
    [SerializeField] SaveManager saveManager;   


    public bool IsOpen => rootPanel && rootPanel.activeSelf;

    readonly List<GameObject> _spawnedRows = new List<GameObject>();

    void Awake()
    {
        Hide();
        BuildList();
        HookSell();
        RefreshAll();
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

    // ---------- BUY LIST (machines + ingredients + garage) ----------
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

            var label = string.IsNullOrEmpty(def.id) ? def.name : def.id;
            if (name) name.text = label;

            int displayCost = def.cost;

            // special case: garage upgrade uses dynamic cost
            if (!string.IsNullOrEmpty(def.id) && def.id.ToLower() == "garage_upgrade")
            {
                displayCost = GetGarageUpgradeCost();
            }

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
            case "barley":
                BuyBarley(def);
                break;

            case "yeast":
                BuyYeast(def);
                break;

            case "bottles":
                BuyBottles(def);
                break;

            case "garage_upgrade":
                UpgradeGarageFromDef(def);
                break;

            default:
                // normal machine → place with MachinePlacer
                Equip(def);
                break;
        }

        SFXManager.Instance.Play(SFX.Purchase);
    }

    void BuyBarley(MachineDefinition def)
    {
        if (!Currency.Instance.SpendCurrency(def.cost))
        {
            Toast("Not enough money.");
            return;
        }

        if (IngredientStorage.Instance != null)
        {
            int amount = 10;
            IngredientStorage.Instance.AddBarley(amount);
            Toast($"Bought {amount} barley.");
        }
        else
        {
            Toast("No IngredientStorage in scene.");
        }

        RefreshAll();
    }

    void BuyYeast(MachineDefinition def)
    {
        if (!Currency.Instance.SpendCurrency(def.cost))
        {
            Toast("Not enough money.");
            return;
        }

        if (IngredientStorage.Instance != null)
        {
            int amount = 5;
            IngredientStorage.Instance.Yeast += amount;
            Toast($"Bought {amount} yeast.");
        }
        else
        {
            Toast("No IngredientStorage in scene.");
        }

        RefreshAll();
    }

    void BuyBottles(MachineDefinition def)
    {
        if (!Currency.Instance.SpendCurrency(def.cost))
        {
            Toast("Not enough money.");
            return;
        }

        if (IngredientStorage.Instance != null)
        {
            int amount = 12;
            IngredientStorage.Instance.Bottles += amount;
            Toast($"Bought {amount} bottles.");
        }
        else
        {
            Toast("No IngredientStorage in scene.");
        }

        RefreshAll();
    }

    void Equip(MachineDefinition def)
    {
        if (!placer)
        {
            Toast("No MachinePlacer assigned.");
            return;
        }

        placer.EquipFromUI(def);                // ghost + spending handled by MachinePlacer
        Toast($"Equipped {def.name}. Place with Space or LMB.");
        Hide();                                 // optional: close shop so player can place immediately
    }

    // ---------- SELL ----------
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

                if (BeerStorage.Instance == null)
                {
                    Toast("No BeerStorage in scene.");
                    return;
                }

                int totalBottles;
                int payout = BeerStorage.Instance.SellAll(out totalBottles);

                if (totalBottles <= 0)
                {
                    Toast("No beer to sell.");
                    return;
                }

                Currency.Instance.AddCurrency(payout);
                Toast($"Sold {totalBottles} bottles for ${payout}.");
                RefreshAll();
            });
        }

        if (addBeerDebugButton)
        {
            addBeerDebugButton.onClick.RemoveAllListeners();
            addBeerDebugButton.onClick.AddListener(() =>
            {
                BeerStorage.Instance?.AddBeer(5); // common debug
                RefreshSellTexts();
            });
        }
    }

    // ---------- GARAGE UPGRADE HELPERS ----------
    int GetGarageUpgradeLevel()
    {
        if (!grid) return 0;

        // base garage is 5x5
        int baseWidth = 5;
        int extra = Mathf.Max(0, grid.width - baseWidth);

        // Expand(1,1,1,1) increases width by 2 each time: 5,7,9,11...
        int level = extra / 2;
        return level;
    }

    // 1st: 400, 2nd: 500, 3rd: 700, 4th: 900, then +200 each level
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

        // expand 1 tile in every direction
        grid.Expand(addRight: 1, addLeft: 1, addUp: 1, addDown: 1);

        // 🔁 Rebuild grid + machines + interactables properly
        

        Toast($"Garage upgraded! New size: {grid.width} x {grid.height} (cost ${cost}).");

        // rebuild UI list for new upgrade cost
        BuildList();
        RefreshAll();
    }


    // ---------- UI refresh ----------
    void RefreshAll()
    {
        RefreshMoney();
        RefreshSellTexts();
    }

    void RefreshMoney()
    {
        if (moneyText)
        {
            moneyText.text =
                Currency.Instance ? $"$ {Currency.Instance.CurrencyAmount}" : "$ 0";
        }
    }

    void RefreshSellTexts()
    {
        if (unitPriceText) unitPriceText.text = "Value varies by rarity";
        if (beerCountText)
        {
            int count = BeerStorage.Instance ? BeerStorage.Instance.TotalBottles : 0;
            beerCountText.text = $"{count} units";
        }
    }

    void Toast(string msg)
    {
        if (toastText) toastText.text = msg;
    }
}
