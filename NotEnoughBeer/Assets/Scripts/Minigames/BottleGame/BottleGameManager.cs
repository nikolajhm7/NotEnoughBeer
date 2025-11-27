using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public enum GameDifficulty
{
    Easy,
    Medium,
    Hard
}

public class BottleGameManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private TextMeshProUGUI bottlesRemainingText;
    
    [Header("Button Colors")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    
    [Header("Game Objects")]
    [SerializeField] private BottleMoveScript bottleSpawner;
    [SerializeField] private BottleTimingZone timingZone;
    
    [Header("Settings")]
    [SerializeField] private int countdownTime = 3;
    [SerializeField] private int totalBottles = 20;
    
    private bool gameStarted = false;
    private float currentCountdown;
    private bool countingDown = false;
    private GameDifficulty selectedDifficulty = GameDifficulty.Easy;
    private int bottlesRemaining = 0;

    void Start()
    {
        // Show start screen, hide everything else
        if (startScreen != null) startScreen.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (endScreen != null) endScreen.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        
        // Disable game components
        if (bottleSpawner != null) bottleSpawner.enabled = false;
        if (timingZone != null) timingZone.enabled = false;
        
        // Set initial difficulty selection visual
        UpdateDifficultyButtons();
    }

    void Update()
    {
        if (countingDown)
        {
            currentCountdown -= Time.deltaTime;
            
            if (currentCountdown > 0)
            {
                // Update countdown display
                if (countdownText != null)
                {
                    countdownText.text = Mathf.Ceil(currentCountdown).ToString();
                }
            }
            else
            {
                // Countdown finished, start game
                StartGame();
            }
        }
    }

    public void OnStartButtonPressed()
    {
        if (!gameStarted && !countingDown)
        {
            StartCountdown();
        }
    }

    public void SetDifficultyEasy()
    {
        selectedDifficulty = GameDifficulty.Easy;
        UpdateDifficultyButtons();
        Debug.Log("Difficulty set to Easy");
    }

    public void SetDifficultyMedium()
    {
        selectedDifficulty = GameDifficulty.Medium;
        UpdateDifficultyButtons();
        Debug.Log("Difficulty set to Medium");
    }

    public void SetDifficultyHard()
    {
        selectedDifficulty = GameDifficulty.Hard;
        UpdateDifficultyButtons();
        Debug.Log("Difficulty set to Hard");
    }
    
    void UpdateDifficultyButtons()
    {
        // Reset all buttons to normal color
        if (easyButton != null)
        {
            ColorBlock colors = easyButton.colors;
            colors.normalColor = normalColor;
            colors.selectedColor = normalColor;
            easyButton.colors = colors;
        }
        if (mediumButton != null)
        {
            ColorBlock colors = mediumButton.colors;
            colors.normalColor = normalColor;
            colors.selectedColor = normalColor;
            mediumButton.colors = colors;
        }
        if (hardButton != null)
        {
            ColorBlock colors = hardButton.colors;
            colors.normalColor = normalColor;
            colors.selectedColor = normalColor;
            hardButton.colors = colors;
        }
        
        // Highlight selected button
        Button selectedButton = null;
        switch (selectedDifficulty)
        {
            case GameDifficulty.Easy:
                selectedButton = easyButton;
                break;
            case GameDifficulty.Medium:
                selectedButton = mediumButton;
                break;
            case GameDifficulty.Hard:
                selectedButton = hardButton;
                break;
        }
        
        if (selectedButton != null)
        {
            ColorBlock colors = selectedButton.colors;
            colors.normalColor = selectedColor;
            colors.selectedColor = selectedColor;
            selectedButton.colors = colors;
        }
    }

    void StartCountdown()
    {
        countingDown = true;
        currentCountdown = countdownTime;
        
        // Hide start screen, show countdown
        if (startScreen != null) startScreen.SetActive(false);
        if (countdownText != null) 
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = countdownTime.ToString();
        }
    }

    void StartGame()
    {
        countingDown = false;
        gameStarted = true;
        
        // Initialize bottle counter
        bottlesRemaining = totalBottles;
        UpdateBottlesRemainingUI();
        
        // Hide countdown, show game UI
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        
        // Enable game components
        if (bottleSpawner != null) bottleSpawner.enabled = true;
        if (timingZone != null) timingZone.enabled = true;
        
        Debug.Log("Game Started!");
    }
    
    public void OnBottleProcessed()
    {
        bottlesRemaining--;
        UpdateBottlesRemainingUI();
        
        if (bottlesRemaining <= 0)
        {
            EndGame();
        }
    }
    
    void UpdateBottlesRemainingUI()
    {
        if (bottlesRemainingText != null)
        {
            bottlesRemainingText.text = "Bottles: " + bottlesRemaining;
        }
    }
    
    void EndGame()
    {
        Debug.Log("Game Ended!");
        // Disable game components
        if (bottleSpawner != null) bottleSpawner.enabled = false;
        if (timingZone != null) timingZone.enabled = false;
        
        // Hide game UI, show end screen
        if (gameUI != null) gameUI.SetActive(false);
        if (endScreen != null) endScreen.SetActive(true);
        
        // Display final score
        if (finalScoreText != null && timingZone != null)
        {
            int finalScore = timingZone.GetScore();
            int missed = timingZone.GetMissed();
            finalScoreText.text = "Final Score: " + finalScore + "\nMissed: " + missed;
        }
    }
}
