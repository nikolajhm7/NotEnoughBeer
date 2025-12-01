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
    
    [Header("Bottle Cap Putter")]
    [SerializeField] private Transform bottomPart;
    [SerializeField] private Transform secondBottomPart;
    [SerializeField] private Transform middlePart;
    [SerializeField] private float stepDistance = 0.02f;
    [SerializeField] private float moveDuration = 0.05f;
    [SerializeField] private float returnDuration = 0.5f;
    
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

        // Calculate score first
        int finalScore = 0;
        int missed = 0;
        if (timingZone != null)
        {
            finalScore = timingZone.GetScore();
            missed = timingZone.GetMissed();
        }

        // Turn that into a single float value for the batch system
        float scoreValue = Mathf.Max(0, finalScore - missed * 0.5f);

        // ===== If we came from a machine, report back and return to main =====
        if (MinigameBridge.Instance != null)
        {
            MinigameBridge.Instance.FinishMinigame(scoreValue);
            return; // we’re leaving this scene, no need to show endScreen
        }

        // ===== Fallback: old behaviour if no bridge exists =====
        // Disable game components
        if (bottleSpawner != null) bottleSpawner.enabled = false;
        if (timingZone != null) timingZone.enabled = false;

        // Hide game UI, show end screen
        if (gameUI != null) gameUI.SetActive(false);
        if (endScreen != null) endScreen.SetActive(true);

        // Display final score
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore + "\nMissed: " + missed;
        }
    }


    public void PlayBottleCapAnimation()
    {
        Debug.Log("PlayBottleCapAnimation called!");
        StartCoroutine(AnimateBottleCap());
    }
    
    private System.Collections.IEnumerator AnimateBottleCap()
    {
        // Store original positions
        Vector3 bottomOriginal = bottomPart != null ? bottomPart.localPosition : Vector3.zero;
        Vector3 secondBottomOriginal = secondBottomPart != null ? secondBottomPart.localPosition : Vector3.zero;
        Vector3 middleOriginal = middlePart != null ? middlePart.localPosition : Vector3.zero;
        
        float elapsed;
        
        // Step 1: Move all three parts down 0.1f
        Vector3 bottomStep1 = bottomOriginal + Vector3.down * stepDistance;
        Vector3 secondBottomStep1 = secondBottomOriginal + Vector3.down * stepDistance;
        Vector3 middleStep1 = middleOriginal + Vector3.down * stepDistance;
        
        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            if (bottomPart != null)
                bottomPart.localPosition = Vector3.Lerp(bottomOriginal, bottomStep1, t);
            if (secondBottomPart != null)
                secondBottomPart.localPosition = Vector3.Lerp(secondBottomOriginal, secondBottomStep1, t);
            if (middlePart != null)
                middlePart.localPosition = Vector3.Lerp(middleOriginal, middleStep1, t);
            
            yield return null;
        }
        
        // Step 2: Move SecondBottom and Bottom down another 0.1f
        Vector3 bottomStep2 = bottomStep1 + Vector3.down * stepDistance;
        Vector3 secondBottomStep2 = secondBottomStep1 + Vector3.down * stepDistance;
        
        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            if (bottomPart != null)
                bottomPart.localPosition = Vector3.Lerp(bottomStep1, bottomStep2, t);
            if (secondBottomPart != null)
                secondBottomPart.localPosition = Vector3.Lerp(secondBottomStep1, secondBottomStep2, t);
            
            yield return null;
        }
        
        // Step 3: Move only Bottom down another 0.1f
        Vector3 bottomStep3 = bottomStep2 + Vector3.down * stepDistance;
        
        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            if (bottomPart != null)
                bottomPart.localPosition = Vector3.Lerp(bottomStep2, bottomStep3, t);
            
            yield return null;
        }
        
        // Hold briefly at the bottom
        yield return new WaitForSeconds(0.1f);
        
        // Return all parts to original positions slowly
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            
            if (bottomPart != null)
                bottomPart.localPosition = Vector3.Lerp(bottomStep3, bottomOriginal, t);
            if (secondBottomPart != null)
                secondBottomPart.localPosition = Vector3.Lerp(secondBottomStep2, secondBottomOriginal, t);
            if (middlePart != null)
                middlePart.localPosition = Vector3.Lerp(middleStep1, middleOriginal, t);
            
            yield return null;
        }
        
        // Ensure final positions are exact
        if (bottomPart != null)
            bottomPart.localPosition = bottomOriginal;
        if (secondBottomPart != null)
            secondBottomPart.localPosition = secondBottomOriginal;
        if (middlePart != null)
            middlePart.localPosition = middleOriginal;
    }
}
