using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BottlingUI : MonoBehaviour
{
    [Header("Score UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bottlesProcessedText;
    [SerializeField] private TextMeshProUGUI successRateText;
    
    [Header("Game Timer UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalStatsText;
    
    [Header("Button Prompt UI")]
    [SerializeField] private GameObject buttonPromptPanel;
    [SerializeField] private TextMeshProUGUI buttonPromptText;
    [SerializeField] private Image buttonPromptBackground;
    [SerializeField] private Image promptTimerBar;
    [SerializeField] private TextMeshProUGUI promptInstructionText;
    
    [Header("Feedback UI")]
    [SerializeField] private GameObject successFeedback;
    [SerializeField] private GameObject failureFeedback;
    [SerializeField] private float feedbackDuration = 1f;
    
    [Header("Overlay UI")]
    [SerializeField] private GameObject blackOverlay;
    [SerializeField] private GameObject blurOverlay;
    
    [Header("Script References")]
    [SerializeField] private BottlingGameManager gameManager;
    
    [Header("Display Settings")]
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private string bottlesPrefix = "Bottles: ";
    [SerializeField] private string timerPrefix = "Time: ";
    [SerializeField] private string buttonPromptPrefix = "Press ";
    [SerializeField] private int decimalPlaces = 1;
    
    [Header("Button Prompt Colors")]
    [SerializeField] private Color normalPromptColor = Color.white;
    [SerializeField] private Color urgentPromptColor = Color.red;
    [SerializeField] private float urgentThreshold = 0.5f; // When to turn red (seconds remaining)
    
    public static BottlingUI Instance;
    
    private Coroutine feedbackCoroutine;
    private Coroutine promptTimerCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Auto-find scripts if not assigned
        if (gameManager == null)
            gameManager = FindFirstObjectByType<BottlingGameManager>();
        
        // Subscribe to game manager events
        if (gameManager != null)
        {
            gameManager.OnGameStart.AddListener(OnGameStart);
            gameManager.OnGameEnd.AddListener(OnGameEnd);
            gameManager.OnCountdownStart.AddListener(OnCountdownStart);
            gameManager.OnCountdownTick.AddListener(OnCountdownTick);
            gameManager.OnScoreChanged.AddListener(OnScoreChanged);
        }
        
        // Initialize UI
        InitializeUI();
    }

    private void Update()
    {
        if (gameManager == null) return;
        
        // Update timer display
        if (gameManager.GameIsActive)
        {
            UpdateTimerDisplay();
        }
        
        // Update countdown display
        if (gameManager.IsCountingDown)
        {
            UpdateCountdownDisplay();
        }
        
        // Update stats display
        UpdateStatsDisplay();
    }

    private void InitializeUI()
    {
        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Hide button prompt
        if (buttonPromptPanel != null)
            buttonPromptPanel.SetActive(false);
            
        // Hide feedback panels
        if (successFeedback != null)
            successFeedback.SetActive(false);
        if (failureFeedback != null)
            failureFeedback.SetActive(false);
            
        // Hide overlays initially - they should only show during countdown
        if (blackOverlay != null)
            blackOverlay.SetActive(false);
        if (blurOverlay != null)
            blurOverlay.SetActive(true); // Keep blur overlay visible until game starts
            
        // Show start button
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            
            // Set initial button text
            TextMeshProUGUI buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "Start Game";
        }
            
        // Clear countdown text
        if (countdownText != null)
            countdownText.text = "";
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        float remainingTime = gameManager.RemainingTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        
        timerText.text = timerPrefix + string.Format("{0:00}:{1:00}", minutes, seconds);
        
        // Change color when time is running low
        if (remainingTime <= 10f)
        {
            timerText.color = Color.red;
        }
        else if (remainingTime <= 30f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    private void UpdateCountdownDisplay()
    {
        if (countdownText == null) return;
        
        int countdownNumber = Mathf.CeilToInt(gameManager.CountdownTime);
        
        if (countdownNumber > 0)
        {
            countdownText.text = countdownNumber.ToString();
        }
        else
        {
            countdownText.text = "GO!";
        }
    }

    private void UpdateStatsDisplay()
    {
        // Update bottles processed text
        if (bottlesProcessedText != null)
        {
            bottlesProcessedText.text = bottlesPrefix + 
                gameManager.SuccessfulBottles + "/" + gameManager.TotalBottles;
        }
        
        // Update success rate
        if (successRateText != null && gameManager.TotalBottles > 0)
        {
            float successRate = (float)gameManager.SuccessfulBottles / gameManager.TotalBottles * 100f;
            successRateText.text = "Success: " + successRate.ToString("F1") + "%";
        }
    }

    public void ShowButtonPrompt(int buttonNumber, float duration)
    {
        if (buttonPromptPanel == null) return;
        
        buttonPromptPanel.SetActive(true);
        
        // Set button text
        if (buttonPromptText != null)
        {
            buttonPromptText.text = buttonPromptPrefix + buttonNumber.ToString();
        }
        
        // Set instruction text
        if (promptInstructionText != null)
        {
            promptInstructionText.text = "Press " + buttonNumber + " quickly!";
        }
        
        // Reset colors
        if (buttonPromptBackground != null)
            buttonPromptBackground.color = normalPromptColor;
            
        // Start timer bar animation
        if (promptTimerCoroutine != null)
            StopCoroutine(promptTimerCoroutine);
        promptTimerCoroutine = StartCoroutine(AnimatePromptTimer(duration));
    }

    public void HideButtonPrompt()
    {
        if (buttonPromptPanel != null)
            buttonPromptPanel.SetActive(false);
            
        if (promptTimerCoroutine != null)
        {
            StopCoroutine(promptTimerCoroutine);
            promptTimerCoroutine = null;
        }
    }

    private IEnumerator AnimatePromptTimer(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Update timer bar
            if (promptTimerBar != null)
            {
                promptTimerBar.fillAmount = 1f - progress;
            }
            
            // Change color when urgent
            if (elapsed >= duration - urgentThreshold)
            {
                if (buttonPromptBackground != null)
                    buttonPromptBackground.color = urgentPromptColor;
            }
            
            yield return null;
        }
        
        // Timer finished
        if (promptTimerBar != null)
            promptTimerBar.fillAmount = 0f;
    }

    public void ShowSuccessFeedback()
    {
        ShowFeedback(successFeedback);
    }

    public void ShowFailureFeedback()
    {
        ShowFeedback(failureFeedback);
    }

    private void ShowFeedback(GameObject feedbackObject)
    {
        if (feedbackObject == null) return;
        
        // Stop any existing feedback
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);
            
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(feedbackObject));
    }

    private IEnumerator ShowFeedbackCoroutine(GameObject feedbackObject)
    {
        feedbackObject.SetActive(true);
        yield return new WaitForSeconds(feedbackDuration);
        feedbackObject.SetActive(false);
        feedbackCoroutine = null;
    }

    // Event handlers
    private void OnGameStart()
    {
        if (startButton != null)
            startButton.gameObject.SetActive(false);
            
        if (countdownText != null)
            countdownText.text = "";
            
        if (blackOverlay != null)
            blackOverlay.SetActive(false);
            
        if (blurOverlay != null)
            blurOverlay.SetActive(false);
    }

    private void OnGameEnd()
    {
        // Hide button prompt
        HideButtonPrompt();
        
        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update final score
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + gameManager.Score;
            }
            
            // Update final stats
            if (finalStatsText != null)
            {
                float successRate = gameManager.TotalBottles > 0 ? 
                    (float)gameManager.SuccessfulBottles / gameManager.TotalBottles * 100f : 0f;
                    
                finalStatsText.text = string.Format(
                    "Bottles Processed: {0}/{1}\nSuccess Rate: {2:F1}%\nFailed: {3}",
                    gameManager.SuccessfulBottles,
                    gameManager.TotalBottles,
                    successRate,
                    gameManager.FailedBottles
                );
            }
        }
        
        // Show restart option
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            // Change button text to "Play Again" if possible
            TextMeshProUGUI buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "Play Again";
        }
    }

    private void OnCountdownStart()
    {
        if (blackOverlay != null)
            blackOverlay.SetActive(true);
    }

    private void OnCountdownTick(int tickNumber)
    {
        // Countdown display is handled in Update()
        // Could add sound effects or animations here
    }

    private void OnScoreChanged(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + newScore.ToString();
        }
    }

    // Public methods for UI buttons
    public void StartGame()
    {
        if (gameManager != null)
            gameManager.StartGameButton();
    }

    public void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
            
            // Reset UI
            InitializeUI();
            
            // Reset button text
            if (startButton != null)
            {
                TextMeshProUGUI buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = "Start Game";
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnGameStart.RemoveListener(OnGameStart);
            gameManager.OnGameEnd.RemoveListener(OnGameEnd);
            gameManager.OnCountdownStart.RemoveListener(OnCountdownStart);
            gameManager.OnCountdownTick.RemoveListener(OnCountdownTick);
            gameManager.OnScoreChanged.RemoveListener(OnScoreChanged);
        }
    }
}