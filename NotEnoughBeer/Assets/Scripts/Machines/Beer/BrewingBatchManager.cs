using UnityEngine;

public class BrewingBatchManager : MonoBehaviour
{
    public static BrewingBatchManager Instance;

    public Batch CurrentBatch { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartNewBatch()
    {
        CurrentBatch = new Batch();
    }

    public bool HasActiveBatch => CurrentBatch != null && !CurrentBatch.IsFinished;

    public void AddScore(float score)
    {
        if (CurrentBatch == null) return;
        CurrentBatch.AddScore(score);
    }

    public void AdvanceStage()
    {
        if (CurrentBatch == null) return;
        CurrentBatch.Advance();
    }

    
    public BeerStorage.BeerRarity GetRarity()
    {
        if (CurrentBatch == null)
            return BeerStorage.BeerRarity.Common;

        float q = CurrentBatch.QualityValue;

        if (q >= 90) return BeerStorage.BeerRarity.Legendary;
        if (q >= 70) return BeerStorage.BeerRarity.Mythical;
        if (q >= 50) return BeerStorage.BeerRarity.Rare;
        if (q >= 30) return BeerStorage.BeerRarity.Uncommon;
        return BeerStorage.BeerRarity.Common;
    }

    public string GetRarityName()
    {
        return GetRarity().ToString();
    }
}
