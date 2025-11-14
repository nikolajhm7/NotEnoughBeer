using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BottleTimingZone : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonDisplayText;
    
    private bool isInZone = false;
    private GameObject currentBottle = null;
    private bool bottleCaught = false;
    private Key requiredKey;
    private Key[] possibleKeys = { Key.Q, Key.W, Key.E, Key.R };

    void Update()
    {
        // Check for the required key input using new Input System
        if (Keyboard.current != null && Keyboard.current[requiredKey].wasPressedThisFrame)
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
        bottleCaught = true;
        Destroy(currentBottle);
        currentBottle = null;
        isInZone = false;
    }

    void OnFailedTiming()
    {
        Debug.Log("Missed! No bottle in the zone.");
        // Add your failure logic here
    }

    void OnMissedBottle()
    {
        Debug.Log("Bottle passed without pressing space!");
        // Add logic for missing the bottle completely
    }
}
