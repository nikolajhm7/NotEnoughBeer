using UnityEngine;
using UnityEngine.UI;

public class LogPileCycler : MonoBehaviour
{
    [Header("Log Pile Sprites")]
    [Tooltip("Assign all 5 log pile sprites from LogPiles.psd (LogPiles_0 through LogPiles_4)")]
    public Sprite[] logPileSprites = new Sprite[5];
    
    [Header("Current Pile")]
    [Tooltip("Current pile index (0-5 = number of logs). Change this to cycle through different piles")]
    [Range(0, 5)]
    public int currentPileIndex = 4;
    
    [Header("Fire Integration")]
    [Tooltip("Optional: Reference to FireManager to auto-update fires when logs change")]
    public FireManager fireManager;
    
    [Header("Auto Decay Settings")]
    [Tooltip("Should logs automatically decay over time?")]
    public bool autoDecay = true;
    
    [Tooltip("Time in seconds before one log is removed")]
    public float decayInterval = 3f;
    
    [Tooltip("Should decay stop at 0 logs or continue?")]
    public bool stopAtZeroLogs = true;
    
    private SpriteRenderer spriteRenderer;
    private Image imageComponent;
    private float decayTimer = 0f;
    
    void Awake()
    {
        // Try to get SpriteRenderer first (for GameObject with SpriteRenderer)
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // If no SpriteRenderer, try to get Image component (for UI Image)
        if (spriteRenderer == null)
        {
            imageComponent = GetComponent<Image>();
        }
        
        if (spriteRenderer == null && imageComponent == null)
        {
            Debug.LogError("LogPileCycler requires either a SpriteRenderer or Image component!");
        }
    }
    
    void Start()
    {
        // Try to find FireManager if not assigned
        if (fireManager == null)
        {
            fireManager = FindObjectOfType<FireManager>();
        }
        
        // Set the initial sprite
        UpdateLogPileSprite();
        
        // Reset decay timer
        decayTimer = decayInterval;
    }
    
    void Update()
    {
        // Handle auto decay
        if (autoDecay && decayInterval > 0)
        {
            // Check if we should stop decaying
            if (stopAtZeroLogs && currentPileIndex <= 0)
            {
                return;
            }
            
            decayTimer -= Time.deltaTime;
            
            if (decayTimer <= 0f)
            {
                // Remove one log
                RemoveOneLog();
                
                // Reset timer
                decayTimer = decayInterval;
            }
        }
    }
    
    void OnValidate()
    {
        // This allows you to see changes in the editor when you modify currentPileIndex
        if (Application.isPlaying)
        {
            UpdateLogPileSprite();
        }
    }
    
    /// <summary>
    /// Updates the displayed sprite based on currentPileIndex
    /// </summary>
    public void UpdateLogPileSprite()
    {
        // Handle no logs case (index 0)
        if (currentPileIndex == 0)
        {
            // Hide the sprite by setting it to null or disabling
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
            }
            else if (imageComponent != null)
            {
                imageComponent.enabled = false;
            }
            
            // Update fires
            if (fireManager != null)
            {
                fireManager.UpdateFiresBasedOnLogs(0);
            }
            return;
        }
        
        // Re-enable image component if it was disabled
        if (imageComponent != null && !imageComponent.enabled)
        {
            imageComponent.enabled = true;
        }
        
        // Get the sprite index based on log count
        int spriteIndex = GetSpriteIndexForLogCount(currentPileIndex);
        
        // Validate sprite index
        if (spriteIndex < 0 || spriteIndex >= logPileSprites.Length)
        {
            Debug.LogWarning($"Invalid sprite index: {spriteIndex} for {currentPileIndex} logs");
            return;
        }
        
        // Validate sprite exists
        if (logPileSprites[spriteIndex] == null)
        {
            Debug.LogWarning($"No sprite assigned at sprite index {spriteIndex}");
            return;
        }
        
        // Update the appropriate component
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = logPileSprites[spriteIndex];
        }
        else if (imageComponent != null)
        {
            imageComponent.sprite = logPileSprites[spriteIndex];
        }
        
        // Update fires if FireManager is connected
        if (fireManager != null)
        {
            fireManager.UpdateFiresBasedOnLogs(GetCurrentLogCount());
        }
    }
    
    /// <summary>
    /// Set the log pile to a specific log count (0-5)
    /// Index directly represents the number of logs
    /// </summary>
    public void SetLogPile(int logCount)
    {
        currentPileIndex = Mathf.Clamp(logCount, 0, 5);
        UpdateLogPileSprite();
    }
    
    /// <summary>
    /// Cycle to the next log pile
    /// </summary>
    public void NextLogPile()
    {
        currentPileIndex = (currentPileIndex + 1) % logPileSprites.Length;
        UpdateLogPileSprite();
    }
    
    /// <summary>
    /// Cycle to the previous log pile
    /// </summary>
    public void PreviousLogPile()
    {
        currentPileIndex--;
        if (currentPileIndex < 0)
        {
            currentPileIndex = logPileSprites.Length - 1;
        }
        UpdateLogPileSprite();
    }
    
    /// <summary>
    /// Get the number of logs in the current pile
    /// Index directly represents log count: 0 = no logs, 1 = 1 log, etc.
    /// </summary>
    public int GetCurrentLogCount()
    {
        return currentPileIndex;
    }
    
    /// <summary>
    /// Get the sprite index for a given log count
    /// Maps log count to the correct LogPiles sprite
    /// LogPiles_0 = 2 logs, LogPiles_1 = 1 log, LogPiles_2 = 4 logs, 
    /// LogPiles_3 = 5 logs, LogPiles_4 = 3 logs
    /// </summary>
    private int GetSpriteIndexForLogCount(int logCount)
    {
        switch (logCount)
        {
            case 1: return 1; // 1 log -> LogPiles_1 (sprite array index 1)
            case 2: return 0; // 2 logs -> LogPiles_0 (sprite array index 0)
            case 3: return 4; // 3 logs -> LogPiles_4 (sprite array index 4)
            case 4: return 2; // 4 logs -> LogPiles_2 (sprite array index 2)
            case 5: return 3; // 5 logs -> LogPiles_3 (sprite array index 3)
            default: return -1; // Invalid
        }
    }
    
    /// <summary>
    /// Remove one log from the pile (automatically cycles to next lower pile)
    /// 5 logs -> 4 logs -> 3 logs -> 2 logs -> 1 log -> 0 logs
    /// </summary>
    public void RemoveOneLog()
    {
        if (currentPileIndex <= 0)
        {
            // Already at 0 logs
            return;
        }
        
        currentPileIndex--;
        UpdateLogPileSprite();
        Debug.Log($"Log removed: now at {currentPileIndex} logs");
    }
    
    /// <summary>
    /// Add one log to the pile (automatically cycles to next higher pile)
    /// 0 logs -> 1 log -> 2 logs -> 3 logs -> 4 logs -> 5 logs
    /// </summary>
    public void AddOneLog()
    {
        if (currentPileIndex >= 5)
        {
            // Already at max logs
            Debug.Log("Already at maximum logs (5)");
            return;
        }
        
        currentPileIndex++;
        UpdateLogPileSprite();
        Debug.Log($"Log added: now at {currentPileIndex} logs");
    }
    
    /// <summary>
    /// Reset the decay timer (useful when adding logs)
    /// </summary>
    public void ResetDecayTimer()
    {
        decayTimer = decayInterval;
    }
    
    /// <summary>
    /// Get the remaining time until next decay
    /// </summary>
    public float GetRemainingDecayTime()
    {
        return decayTimer;
    }
}
