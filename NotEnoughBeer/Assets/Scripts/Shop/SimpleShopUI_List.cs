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
    [SerializeField] int sellPricePerUnit = 10;

    [Header("HUD")]
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text toastText;

    [Header("Garage Upgrade")]
    [SerializeField] GridManager grid;
    [SerializeField] Button upgradeGarageButton;
    [SerializeField] int garageUpgradeCost = 200;

    public bool IsOpen => rootPanel && rootPanel.activeSelf;

    readonly List<GameObject> _spawnedRows = new List<GameObject>();

    void Awake()
    {
        Hide();
        BuildList();
        HookSell();
        HookGarageUpgrade();
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

    // ---------- BUY LIST (machines + ingredients) ----------
    void BuildList()
    {
        // clear old rows
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
            if (price) price.text = $"$ {def.cost}";
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
            default:
                Equip(def); // normal machine
                break;
        }
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


    // ---------- GARAGE UPGRADE ----------
    void HookGarageUpgrade()
    {
        if (!upgradeGarageButton || !grid) return;

        upgradeGarageButton.onClick.RemoveAllListeners();
        upgradeGarageButton.onClick.AddListener(() =>
        {
            if (Currency.Instance == null)
            {
                Toast("No Currency in scene.");
                return;
            }

            if (!Currency.Instance.SpendCurrency(garageUpgradeCost))
            {
                Toast("Not enough money to upgrade garage.");
                return;
            }

            grid.Expand(addRight: 1, addLeft: 1, addUp: 1, addDown: 1);
            Toast("Garage upgraded!");
            RefreshAll();
        });
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
