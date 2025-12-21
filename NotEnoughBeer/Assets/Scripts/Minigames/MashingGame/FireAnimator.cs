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
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
            
            float frameTime = 1f / framesPerSecond;
            
            if (timer >= frameTime)
            {
                timer -= frameTime;
                NextFrame();
            }
        }
    }
    

    public void UpdateFireSprite()
    {
        if (currentFrameIndex < 0 || currentFrameIndex >= fireSprites.Length)
        {
            Debug.LogWarning($"Invalid frame index: {currentFrameIndex}. Must be between 0 and {fireSprites.Length - 1}");
            return;
        }
        
        if (fireSprites[currentFrameIndex] == null)
        {
            Debug.LogWarning($"No sprite assigned at index {currentFrameIndex}");
            return;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = fireSprites[currentFrameIndex];
        }
        else if (imageComponent != null)
        {
            imageComponent.sprite = fireSprites[currentFrameIndex];
        }
    }
    
    public void NextFrame()
    {
        currentFrameIndex = (currentFrameIndex + 1) % fireSprites.Length;
        UpdateFireSprite();
    }
    
    public void PreviousFrame()
    {
        currentFrameIndex--;
        if (currentFrameIndex < 0)
        {
            currentFrameIndex = fireSprites.Length - 1;
        }
        UpdateFireSprite();
    }
    
    public void SetFrame(int index)
    {
        currentFrameIndex = Mathf.Clamp(index, 0, fireSprites.Length - 1);
        UpdateFireSprite();
    }
    
    public void Play()
    {
        isPlaying = true;
        timer = 0f;
    }

    public void Stop()
    {
        isPlaying = false;
    }
    
    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        isPlaying = true;
    }

    public void Reset()
    {
        currentFrameIndex = 0;
        timer = 0f;
        UpdateFireSprite();
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}
