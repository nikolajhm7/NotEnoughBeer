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

    [Tooltip("Panel shown during gameplay")]
    public GameObject gamePanel;

    [Tooltip("Panel shown when game ends")]
    public GameObject endPanel;

    [Header("UI Elements")]
    [Tooltip("Text displaying countdown numbers")]
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

    [Header("Game Settings")]
    [Tooltip("Countdown duration in seconds")]
    public float countdownDuration = 3f;

    [Tooltip("Game duration in seconds (0 = infinite)")]
    public float gameDuration = 30f;

    private GameState currentState = GameState.Start;
    private float gameTimer = 0f;

    [Header("End Game")]
    public float endScreenDelay = 10f;

    private Coroutine _endRoutine;
    private bool _ending = false;


    // ✅ Prevent double-finishing (timer end + button click, etc.)
    private bool _resultSent = false;

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

        ShowStartScreen();
    }

    void Update()
    {
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
    /// Called when Start button is clicked
    /// </summary>
    public void OnStartButtonClicked()
    {
        StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        currentState = GameState.Countdown;

        if (startPanel != null) startPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);

        float countdown = countdownDuration;

        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(countdown).ToString();

            countdown -= Time.deltaTime;
            yield return null;
        }

        if (countdownText != null)
            countdownText.text = "GO!";

        yield return new WaitForSeconds(0.5f);

        StartGameplay();
    }

    void StartGameplay()
    {
        currentState = GameState.Playing;
        _ending = false;
        _resultSent = false;

        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (endPanel != null) endPanel.SetActive(false);

        gameTimer = 0f;
        _resultSent = false;

        if (gameScorer != null)
        {
            gameScorer.enabled = true;
            gameScorer.currentScore = 0f;
        }

        if (logPileCycler != null)
        {
            logPileCycler.enabled = true;
            logPileCycler.currentPileIndex = 4; // reset to 4 logs (your default)
        }

        if (thermometer != null)
        {
            thermometer.enabled = true;
            thermometer.SetTemperature(60f);
        }
    }

    /// <summary>
    /// Called when How To Play button is clicked
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
        // If we reached the end screen, continue means "finish and return"
        if (currentState == GameState.End)
        {
            EndGame();
            return;
        }

        ShowStartScreen();
    }

    /// <summary>
    /// Show the start screen
    /// </summary>
    private void ShowStartScreen()
    {
        currentState = GameState.Start;
        _ending = false;
        _resultSent = false;
        if (_endRoutine != null) { StopCoroutine(_endRoutine); _endRoutine = null; }

        if (startPanel != null) startPanel.SetActive(true);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);

        // Disable game components
        if (gameScorer != null) gameScorer.enabled = false;
        if (logPileCycler != null) logPileCycler.enabled = false;
        if (thermometer != null) thermometer.enabled = false;

        gameTimer = 0f;
        _resultSent = false;
    }

    IEnumerator DelayedFinishRoutine()
    {
        float score = 0f;
        if (gameScorer != null)
            score = Mathf.Clamp(gameScorer.GetScore(), 0f, 100f);

        // Optional: show countdown on end screen if you want
        // Example:
        // if (finalScoreText != null)
        //     finalScoreText.text += $"\nReturning in {endScreenDelay:0}s...";

        yield return new WaitForSeconds(endScreenDelay);

        if (!_resultSent)
        {
            _resultSent = true;

            if (MinigameBridge.Instance != null)
                MinigameBridge.Instance.FinishMinigame(score);
            else
                Debug.LogWarning("[MashingGame] No MinigameBridge found - staying in Mashing scene.");
        }
    }


    /// <summary>
    /// End the game and show results (AND return to main scene like other minigames)
    /// </summary>
    public void EndGame()
    {
        if (_ending) return;        // prevent double trigger
        _ending = true;

        currentState = GameState.End;

        // Hide game UI, show end screen
        if (gamePanel != null) gamePanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(true);

        // Disable game components
        if (gameScorer != null) gameScorer.enabled = false;
        if (logPileCycler != null) logPileCycler.enabled = false;

        // Display final score
        if (finalScoreText != null && gameScorer != null)
            finalScoreText.text = $"Final Score: {gameScorer.GetScoreInt()}";

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

        // Start delayed finish
        if (_endRoutine != null) StopCoroutine(_endRoutine);
        _endRoutine = StartCoroutine(DelayedFinishRoutine());
    }

}
