using UnityEngine;

public class MashingGameScorer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The thermometer to monitor temperature")]
    public Thermometer thermometer;
    
    [Tooltip("The spoon stirrer to monitor stirring")]
    public SpoonStirrer spoonStirrer;
    
    [Header("UI Elements")]
    [Tooltip("Text to display the current score")]
    public TMPro.TextMeshProUGUI scoreText;
    
    [Header("Scoring Criteria")]
    [Tooltip("Minimum temperature to score (default 60)")]
    public float minTemperature = 60f;
    
    [Tooltip("Maximum temperature to score (default 75)")]
    public float maxTemperature = 75f;
    
    [Tooltip("Time window to check stirring (default 3 seconds)")]
    public float stirCheckWindow = 3f;
    
    [Tooltip("Minimum stir time required in the window (default 1 second)")]
    public float minStirTimeRequired = 1f;
    
    [Tooltip("Points gained per second when conditions are met")]
    public float pointsPerSecond = 1f;
    
    [Header("Current State")]
    [Tooltip("Current score")]
    public float currentScore = 0f;
    
    [Tooltip("Shows if currently scoring")]
    public bool isScoring = false;
    
    [Tooltip("Total time stirred in the last 3 seconds")]
    public float recentStirTime = 0f;
    
    private float[] stirHistory;
    private int historyIndex = 0;
    private float historyInterval = 0.1f; // Check every 0.1 seconds
    private float historyTimer = 0f;
    
    void Start()
    {
        // Try to find references if not assigned
        if (thermometer == null)
        {
            thermometer = FindAnyObjectByType<Thermometer>();
        }
        
        if (spoonStirrer == null)
        {
            spoonStirrer = FindAnyObjectByType<SpoonStirrer>();
        }
        
        // Initialize stir history array
        int historySize = Mathf.CeilToInt(stirCheckWindow / historyInterval);
        stirHistory = new float[historySize];
    }
    
    void Update()
    {
        // Update stir history
        historyTimer += Time.deltaTime;
        if (historyTimer >= historyInterval)
        {
            historyTimer -= historyInterval;
            
            // Record current stir state
            bool isCurrentlyStirring = spoonStirrer != null && spoonStirrer.IsStirring();
            stirHistory[historyIndex] = isCurrentlyStirring ? historyInterval : 0f;
            
            historyIndex = (historyIndex + 1) % stirHistory.Length;
        }
        
        // Calculate total stir time in recent window
        recentStirTime = 0f;
        for (int i = 0; i < stirHistory.Length; i++)
        {
            recentStirTime += stirHistory[i];
        }
        
        // Check if scoring conditions are met
        bool temperatureOK = IsTemperatureInRange();
        bool stirringOK = recentStirTime >= minStirTimeRequired;
        
        isScoring = temperatureOK && stirringOK;
        
        // Add score if conditions are met
        if (isScoring)
        {
            currentScore += pointsPerSecond * Time.deltaTime;
        }
        
        // Update score display
        UpdateScoreUI();
    }
    
    private bool IsTemperatureInRange()
    {
        if (thermometer == null)
        {
            return false;
        }
        
        float temp = thermometer.currentTemperature;
        return temp >= minTemperature && temp <= maxTemperature;
    }
    
    /// <summary>
    /// Get the current score
    /// </summary>
    public float GetScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// Get the current score as an integer
    /// </summary>
    public int GetScoreInt()
    {
        return Mathf.FloorToInt(currentScore);
    }
    
    /// <summary>
    /// Check if currently earning points
    /// </summary>
    public bool IsEarningPoints()
    {
        return isScoring;
    }
    
    /// <summary>
    /// Reset the score
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0f;
        System.Array.Clear(stirHistory, 0, stirHistory.Length);
        historyIndex = 0;
    }
    
    /// <summary>
    /// Get a summary of current conditions for debugging
    /// </summary>
    public string GetConditionsSummary()
    {
        float temp = thermometer != null ? thermometer.currentTemperature : 0f;
        bool tempOK = IsTemperatureInRange();
        bool stirOK = recentStirTime >= minStirTimeRequired;
        
        return $"Temp: {temp:F1}° ({(tempOK ? "✓" : "✗")}) | Stir: {recentStirTime:F1}s/{minStirTimeRequired}s ({(stirOK ? "✓" : "✗")}) | Score: {GetScoreInt()}";
    }
    
    /// <summary>
    /// Update the score display UI
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {GetScoreInt()}";
        }
    }
}
