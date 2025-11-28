using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BottleTimingZone : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonDisplayText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI missedText;
    [SerializeField] private BottleGameManager gameManager;
    
    private bool isInZone = false;
    private GameObject currentBottle = null;
    private bool bottleCaught = false;
    private Key requiredKey;
    private Key[] possibleKeys = { Key.W, Key.A, Key.S, Key.D };
    
    private int score = 0;
    private int missedBottles = 0;

    void Update()
    {
        if (Keyboard.current != null)
        {
            // Check for the required key input using new Input System
            if (Keyboard.current[requiredKey].wasPressedThisFrame)
            {
                if (isInZone && currentBottle != null)
                {
                    OnSuccessfulTiming();
                }
                else
                {
                    OnFailedTiming();
                }
            }
            // Check if any wrong key was pressed
            else if (isInZone && currentBottle != null)
            {
                foreach (Key key in possibleKeys)
                {
                    if (key != requiredKey && Keyboard.current[key].wasPressedThisFrame)
                    {
                        OnWrongKeyPressed();
                        break;
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bottle"))
        {
            isInZone = true;
            currentBottle = other.gameObject;
            bottleCaught = false;
            
            // Select a random key
            requiredKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
            
            // Update UI to show the required key
            if (buttonDisplayText != null)
            {
                buttonDisplayText.text = requiredKey.ToString();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bottle") && currentBottle == other.gameObject)
        {
            isInZone = false;
            // Only call OnMissedBottle if the bottle wasn't caught
            if (!bottleCaught)
            {
                OnMissedBottle();
            }
            currentBottle = null;
            
            // Clear UI text
            if (buttonDisplayText != null)
            {
                buttonDisplayText.text = "";
            }
        }
    }

    void OnSuccessfulTiming()
    {
        Debug.Log("Perfect! You caught the bottle!");
        // Add your success logic here (points, effects, etc.)
        score++;
        UpdateScoreUI();
        
        bottleCaught = true;
        Destroy(currentBottle);
        currentBottle = null;
        isInZone = false;
        
        // Notify game manager
        if (gameManager != null)
        {
            gameManager.OnBottleProcessed();
            gameManager.PlayBottleCapAnimation();
        }
    }

    void OnFailedTiming()
    {
        Debug.Log("Missed! No bottle in the zone.");
        // Add your failure logic here
    }

    void OnWrongKeyPressed()
    {
        Debug.Log("Wrong key! You pressed the wrong button!");
        missedBottles++;
        UpdateMissedUI();
        
        bottleCaught = true; // Prevent counting as missed again when exiting zone
        Destroy(currentBottle);
        currentBottle = null;
        isInZone = false;
        
        // Clear UI text
        if (buttonDisplayText != null)
        {
            buttonDisplayText.text = "";
        }
        
        // Notify game manager
        if (gameManager != null)
        {
            gameManager.OnBottleProcessed();
        }
    }

    void OnMissedBottle()
    {
        Debug.Log("Bottle passed without pressing space!");
        // Add logic for missing the bottle completely
        missedBottles++;
        UpdateMissedUI();
        
        // Notify game manager
        if (gameManager != null)
        {
            gameManager.OnBottleProcessed();
        }
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
    
    void UpdateMissedUI()
    {
        if (missedText != null)
        {
            missedText.text = "Missed: " + missedBottles;
        }
    }
    
    public int GetScore()
    {
        return score;
    }
    
    public int GetMissed()
    {
        return missedBottles;
    }
}
