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
    
    [Header("Script References")]
    [SerializeField] private GreenSectionScript greenSectionScript;
    [SerializeField] private RingScript ringScript;
    [SerializeField] private FermentationGameManager gameManager;
    
    [Header("Display Settings")]
    [SerializeField] private string greenPrefix = "Green: ";
    [SerializeField] private string redPrefix = "Red: ";
    [SerializeField] private string percentagePrefix = "Green %: ";
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
        // Show start button, hide game over panel
        if (startButton != null)
            startButton.gameObject.SetActive(true);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
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
        if (startButton != null)
            startButton.gameObject.SetActive(false);
            
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);
    }

    private void OnGameStart()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void OnGameEnd()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
            
        if (startButton != null)
            startButton.gameObject.SetActive(true);
            
        // Update final score text
        if (finalScoreText != null)
        {
            float greenScore = greenSectionScript != null ? greenSectionScript.GreenSectionCount : 0f;
            float redScore = ringScript != null ? ringScript.RedRingCount : 0f;
            float greenPercentage = greenSectionScript != null ? greenSectionScript.GreenPercentage : 0f;
            
            finalScoreText.text = $"Final score %: {greenPercentage:F1}%";
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
    }

    // Button event handlers
    public void OnStartButtonClicked()
    {
        if (gameManager != null)
            gameManager.OnStartButtonPressed();
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