using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI currencyText;
    public static CurrencyUI Instance;
    public Currency Currency;


    private void Awake()
    {
        Instance = this;
        UpdateCurrency(Currency.CurrencyAmount);
    }

    public void UpdateCurrency(int amount)
    {
        currencyText.text = $"${amount}";
    }
}
