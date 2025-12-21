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
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
        if (fireManager == null)
        {
            fireManager = FindObjectOfType<FireManager>();
        }
        
        UpdateLogPileSprite();
        
        decayTimer = decayInterval;
    }
    
    void Update()
    {
        if (autoDecay && decayInterval > 0)
        {
            if (stopAtZeroLogs && currentPileIndex <= 0)
            {
                return;
            }
            
            decayTimer -= Time.deltaTime;
            
            if (decayTimer <= 0f)
            {
                RemoveOneLog();
                
                decayTimer = decayInterval;
            }
        }
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateLogPileSprite();
        }
    }
    
    public void UpdateLogPileSprite()
    {

        if (currentPileIndex == 0)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
            }
            else if (imageComponent != null)
            {
                imageComponent.enabled = false;
            }
            
            if (fireManager != null)
            {
                fireManager.UpdateFiresBasedOnLogs(0);
            }
            return;
        }
        
        if (imageComponent != null && !imageComponent.enabled)
        {
            imageComponent.enabled = true;
        }
        
        int spriteIndex = GetSpriteIndexForLogCount(currentPileIndex);
        
        if (spriteIndex < 0 || spriteIndex >= logPileSprites.Length)
        {
            Debug.LogWarning($"Invalid sprite index: {spriteIndex} for {currentPileIndex} logs");
            return;
        }
        
        if (logPileSprites[spriteIndex] == null)
        {
            Debug.LogWarning($"No sprite assigned at sprite index {spriteIndex}");
            return;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = logPileSprites[spriteIndex];
        }
        else if (imageComponent != null)
        {
            imageComponent.sprite = logPileSprites[spriteIndex];
        }
        
        if (fireManager != null)
        {
            fireManager.UpdateFiresBasedOnLogs(GetCurrentLogCount());
        }
    }
    
    public void SetLogPile(int logCount)
    {
        currentPileIndex = Mathf.Clamp(logCount, 0, 5);
        UpdateLogPileSprite();
    }
    
    public void NextLogPile()
    {
        currentPileIndex = (currentPileIndex + 1) % logPileSprites.Length;
        UpdateLogPileSprite();
    }

    public void PreviousLogPile()
    {
        currentPileIndex--;
        if (currentPileIndex < 0)
        {
            currentPileIndex = logPileSprites.Length - 1;
        }
        UpdateLogPileSprite();
    }
    
    public int GetCurrentLogCount()
    {
        return currentPileIndex;
    }
    
    private int GetSpriteIndexForLogCount(int logCount)
    {
        switch (logCount)
        {
            case 1: return 1;
            case 2: return 0;
            case 3: return 4;
            case 4: return 2;
            case 5: return 3;
            default: return -1;
        }
    }

    public void RemoveOneLog()
    {
        if (currentPileIndex <= 0)
        {
            return;
        }
        
        currentPileIndex--;
        UpdateLogPileSprite();
        Debug.Log($"Log removed: now at {currentPileIndex} logs");
    }

    public void AddOneLog()
    {
        if (currentPileIndex >= 5)
        {
            Debug.Log("Already at maximum logs (5)");
            return;
        }
        
        currentPileIndex++;
        UpdateLogPileSprite();
        Debug.Log($"Log added: now at {currentPileIndex} logs");
    }
    
    public void ResetDecayTimer()
    {
        decayTimer = decayInterval;
    }

    public float GetRemainingDecayTime()
    {
        return decayTimer;
    }
}
