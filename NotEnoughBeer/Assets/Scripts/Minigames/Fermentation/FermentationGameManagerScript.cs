using UnityEngine;
using UnityEngine.Events;

public class FermentationGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private float countdownDuration = 3f;
    
    [Header("Game State")]
    [SerializeField] private bool gameIsActive = false;
    [SerializeField] private bool gameIsStarted = false;
    [SerializeField] private float currentGameTime = 0f;
    [SerializeField] private float countdownTime = 0f;
    [SerializeField] private bool isCountingDown = false;
    
    [Header("Script References")]
    [SerializeField] private FermentationUI fermentationUI;
    [SerializeField] private GreenSectionScript greenSectionScript;
    [SerializeField] private RingScript ringScript;
    [SerializeField] private NeedleScript needleScript;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject howToPlayPanel;
    
    [Header("Events")]
    public UnityEvent OnGameStart;
    public UnityEvent OnGameEnd;
    public UnityEvent OnCountdownStart;
    public UnityEvent<int> OnCountdownTick;
    
    public static FermentationGameManager Instance;
    
    public bool GameIsActive => gameIsActive;
    public bool GameIsStarted => gameIsStarted;
    public float CurrentGameTime => currentGameTime;
    public float RemainingTime => gameDuration - currentGameTime;
    public float GameDuration => gameDuration;
    public bool IsCountingDown => isCountingDown;
    public float CountdownTime => countdownTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (fermentationUI == null)
            fermentationUI = FindFirstObjectByType<FermentationUI>();
            
        if (greenSectionScript == null)
            greenSectionScript = FindFirstObjectByType<GreenSectionScript>();
            
        if (ringScript == null)
            ringScript = FindFirstObjectByType<RingScript>();
            
        if (needleScript == null)
            needleScript = FindFirstObjectByType<NeedleScript>();
    }

    private void Update()
    {
        if (isCountingDown)
        {
            UpdateCountdown();
        }
        else if (gameIsActive)
        {
            UpdateGameTimer();
        }
    }

    private void UpdateCountdown()
    {
        countdownTime -= Time.deltaTime;
        
        int currentTick = Mathf.CeilToInt(countdownTime);
        if (currentTick <= 3 && currentTick >= 0)
        {
            OnCountdownTick?.Invoke(currentTick);
        }
        
        if (countdownTime <= 0f)
        {
            isCountingDown = false;
            StartGame();
        }
    }

    private void UpdateGameTimer()
    {
        currentGameTime += Time.deltaTime;
        
        if (currentGameTime >= gameDuration)
        {
            EndGame();
        }
    }

    public void StartCountdown()
    {
        if (gameIsStarted) return;
        
        Debug.Log("Starting countdown...");
        
        ResetGame();
        
        isCountingDown = true;
        countdownTime = countdownDuration;
        gameIsStarted = true;
        
        Debug.Log("Invoking OnCountdownStart event");
        OnCountdownStart?.Invoke();
    }

    private void StartGame()
    {
        Debug.Log("Game started!");
        
        gameIsActive = true;
        currentGameTime = 0f;
        
        OnGameStart?.Invoke();
    }

    public void EndGame()
    {
        if (!gameIsActive) return;
        
        Debug.Log("Game ended!");
        
        gameIsActive = false;
        
        float greenScore = greenSectionScript != null ? greenSectionScript.GreenSectionCount : 0f;
        float redScore = ringScript != null ? ringScript.RedRingCount : 0f;
        float greenPercentage = greenSectionScript != null ? greenSectionScript.GreenPercentage : 0f;
        
        Debug.Log($"Final Scores: {greenPercentage:F1}%");
        
        OnGameEnd?.Invoke();

        if (MinigameBridge.Instance != null)
        {
            MinigameBridge.Instance.FinishMinigame(greenPercentage);
        }
        else
        {
            Debug.LogWarning("No MinigameBridge found � staying in Fermentation scene.");
        }
    }

    public void ResetGame()
    {
        Debug.Log("Resetting game...");
        
        gameIsActive = false;
        gameIsStarted = false;
        currentGameTime = 0f;
        countdownTime = 0f;
        isCountingDown = false;
        
        if (greenSectionScript != null)
            greenSectionScript.ResetCounter();
            
        if (ringScript != null)
            ringScript.ResetCounter();
    }

    public void ForceStopGame()
    {
        if (gameIsActive || isCountingDown)
        {
            EndGame();
        }
    }

    public void OnStartButtonPressed()
    {
        if (!gameIsStarted)
        {
            StartCountdown();
        }
    }

    public void OnResetButtonPressed()
    {
        ResetGame();
    }
    
    public void OnHowToPlayButtonPressed()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }
        
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
    }
    
    public void OnHowToPlayBackButtonPressed()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
        
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    public bool CanCountScore()
    {
        return gameIsActive && !isCountingDown;
    }

    public bool CanMoveNeedle()
    {
        return gameIsActive || isCountingDown;
    }
}