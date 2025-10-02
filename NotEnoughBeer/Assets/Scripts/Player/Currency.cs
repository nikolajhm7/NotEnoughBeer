using UnityEngine;

public class Currency : MonoBehaviour
{
    public static Currency Instance;
    public int CurrencyAmount { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetCurrency(int amount)
    {
        CurrencyAmount = Mathf.Max(0, amount);
        if (CurrencyUI.Instance)
        {
            CurrencyUI.Instance.UpdateCurrency(CurrencyAmount);
        }
    }

    public void AddCurrency(int amount)
    {
        CurrencyAmount += amount;
        CurrencyUI.Instance.UpdateCurrency(CurrencyAmount);
    }

    public bool SpendCurrency(int amount)
    {
        if (CurrencyAmount < amount)
        {
            return false;
        }
        
        CurrencyAmount -= amount;
        
        CurrencyUI.Instance.UpdateCurrency(CurrencyAmount);
        return true;
    }
}
