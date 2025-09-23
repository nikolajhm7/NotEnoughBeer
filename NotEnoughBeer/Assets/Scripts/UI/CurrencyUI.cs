using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;
    public static CurrencyUI Instance;

    private void Awake()
    {
        Instance = this;
        UpdateCurrency(0);
    }

    public void UpdateCurrency(int amount)
    {
        currencyText.text = $"${amount}";
    }
}
