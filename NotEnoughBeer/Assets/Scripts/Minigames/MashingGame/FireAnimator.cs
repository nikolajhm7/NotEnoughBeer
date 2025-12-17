using UnityEngine;
using UnityEngine.UI;

public class FireAnimator : MonoBehaviour
{
    [Header("Fire Sprites")]
    [Tooltip("Assign all 3 fire sprites from Fire.psd (Fire_0, Fire_1, Fire_2)")]
    public Sprite[] fireSprites = new Sprite[3];
    
    [Header("Animation Settings")]
    [Tooltip("How many frames per second the fire animation plays")]
    [Range(1f, 30f)]
    public float framesPerSecond = 12f;
    
    [Tooltip("Should the fire animation play automatically?")]
    public bool autoPlay = true;
    
    [Header("Current Frame")]
    [Tooltip("Current frame index (0-2). This auto-updates during animation")]
    [Range(0, 2)]
    public int currentFrameIndex = 0;
    
    private SpriteRenderer spriteRenderer;
    private Image imageComponent;
    private float timer = 0f;
    private bool isPlaying = false;
    
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
            Debug.LogError("FireAnimator requires either a SpriteRenderer or Image component!");
        }
    }
    
    void Start()
    {
        // Set the initial sprite
        UpdateFireSprite();
        
        if (autoPlay)
        {
            Play();
        }
    }
    
    void Update()
    {
        if (isPlaying)
        {
            timer += Time.deltaTime;
            
            // Calculate when to switch to next frame
            float frameTime = 1f / framesPerSecond;
            
            if (timer >= frameTime)
            {
                timer -= frameTime;
                NextFrame();
            }
        }
    }
    
    /// <summary>
    /// Updates the displayed sprite based on currentFrameIndex
    /// </summary>
    public void UpdateFireSprite()
    {
        // Validate index
        if (currentFrameIndex < 0 || currentFrameIndex >= fireSprites.Length)
        {
            Debug.LogWarning($"Invalid frame index: {currentFrameIndex}. Must be between 0 and {fireSprites.Length - 1}");
            return;
        }
        
        // Validate sprite exists
        if (fireSprites[currentFrameIndex] == null)
        {
            Debug.LogWarning($"No sprite assigned at index {currentFrameIndex}");
            return;
        }
        
        // Update the appropriate component
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = fireSprites[currentFrameIndex];
        }
        else if (imageComponent != null)
        {
            imageComponent.sprite = fireSprites[currentFrameIndex];
        }
    }
    
    /// <summary>
    /// Advance to the next frame in the animation
    /// </summary>
    public void NextFrame()
    {
        currentFrameIndex = (currentFrameIndex + 1) % fireSprites.Length;
        UpdateFireSprite();
    }
    
    /// <summary>
    /// Go to the previous frame in the animation
    /// </summary>
    public void PreviousFrame()
    {
        currentFrameIndex--;
        if (currentFrameIndex < 0)
        {
            currentFrameIndex = fireSprites.Length - 1;
        }
        UpdateFireSprite();
    }
    
    /// <summary>
    /// Set the fire to a specific frame (0-2)
    /// </summary>
    public void SetFrame(int index)
    {
        currentFrameIndex = Mathf.Clamp(index, 0, fireSprites.Length - 1);
        UpdateFireSprite();
    }
    
    /// <summary>
    /// Start playing the fire animation
    /// </summary>
    public void Play()
    {
        isPlaying = true;
        timer = 0f;
    }
    
    /// <summary>
    /// Stop playing the fire animation
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }
    
    /// <summary>
    /// Pause the fire animation (keeps current frame)
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }
    
    /// <summary>
    /// Resume the fire animation from current frame
    /// </summary>
    public void Resume()
    {
        isPlaying = true;
    }
    
    /// <summary>
    /// Reset to the first frame
    /// </summary>
    public void Reset()
    {
        currentFrameIndex = 0;
        timer = 0f;
        UpdateFireSprite();
    }
    
    /// <summary>
    /// Check if the animation is currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
}
