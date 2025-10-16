using UnityEngine;
using UnityEngine.InputSystem;

public class NeedleScript : MonoBehaviour
{
    // Timer to control rotation speed
    public float rotationTimer = 0f;
    public float rotationInterval = 0.02f;

    public float currentAngle = 0f;

    [SerializeField] private float startingAngle = 210f;
    [SerializeField] private float minAngle = 60f;
    [SerializeField] private float maxAngle = 300f;
    [SerializeField] private float zeroAngleOffset = 70f;
    
    [Header("Speed Settings")]
    [SerializeField] private float normalSpeed = 1f; // Normal movement speed
    [SerializeField] private float speedBoostMultiplier = 2f; // Speed multiplier when W is pressed
    
    [Header("Game Manager")]
    [SerializeField] private FermentationGameManager gameManager;

    void Awake()
    {
        if (minAngle - zeroAngleOffset > maxAngle - zeroAngleOffset)
        {
            float temp = minAngle;
            minAngle = maxAngle;
            maxAngle = temp;
        }
        currentAngle = Mathf.Clamp(startingAngle, minAngle, maxAngle);
        transform.localEulerAngles = new Vector3(0f, 0f, currentAngle + zeroAngleOffset);
        
        // Auto-find game manager if not assigned
        if (gameManager == null)
            gameManager = FindFirstObjectByType<FermentationGameManager>();
    }

    void Update()
    {
        // Check if needle movement is allowed
        bool canMove = gameManager == null || gameManager.CanMoveNeedle();
        if (!canMove) return;
        
        rotationTimer += Time.deltaTime;
        if (rotationTimer >= rotationInterval)
        {
            // Check input states
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            bool wPressed = Keyboard.current != null && Keyboard.current.wKey.isPressed;
            
            // Determine base direction
            float angleDelta = spacePressed ? -1f : 1f;
            
            // Apply speed boost if W is pressed
            if (wPressed)
            {
                angleDelta *= speedBoostMultiplier;
            }
            else
            {
                angleDelta *= normalSpeed;
            }
            
            RotateNeedle(angleDelta);
            rotationTimer = 0f;
        }
    }

    void RotateNeedle(float angle)
    {
        float newAngle = currentAngle + angle;
        // Prevent moving below minAngle
        if (newAngle < minAngle)
        {
            currentAngle = minAngle;
        }
        // Prevent moving above maxAngle
        else if (newAngle > maxAngle)
        {
            currentAngle = maxAngle;
        }
        else
        {
            currentAngle = newAngle;
        }
        // Apply rotation
        transform.localEulerAngles = new Vector3(0f, 0f, currentAngle + zeroAngleOffset);
    }
}
