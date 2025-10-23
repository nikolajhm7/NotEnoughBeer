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
    [SerializeField] private float promptZoneDistance = 3f; // Distance from crate to show button prompt
    [SerializeField] private bool hasTriggeredPrompt = false;
    
    [Header("Destruction Settings")]
    [SerializeField] private float destroyAfterDistance = 10f; // Destroy bottle if it goes too far
    
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
        if (hasTriggeredPrompt || beerCrate == null) return;
        
        float distanceToCrate = Vector3.Distance(transform.position, beerCrate.position);
        
        if (distanceToCrate <= promptZoneDistance)
        {
            hasTriggeredPrompt = true;
            OnBottleReachPromptZone?.Invoke(this);
        }
    }

    private void CheckCrateReached()
    {
        if (beerCrate == null) return;
        
        float distanceToCrate = Vector3.Distance(transform.position, beerCrate.position);
        
        // If bottle is very close to crate
        if (distanceToCrate <= 0.5f)
        {
            ReachCrate();
        }
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
        // Draw prompt zone in scene view for debugging
        if (beerCrate != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(beerCrate.position, promptZoneDistance);
        }
        
        // Draw destroy distance
        Gizmos.color = Color.red;
        Vector3 destroyPoint = transform.position + (moveDirection * destroyAfterDistance);
        Gizmos.DrawWireSphere(destroyPoint, 0.5f);
    }
}