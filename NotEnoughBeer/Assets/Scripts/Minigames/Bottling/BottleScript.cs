using UnityEngine;

public class Bottle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    
    [Header("Bottle State")]
    [SerializeField] private bool isMoving = true;
    [SerializeField] private bool isProcessed = false; // Has player interacted with this bottle
    [SerializeField] private bool isSuccessful = false; // Did player succeed
    
    [Header("Button Prompt Settings")]
    [SerializeField] private float promptTriggerX = 0f; // X position where button prompt appears (middle of screen)
    [SerializeField] private bool hasTriggeredPrompt = false;
    
    [Header("Destruction Settings")]
    [SerializeField] private float destroyAfterDistance = 1500f; // Destroy bottle if it goes too far
    
    [Header("References")]
    [SerializeField] private Transform beerCrate; // Will be set by spawn manager
    [SerializeField] private SpriteRenderer bottleSprite;
    
    // Events for game manager
    public System.Action<Bottle> OnBottleReachPromptZone;
    public System.Action<Bottle> OnBottleReachCrate;
    public System.Action<Bottle> OnBottleDestroyed;
    
    // Properties
    public bool IsProcessed => isProcessed;
    public bool IsSuccessful => isSuccessful;
    public bool IsMoving => isMoving;
    
    private Vector3 startPosition;
    private BottlingGameManager gameManager;

    private void Start()
    {
        startPosition = transform.position;
        gameManager = FindFirstObjectByType<BottlingGameManager>();
        
        // Auto-find components if not assigned
        if (bottleSprite == null)
            bottleSprite = GetComponent<SpriteRenderer>();
            
        // Find beer crate if not assigned
        if (beerCrate == null)
        {
            GameObject crateObject = GameObject.FindGameObjectWithTag("BeerCrate");
            if (crateObject != null)
                beerCrate = crateObject.transform;
        }
    }

    private void Update()
    {
        if (!isMoving) return;
        
        // Move the bottle
        MoveBottle();
        
        // Check for prompt zone trigger
        CheckPromptZone();
        
        // Check if bottle reached crate
        CheckCrateReached();
        
        // Check if bottle should be destroyed (went too far)
        CheckDestroyDistance();
    }

    private void MoveBottle()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void CheckPromptZone()
    {
        if (hasTriggeredPrompt) return;
        
        // Trigger prompt when bottle reaches specific X position (middle of screen)
        if (transform.position.x >= promptTriggerX)
        {
            Debug.Log("Bottle triggered prompt at X: " + transform.position.x);
            hasTriggeredPrompt = true;
            OnBottleReachPromptZone?.Invoke(this);
        }
    }

    private void CheckCrateReached()
    {
        // For visual-only crate, we'll check if bottle has traveled far enough
        // or reached a specific end position instead of actual crate distance
        
        // Option 1: Check if bottle reached a far-right position (like X = 10)
        if (transform.position.x >= 10f)
        {
            ReachCrate();
        }
        
        // Option 2: Alternatively, disable this check entirely if you don't need it
        // since bottles will be destroyed by distance anyway
    }

    private void CheckDestroyDistance()
    {
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);
        
        if (distanceFromStart >= destroyAfterDistance)
        {
            DestroyBottle();
        }
    }

    private void ReachCrate()
    {
        OnBottleReachCrate?.Invoke(this);
        
        if (isSuccessful)
        {
            // Bottle successfully bottled - animate into crate
            AnimateIntoCrate();
        }
        else
        {
            // Bottle failed - animate falling off
            AnimateFallOff();
        }
    }

    private void AnimateIntoCrate()
    {
        // Simple version - just destroy for now
        // TODO: Add animation moving into crate
        isMoving = false;
        
        // You can add animation here later
        DestroyBottle(0.1f);
    }

    private void AnimateFallOff()
    {
        // Simple version - just destroy for now
        // TODO: Add falling animation
        isMoving = false;
        
        // You can add falling animation here later
        DestroyBottle(0.1f);
    }

    public void ProcessBottle(bool successful)
    {
        isProcessed = true;
        isSuccessful = successful;
        
        // Visual feedback
        if (successful)
        {
            // Green tint or checkmark
            if (bottleSprite != null)
                bottleSprite.color = Color.green;
        }
        else
        {
            // Red tint or X mark
            if (bottleSprite != null)
                bottleSprite.color = Color.red;
        }
    }

    public void StopBottle()
    {
        isMoving = false;
    }

    public void StartBottle()
    {
        isMoving = true;
    }

    private void DestroyBottle(float delay = 0f)
    {
        OnBottleDestroyed?.Invoke(this);
        
        if (delay > 0f)
        {
            Invoke(nameof(DestroyImmediate), delay);
        }
        else
        {
            DestroyImmediate();
        }
    }

    private void DestroyImmediate()
    {
        Destroy(gameObject);
    }

    // Public method for game manager to set crate reference
    public void SetBeerCrate(Transform crate)
    {
        beerCrate = crate;
    }

    // Public method to set move speed (useful for difficulty scaling)
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw prompt trigger line in scene view for debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(promptTriggerX, -10, 0), new Vector3(promptTriggerX, 10, 0));
        
        // Draw destroy distance
        Gizmos.color = Color.red;
        Vector3 destroyPoint = transform.position + (moveDirection * destroyAfterDistance);
        Gizmos.DrawWireSphere(destroyPoint, 0.5f);
    }
}