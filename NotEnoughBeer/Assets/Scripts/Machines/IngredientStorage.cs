using UnityEngine;

public class IngredientStorage : MonoBehaviour
{
    public static IngredientStorage Instance;

    [Header("Ingredients")]
    public int Barley;
    public int Yeast;
    public int Bottles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------- BARLEY ----------
    public void AddBarley(int amount) => Barley += amount;

    public bool UseBarley(int amount)
    {
        if (Barley < amount) return false;
        Barley -= amount;
        return true;
    }

    // ---------- YEAST ----------
    public void AddYeast(int amount) => Yeast += amount;

    public bool UseYeast(int amount)
    {
        if (Yeast < amount) return false;
        Yeast -= amount;
        return true;
    }

    // ---------- BOTTLES ----------
    public void AddBottles(int amount) => Bottles += amount;

    public bool UseBottles(int amount)
    {
        if (Bottles < amount) return false;
        Bottles -= amount;
        return true;
    }
}
