public class Batch
{
    public enum Stage
    {
        Malting,
        Mashing,
        Fermentation,
        Conditioning,
        Packaging,
        Finished
    }

    public Stage CurrentStage { get; private set; } = Stage.Malting;
    public float QualityValue { get; private set; } = 0f;

    public bool IsFinished => CurrentStage == Stage.Finished;

    public void AddScore(float score)
    {
        QualityValue += score;
    }

    public void Advance()
    {
        switch (CurrentStage)
        {
            case Stage.Malting: CurrentStage = Stage.Mashing; break;
            case Stage.Mashing: CurrentStage = Stage.Fermentation; break;
            case Stage.Fermentation: CurrentStage = Stage.Conditioning; break;
            case Stage.Conditioning: CurrentStage = Stage.Packaging; break;
            case Stage.Packaging: CurrentStage = Stage.Finished; break;
        }
    }
}
