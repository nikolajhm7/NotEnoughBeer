using UnityEngine;
using UnityEngine.InputSystem;

public class NeedleScript : MonoBehaviour
{
    public float rotationTimer = 0f;
    public float rotationInterval = 0.02f;

    public float currentAngle = 0f;

    [SerializeField] private float startingAngle = 210f;
    [SerializeField] private float minAngle = 60f;
    [SerializeField] private float maxAngle = 300f;
    [SerializeField] private float zeroAngleOffset = 70f;
    
    [Header("Speed Settings")]
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float speedBoostMultiplier = 2f;
    
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
        
        if (gameManager == null)
            gameManager = FindFirstObjectByType<FermentationGameManager>();
    }

    void Update()
    {
        bool canMove = gameManager == null || gameManager.CanMoveNeedle();
        if (!canMove) return;
        
        rotationTimer += Time.deltaTime;
        if (rotationTimer >= rotationInterval)
        {
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            bool wPressed = Keyboard.current != null && Keyboard.current.wKey.isPressed;
            
            float angleDelta = spacePressed ? -1f : 1f;
            
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
        if (newAngle < minAngle)
        {
            currentAngle = minAngle;
        }
        else if (newAngle > maxAngle)
        {
            currentAngle = maxAngle;
        }
        else
        {
            currentAngle = newAngle;
        }
        transform.localEulerAngles = new Vector3(0f, 0f, currentAngle + zeroAngleOffset);
    }
}
