using UnityEngine;

public class SpoonStirrer : MonoBehaviour
{
    [Header("Stirring Settings")]
    [Tooltip("How fast the spoon stirs (rotations per second)")]
    [Range(0.5f, 5f)]
    public float stirSpeed = 1.5f;
    
    [Tooltip("Radius of the circular stirring motion")]
    [Range(0.1f, 2f)]
    public float stirRadius = 0.5f;
    
    [Tooltip("How much the spoon tilts while stirring")]
    [Range(0f, 30f)]
    public float tiltAngle = 15f;
    
    [Header("Animation")]
    [Tooltip("How smoothly the spoon starts/stops stirring")]
    [Range(0.1f, 2f)]
    public float accelerationTime = 0.3f;
    
    private Vector3 centerPosition;
    private Quaternion centerRotation;
    private bool isStirring = false;
    private float currentSpeed = 0f;
    private float stirAngle = 0f;
    
    void Start()
    {
        // Store the initial position and rotation as the center point
        centerPosition = transform.localPosition;
        centerRotation = transform.localRotation;
    }
    
    void Update()
    {
        if (isStirring)
        {
            // Smoothly accelerate to full speed
            currentSpeed = Mathf.Lerp(currentSpeed, stirSpeed, Time.deltaTime / accelerationTime);
            
            // Update stir angle
            stirAngle += currentSpeed * 360f * Time.deltaTime;
            if (stirAngle >= 360f)
            {
                stirAngle -= 360f;
            }
            
            // Calculate circular motion
            float radians = stirAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * stirRadius,
                0f,
                Mathf.Sin(radians) * stirRadius
            );
            
            // Apply position
            transform.localPosition = centerPosition + offset;
            
            // Apply rotation with tilt
            float tiltX = Mathf.Sin(radians) * tiltAngle;
            float tiltZ = -Mathf.Cos(radians) * tiltAngle;
            Quaternion tilt = Quaternion.Euler(tiltX, 0f, tiltZ);
            transform.localRotation = centerRotation * tilt;
        }
        else
        {
            // Smoothly decelerate and return to center
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime / accelerationTime);
            
            if (currentSpeed > 0.01f)
            {
                // Continue moving in circle but slowing down
                stirAngle += currentSpeed * 360f * Time.deltaTime;
                float radians = stirAngle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(radians) * stirRadius,
                    0f,
                    Mathf.Sin(radians) * stirRadius
                );
                transform.localPosition = centerPosition + offset;
                
                float tiltX = Mathf.Sin(radians) * tiltAngle;
                float tiltZ = -Mathf.Cos(radians) * tiltAngle;
                Quaternion tilt = Quaternion.Euler(tiltX, 0f, tiltZ);
                transform.localRotation = centerRotation * tilt;
            }
            else
            {
                // Return to rest position
                transform.localPosition = Vector3.Lerp(transform.localPosition, centerPosition, Time.deltaTime * 5f);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, centerRotation, Time.deltaTime * 5f);
            }
        }
    }
    
    /// <summary>
    /// Start stirring the spoon
    /// </summary>
    public void StartStirring()
    {
        isStirring = true;
        Debug.Log("Started stirring");
    }
    
    /// <summary>
    /// Stop stirring the spoon
    /// </summary>
    public void StopStirring()
    {
        isStirring = false;
        Debug.Log("Stopped stirring");
    }
    
    /// <summary>
    /// Check if currently stirring
    /// </summary>
    public bool IsStirring()
    {
        return isStirring;
    }
}
