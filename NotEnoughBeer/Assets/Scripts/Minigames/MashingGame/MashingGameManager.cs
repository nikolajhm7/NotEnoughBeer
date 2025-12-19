using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MashingGameManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Panel shown before the game starts")]
    public GameObject startPanel;
    
    [Tooltip("Panel showing how to play instructions")]
    public GameObject howToPlayPanel;
    
    [Tooltip("Panel shown during countdown")]
    public GameObject countdownPanel;
    
    [Tooltip("Panel with game UI (thermometer, score, etc)")]
    public GameObject gamePanel;
    
    [Tooltip("Panel shown when game ends")]
    public GameObject endPanel;
    
    [Header("UI Elements")]
    [Tooltip("Text displaying countdown (3, 2, 1, GO!)")]
    public TMPro.TextMeshProUGUI countdownText;
    
    [Tooltip("Text displaying final score on end screen")]
    public TMPro.TextMeshProUGUI finalScoreText;
    
    [Tooltip("Text displaying remaining time during gameplay")]
    public TMPro.TextMeshProUGUI timerText;
    
    [Header("Game References")]
    [Tooltip("Reference to the game scorer")]
    public MashingGameScorer gameScorer;
    
    [Tooltip("Reference to the log pile cycler")]
    public LogPileCycler logPileCycler;
    
    [Tooltip("Reference to the thermometer")]
    public Thermometer thermometer;
    
    [Header("Game Objects to Show/Hide")]
    [Tooltip("All game objects that should only be visible during gameplay")]
    public GameObject[] gameObjects;
    
    [Header("Settings")]
    [Tooltip("Countdown duration in seconds")]
    public float countdownDuration = 3f;
    
    [Tooltip("Game duration in seconds (0 = infinite)")]
    public float gameDuration = 30f;
    
    private GameState currentState = GameState.Start;
    private float gameTimer = 0f;
    
    private enum GameState
    {
        Start,
        Countdown,
        Playing,
        End
    }
    
    void Start()
    {
        // Try to find references if not assigned
        if (gameScorer == null)
        {
            gameScorer = FindAnyObjectByType<MashingGameScorer>();
        }
        
        if (logPileCycler == null)
        {
            logPileCycler = FindAnyObjectByType<LogPileCycler>();
        }
        
        if (thermometer == null)
        {
            thermometer = FindAnyObjectByType<Thermometer>();
        }
        
        // Show start screen
        ShowStartScreen();
    }
    
    void Update()
    {
        // Handle game timer if duration is set
        if (currentState == GameState.Playing && gameDuration > 0f)
        {
            gameTimer += Time.deltaTime;
            
            // Update timer display
            if (timerText != null)
            {
                float timeRemaining = Mathf.Max(0, gameDuration - gameTimer);
                timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}s";
            }
            
            if (gameTimer >= gameDuration)
            {
                EndGame();
            }
        }
    }
    
    /// <summary>
    /// Called when Start Game button is clicked
    /// </summary>
    public void OnStartButtonClicked()
    {
        StartCoroutine(StartCountdown());
    }
    
    /// <summary>
    /// Called when How to Play button is clicked
    /// </summary>
    public void OnHowToPlayButtonClicked()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }
    
    /// <summary>
    /// Called when Back button is clicked on how to play screen
    /// </summary>
    public void OnBackButtonClicked()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
    }
    
    /// <summary>
    /// Called when Continue button is clicked on end screen
    /// </summary>
    public void OnContinueButtonClicked()
    {
        // You can customize this - restart game, go to main menu, etc.
        ShowStartScreen();
    }
    
    /// <summary>
    /// Show the start screen
    /// </summary>
    private void ShowStartScreen()
    {
        currentState = GameState.Start;
        
        if (startPanel != null) startPanel.SetActive(true);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);
        
        // Hide all game objects
        SetGameObjectsActive(false);
        
        // Reset game state
        ResetGame();
    }
    
    /// <summary>
    /// Start the countdown sequence
    /// </summary>
    private IEnumerator StartCountdown()
    {
        currentState = GameState.Countdown;
        
        // Hide start screen, show countdown
        if (startPanel != null) startPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (endPanel != null) endPanel.SetActive(false);
        
        // Show game objects but keep components disabled
        SetGameObjectsActive(true);
        if (gameScorer != null) gameScorer.enabled = false;
        if (logPileCycler != null) logPileCycler.enabled = false;
        if (thermometer != null) thermometer.enabled = false;
        
        // Countdown from 3 to 1
        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }
            yield return new WaitForSeconds(1f);
        }
        
        // Show GO!
        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }
        yield return new WaitForSeconds(1f);
        
        // Start the game
        StartGame();
    }
    
    /// <summary>
    /// Start the actual game
    /// </summary>
    private void StartGame()
    {
        currentState = GameState.Playing;
        
        // Hide countdown, show game UI
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        
        // Show all game objects
        SetGameObjectsActive(true);
        
        // Enable game components
        if (gameScorer != null) gameScorer.enabled = true;
        if (logPileCycler != null) logPileCycler.enabled = true;
        if (thermometer != null) thermometer.enabled = true;
        
        gameTimer = 0f;
    }
    
    /// <summary>
    /// End the game and show results
    /// </summary>
    public void EndGame()
    {
        currentState = GameState.End;
        
        // Hide game UI, show end screen
        if (gamePanel != null) gamePanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(true);
        
        // Disable game components
        if (gameScorer != null) gameScorer.enabled = false;
        if (logPileCycler != null) logPileCycler.enabled = false;
        
        // Display final score
        if (finalScoreText != null && gameScorer != null)
        {
            finalScoreText.text = $"Final Score: {gameScorer.GetScoreInt()}";
        }
    }
    
    /// <summary>
    /// Reset the game to initial state
    /// </summary>
    private void ResetGame()
    {
        // Reset scorer
        if (gameScorer != null)
        {
            gameScorer.ResetScore();
            gameScorer.enabled = false;
        }
        
        // Reset log pile to 4 logs
        if (logPileCycler != null)
        {
            logPileCycler.currentPileIndex = 4;
            logPileCycler.enabled = false;
        }
        
        // Reset thermometer to 60 degrees
        if (thermometer != null)
        {
            thermometer.SetTemperature(60f);
            thermometer.enabled = false;
        }
        
        gameTimer = 0f;
    }
    
    /// <summary>
    /// Show or hide all game objects
    /// </summary>
    private void SetGameObjectsActive(bool active)
    {
        if (gameObjects != null)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
    }
}
