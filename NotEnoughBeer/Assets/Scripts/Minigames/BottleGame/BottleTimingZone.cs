using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BottleTimingZone : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonDisplayText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI missedText;
    [SerializeField] private BottleGameManager gameManager;
    [SerializeField] private Sprite bottleWithCapSprite;
    [SerializeField] private Vector3 buttonOffset = new Vector3(0.6f, 2, 0);
    
    private bool isInZone = false;
    private GameObject currentBottle = null;
    private bool bottleCaught = false;
    private Key requiredKey = Key.W;
    private Key[] possibleKeys = { Key.W, Key.A, Key.S, Key.D };
    
    private int score = 0;
    private int missedBottles = 0;

    void Update()
    {
        // Update button text position to follow the bottle
        if (currentBottle != null && buttonDisplayText != null)
        {
            Vector3 worldPosition = currentBottle.transform.position + buttonOffset;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            buttonDisplayText.transform.position = screenPosition;
        }
        
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
                buttonDisplayText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bottle") && currentBottle == other.gameObject && !bottleCaught)
        {
            // Mark as caught immediately to prevent re-entry
            bottleCaught = true;
            isInZone = false;
            
            // Store reference before clearing
            GameObject missedBottle = currentBottle;
            currentBottle = null;
            
            // Call miss logic
            OnMissedBottle(missedBottle);
            
            // Clear UI text
            if (buttonDisplayText != null)
            {
                buttonDisplayText.text = "";
                buttonDisplayText.gameObject.SetActive(false);
            }
        }
        else if (other.CompareTag("Bottle") && currentBottle == other.gameObject)
        {
            // Bottle was already caught/processed, just clean up
            isInZone = false;
            currentBottle = null;
            
            // Clear UI text
            if (buttonDisplayText != null)
            {
                buttonDisplayText.text = "";
                buttonDisplayText.gameObject.SetActive(false);
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
        
        // Change the bottle sprite to show it has a cap
        if (currentBottle != null && bottleWithCapSprite != null)
        {
            SpriteRenderer spriteRenderer = currentBottle.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = bottleWithCapSprite;
            }
        }
        
        currentBottle = null;
        isInZone = false;
        
        // Hide button text
        if (buttonDisplayText != null)
        {
            buttonDisplayText.gameObject.SetActive(false);
        }
        
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
        
        // Make the bottle shake
        if (currentBottle != null)
        {
            BottleScript bottleScript = currentBottle.GetComponent<BottleScript>();
            if (bottleScript != null)
            {
                bottleScript.Shake();
            }
        }
        
        // Keep the bottle on the conveyor - just clear the reference
        currentBottle = null;
        isInZone = false;
        
        // Clear UI text
        if (buttonDisplayText != null)
        {
            buttonDisplayText.text = "";
            buttonDisplayText.gameObject.SetActive(false);
        }
        
        // Notify game manager
        if (gameManager != null)
        {
            gameManager.OnBottleProcessed();
        }
    }

    void OnMissedBottle(GameObject missedBottle)
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
