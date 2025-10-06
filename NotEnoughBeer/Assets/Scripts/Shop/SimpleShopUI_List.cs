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

    [Header("Sell (RIGHT) - placeholder beer")]
    [SerializeField] TMP_Text beerCountText;
    [SerializeField] TMP_Text unitPriceText;
    [SerializeField] Button sellAllButton;
    [SerializeField] Button addBeerDebugButton;
    [SerializeField] int sellPricePerUnit = 10;
    int beerCount = 0;

    [Header("HUD")]
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text toastText;

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
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        RefreshAll();
    }
    public void Hide()
    {
        if (rootPanel) rootPanel.SetActive(false);
    }

    // ---------- BUY (equip for MachinePlacer) ----------
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

            if (name) name.text = string.IsNullOrEmpty(def.id) ? def.name : def.id; // fallback to asset name
            if (price) price.text = $"$ {def.cost}";
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Equip(def));
            }
        }
    }

    void Equip(MachineDefinition def)
    {
        if (!placer) { Toast("No MachinePlacer assigned."); return; }
        placer.EquipFromUI(def);                // ghost + spending handled by MachinePlacer
        Toast($"Equipped {def.name}. Place with Space or LMB.");
        Hide();                                 // optional: close shop so player can place immediately
    }

    // ---------- SELL (placeholder) ----------
    void HookSell()
    {
        if (sellAllButton)
        {
            sellAllButton.onClick.RemoveAllListeners();
            sellAllButton.onClick.AddListener(() =>
            {
                if (Currency.Instance == null) { Toast("No Currency in scene."); return; }
                if (beerCount <= 0) { Toast("No beer to sell."); return; }

                int payout = beerCount * sellPricePerUnit;
                beerCount = 0;
                Currency.Instance.AddCurrency(payout);
                Toast($"Sold beer for ${payout}.");
                RefreshAll();
            });
        }

        if (addBeerDebugButton)
        {
            addBeerDebugButton.onClick.RemoveAllListeners();
            addBeerDebugButton.onClick.AddListener(() =>
            {
                beerCount += 5; // simulate production
                RefreshSellTexts();
            });
        }
    }

    // ---------- UI refresh ----------
    void RefreshAll() { RefreshMoney(); RefreshSellTexts(); }
    void RefreshMoney()
    {
        if (moneyText) moneyText.text =
            Currency.Instance ? $"$ {Currency.Instance.CurrencyAmount}" : "$ 0";
    }
    void RefreshSellTexts()
    {
        if (unitPriceText) unitPriceText.text = $"$ {sellPricePerUnit}/unit";
        if (beerCountText) beerCountText.text = $"{beerCount} units";
    }
    void Toast(string msg) { if (toastText) toastText.text = msg; }
}
