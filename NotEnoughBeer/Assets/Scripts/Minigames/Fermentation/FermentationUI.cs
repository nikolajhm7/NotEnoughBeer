using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FermentationUI : MonoBehaviour
{
    [Header("Counter UI References")]
    [SerializeField] private TextMeshProUGUI greenCounterText;
    [SerializeField] private TextMeshProUGUI redCounterText;
    [SerializeField] private TextMeshProUGUI greenPercentageText;
    
    [Header("Game Timer UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject startScreen; // Start screen overlay
    [SerializeField] private GameObject endScreen; // End screen overlay
    
    [Header("Script References")]
    [SerializeField] private GreenSectionScript greenSectionScript;
    [SerializeField] private RingScript ringScript;
    [SerializeField] private FermentationGameManager gameManager;
    
    [Header("Display Settings")]
    [SerializeField] private string greenPrefix = "Green: ";
    [SerializeField] private string redPrefix = "Red: ";
    [SerializeField] private string percentagePrefix = "Score: ";
    [SerializeField] private string timerPrefix = "Time: ";
    [SerializeField] private int decimalPlaces = 1;
    
    public static FermentationUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Auto-find scripts if not assigned
        if (greenSectionScript == null)
            greenSectionScript = FindFirstObjectByType<GreenSectionScript>();
            
        if (ringScript == null)
            ringScript = FindFirstObjectByType<RingScript>();
            
        if (gameManager == null)
            gameManager = FindFirstObjectByType<FermentationGameManager>();
            
        // Subscribe to game manager events
        if (gameManager != null)
        {
            gameManager.OnCountdownStart.AddListener(OnCountdownStart);
            gameManager.OnGameStart.AddListener(OnGameStart);
            gameManager.OnGameEnd.AddListener(OnGameEnd);
            gameManager.OnCountdownTick.AddListener(OnCountdownTick);
        }
        
        // Setup initial UI state
        SetupInitialUI();
    }

    private void Update()
    {
        UpdateCounterDisplays();
        UpdateTimerDisplay();
    }

    private void SetupInitialUI()
    {
        // Show start button
        if (startButton != null)
            startButton.gameObject.SetActive(true);
            
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
            
        // Show start screen initially, hide end screen
        if (startScreen != null)
            startScreen.SetActive(true);
            
        if (endScreen != null)
            endScreen.SetActive(false);
    }

    private void UpdateCounterDisplays()
    {
        // Update green counter display
        if (greenCounterText != null && greenSectionScript != null)
        {
            float greenCount = greenSectionScript.GreenSectionCount;
            greenCounterText.text = $"{greenPrefix}{greenCount.ToString($"F{decimalPlaces}")}";
        }
        
        // Update red counter display
        if (redCounterText != null && ringScript != null)
        {
            float redCount = ringScript.RedRingCount;
            redCounterText.text = $"{redPrefix}{redCount.ToString($"F{decimalPlaces}")}";
        }
        
        // Update green percentage display
        if (greenPercentageText != null && greenSectionScript != null)
        {
            float greenPercentage = greenSectionScript.GreenPercentage;
            greenPercentageText.text = $"{percentagePrefix}{greenPercentage.ToString($"F{decimalPlaces}")}%";
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null && gameManager != null)
        {
            if (gameManager.IsCountingDown)
            {
                // Don't show timer during countdown
                timerText.text = "";
            }
            else if (gameManager.GameIsActive)
            {
                float remainingTime = gameManager.RemainingTime;
                timerText.text = $"{timerPrefix}{remainingTime.ToString("F1")}s";
            }
            else
            {
                // Show full time when not active
                timerText.text = $"{timerPrefix}{gameManager.GameDuration.ToString("F0")}s";
            }
        }
    }

    // Event handlers for game manager events
    private void OnCountdownStart()
    {
        Debug.Log("OnCountdownStart called - hiding start screen");
        
        if (startButton != null)
            startButton.gameObject.SetActive(false);
            
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);
            
        // Hide start screen when countdown starts (reveals minigame)
        if (startScreen != null)
        {
            Debug.Log("Start screen found, setting it to inactive");
            startScreen.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Start screen is NULL! Make sure it's assigned in the FermentationUI inspector");
        }
    }

    private void OnGameStart()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void OnGameEnd()
    {
        Debug.Log("Game ended - showing end screen and final scores");
        
        // Show end screen
        if (endScreen != null)
            endScreen.SetActive(true);
        else
            Debug.LogWarning("End screen is null - assign it in the inspector");
            
        if (startButton != null)
            startButton.gameObject.SetActive(true);
            
        // Update final score text with only green accuracy percentage
        if (finalScoreText != null)
        {
            float greenPercentage = greenSectionScript != null ? greenSectionScript.GreenPercentage : 0f;
            
            finalScoreText.text = $"You scored a percentage of {greenPercentage:F1}%\n\n" +
                                 GetPerformanceRating(greenPercentage);
            finalScoreText.gameObject.SetActive(true);
            Debug.Log($"Final score displayed: {greenPercentage:F1}%");
        }
        else
        {
            Debug.LogWarning("Final score text is null - assign it in the inspector");
        }
    }

    private void OnCountdownTick(int count)
    {
        if (countdownText != null)
        {
            if (count > 0)
            {
                countdownText.text = count.ToString();
            }
            else
            {
                countdownText.text = "GO!";
            }
        }
    }

    // Performance rating based on green percentage
    private string GetPerformanceRating(float greenPercentage)
    {
        if (greenPercentage >= 90f)
            return "PERFECT BREWING! ";
        else if (greenPercentage >= 75f)
            return "EXCELLENT BREWING! ";
        else if (greenPercentage >= 60f)
            return "GOOD BREWING! ";
        else if (greenPercentage >= 40f)
            return "FAIR BREWING ";
        else if (greenPercentage >= 20f)
            return "NEEDS IMPROVEMENT ";
        else
            return "KEEP PRACTICING! ";
    }

    /// <summary>
    /// Manually update the displays (useful for when values change infrequently)
    /// </summary>
    public void RefreshDisplays()
    {
        UpdateCounterDisplays();
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Reset all counters to zero (call this when starting a new game)
    /// </summary>
    public void ResetCounters()
    {
        if (gameManager != null)
            gameManager.ResetGame();
        else
        {
            // Fallback if no game manager
            if (greenSectionScript != null)
                greenSectionScript.ResetCounter();
                
            if (ringScript != null)
                ringScript.ResetCounter();
        }
            
        UpdateCounterDisplays();
        UpdateTimerDisplay();
        
        // Show start screen again when resetting
        if (startScreen != null)
            startScreen.SetActive(true);
            
        // Hide end screen when resetting
        if (endScreen != null)
            endScreen.SetActive(false);
    }

    // Button event handlers
    public void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked!");
        if (gameManager != null)
        {
            Debug.Log("Game manager found, calling OnStartButtonPressed");
            gameManager.OnStartButtonPressed();
        }
        else
        {
            Debug.LogError("Game manager is NULL! Make sure FermentationGameManager is in the scene");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnCountdownStart.RemoveListener(OnCountdownStart);
            gameManager.OnGameStart.RemoveListener(OnGameStart);
            gameManager.OnGameEnd.RemoveListener(OnGameEnd);
            gameManager.OnCountdownTick.RemoveListener(OnCountdownTick);
        }
    }
}