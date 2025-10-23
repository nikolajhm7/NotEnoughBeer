using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class BottlingGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float countdownDuration = 3f; // 3-2-1-GO countdown
    
    [Header("Bottle Spawning")]
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private Transform bottleSpawnPoint;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnTimer = 0f;
    [SerializeField] private int maxBottlesOnBelt = 5;
    
    [Header("Game State")]
    [SerializeField] private bool gameIsActive = false;
    [SerializeField] private bool gameIsStarted = false;
    [SerializeField] private float currentGameTime = 0f;
    [SerializeField] private float countdownTime = 0f;
    [SerializeField] private bool isCountingDown = false;
    
    [Header("Scoring")]
    [SerializeField] private int score = 0;
    [SerializeField] private int totalBottles = 0;
    [SerializeField] private int successfulBottles = 0;
    [SerializeField] private int failedBottles = 0;
    
    [Header("Button Prompt Settings")]
    [SerializeField] private int numberOfButtons = 8;
    [SerializeField] private float buttonPromptDuration = 2f;
    
    [Header("Script References")]
    [SerializeField] private BottlingUI bottlingUI;
    [SerializeField] private Transform beerCrate;
    
    [Header("Events")]
    public UnityEvent OnGameStart;
    public UnityEvent OnGameEnd;
    public UnityEvent OnCountdownStart;
    public UnityEvent<int> OnCountdownTick; // Passes countdown number (3, 2, 1, 0 for GO)
    public UnityEvent<int> OnScoreChanged;
    
    // Active bottles tracking
    private List<Bottle> activeBottles = new List<Bottle>();
    private Bottle currentPromptBottle = null;
    private int currentRequiredButton = -1;
    private float promptTimer = 0f;
    private bool isWaitingForInput = false;
    
    public static BottlingGameManager Instance;
    
    // Public properties
    public bool GameIsActive => gameIsActive;
    public bool GameIsStarted => gameIsStarted;
    public float CurrentGameTime => currentGameTime;
    public float RemainingTime => gameDuration - currentGameTime;
    public float GameDuration => gameDuration;
    public bool IsCountingDown => isCountingDown;
    public float CountdownTime => countdownTime;
    public int Score => score;
    public int TotalBottles => totalBottles;
    public int SuccessfulBottles => successfulBottles;
    public int FailedBottles => failedBottles;
    public bool IsWaitingForInput => isWaitingForInput;
    public int CurrentRequiredButton => currentRequiredButton;
    public float PromptTimeRemaining => buttonPromptDuration - promptTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Auto-find script references if not assigned
        if (bottlingUI == null)
            bottlingUI = FindFirstObjectByType<BottlingUI>();
            
        if (beerCrate == null)
        {
            GameObject crateObject = GameObject.FindGameObjectWithTag("BeerCrate");
            if (crateObject != null)
                beerCrate = crateObject.transform;
        }
        
        if (bottleSpawnPoint == null)
        {
            GameObject spawnObject = GameObject.FindGameObjectWithTag("BottleSpawnPoint");
            if (spawnObject != null)
                bottleSpawnPoint = spawnObject.transform;
        }
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
            UpdateBottleSpawning();
            UpdateButtonPrompt();
            HandleInput();
        }
    }

    private void UpdateCountdown()
    {
        countdownTime -= Time.deltaTime;
        
        // Check for countdown ticks
        int currentTick = Mathf.CeilToInt(countdownTime);
        if (currentTick <= 3 && currentTick >= 0)
        {
            OnCountdownTick?.Invoke(currentTick);
        }
        
        if (countdownTime <= 0f)
        {
            // Countdown finished, start the game
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

    private void UpdateBottleSpawning()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval && activeBottles.Count < maxBottlesOnBelt)
        {
            SpawnBottle();
            spawnTimer = 0f;
        }
    }

    private void UpdateButtonPrompt()
    {
        if (!isWaitingForInput) return;
        
        promptTimer += Time.deltaTime;
        
        if (promptTimer >= buttonPromptDuration)
        {
            // Time's up - player failed
            HandleButtonInput(-1); // -1 indicates timeout
        }
    }

    private void HandleInput()
    {
        if (!isWaitingForInput) return;
        
        // Check for number key inputs (1-8)
        for (int i = 1; i <= numberOfButtons; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                HandleButtonInput(i);
                return;
            }
        }
    }

    public void StartCountdown()
    {
        if (gameIsStarted) return;
        
        gameIsStarted = true;
        isCountingDown = true;
        countdownTime = countdownDuration;
        
        OnCountdownStart?.Invoke();
    }

    private void StartGame()
    {
        gameIsActive = true;
        currentGameTime = 0f;
        score = 0;
        totalBottles = 0;
        successfulBottles = 0;
        failedBottles = 0;
        
        OnGameStart?.Invoke();
        OnScoreChanged?.Invoke(score);
    }

    private void EndGame()
    {
        gameIsActive = false;
        isWaitingForInput = false;
        
        // Stop all bottles
        foreach (Bottle bottle in activeBottles)
        {
            if (bottle != null)
                bottle.StopBottle();
        }
        
        OnGameEnd?.Invoke();
    }

    private void SpawnBottle()
    {
        if (bottlePrefab == null || bottleSpawnPoint == null) return;
        
        GameObject newBottleObj = Instantiate(bottlePrefab, bottleSpawnPoint.position, bottleSpawnPoint.rotation);
        Bottle bottle = newBottleObj.GetComponent<Bottle>();
        
        if (bottle != null)
        {
            // Set up bottle
            bottle.SetBeerCrate(beerCrate);
            bottle.OnBottleReachPromptZone += OnBottleReachPromptZone;
            bottle.OnBottleReachCrate += OnBottleReachCrate;
            bottle.OnBottleDestroyed += OnBottleDestroyed;
            
            activeBottles.Add(bottle);
            totalBottles++;
        }
    }

    private void OnBottleReachPromptZone(Bottle bottle)
    {
        if (isWaitingForInput) return; // Already handling another bottle
        
        currentPromptBottle = bottle;
        currentRequiredButton = Random.Range(1, numberOfButtons + 1);
        isWaitingForInput = true;
        promptTimer = 0f;
        
        // Notify UI to show button prompt
        if (bottlingUI != null)
            bottlingUI.ShowButtonPrompt(currentRequiredButton, buttonPromptDuration);
    }

    private void OnBottleReachCrate(Bottle bottle)
    {
        // Bottle reached the crate - check if it was processed correctly
        if (bottle.IsProcessed && bottle.IsSuccessful)
        {
            // Success!
            AddScore(10);
            successfulBottles++;
        }
        else
        {
            // Failed or not processed
            failedBottles++;
        }
    }

    private void OnBottleDestroyed(Bottle bottle)
    {
        // Remove from active bottles list
        if (activeBottles.Contains(bottle))
        {
            activeBottles.Remove(bottle);
        }
        
        // Clear current prompt if this was the prompt bottle
        if (currentPromptBottle == bottle)
        {
            currentPromptBottle = null;
            isWaitingForInput = false;
            
            // Hide UI prompt
            if (bottlingUI != null)
                bottlingUI.HideButtonPrompt();
        }
    }

    private void HandleButtonInput(int buttonPressed)
    {
        if (currentPromptBottle == null) return;
        
        bool success = (buttonPressed == currentRequiredButton);
        
        // Process the bottle
        currentPromptBottle.ProcessBottle(success);
        
        // Clear prompt state
        isWaitingForInput = false;
        currentPromptBottle = null;
        currentRequiredButton = -1;
        
        // Hide UI prompt
        if (bottlingUI != null)
            bottlingUI.HideButtonPrompt();
        
        // Give immediate feedback
        if (success)
        {
            if (bottlingUI != null)
                bottlingUI.ShowSuccessFeedback();
        }
        else
        {
            if (bottlingUI != null)
                bottlingUI.ShowFailureFeedback();
        }
    }

    private void AddScore(int points)
    {
        score += points;
        OnScoreChanged?.Invoke(score);
    }

    // Public methods for UI buttons
    public void StartGameButton()
    {
        StartCountdown();
    }

    public void RestartGame()
    {
        // Reset all game state
        gameIsActive = false;
        gameIsStarted = false;
        isCountingDown = false;
        isWaitingForInput = false;
        
        // Clear all bottles
        foreach (Bottle bottle in activeBottles)
        {
            if (bottle != null)
                Destroy(bottle.gameObject);
        }
        activeBottles.Clear();
        
        // Reset counters
        currentGameTime = 0f;
        spawnTimer = 0f;
        promptTimer = 0f;
    }

    // Debug method
    public void ForceSpawnBottle()
    {
        if (gameIsActive)
            SpawnBottle();
    }
}