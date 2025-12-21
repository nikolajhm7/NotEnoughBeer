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
        centerPosition = transform.localPosition;
        centerRotation = transform.localRotation;
    }
    
    void Update()
    {
        if (isStirring)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, stirSpeed, Time.deltaTime / accelerationTime);
            
            stirAngle += currentSpeed * 360f * Time.deltaTime;
            if (stirAngle >= 360f)
            {
                stirAngle -= 360f;
            }
            
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
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime / accelerationTime);
            
            if (currentSpeed > 0.01f)
            {
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
                transform.localPosition = Vector3.Lerp(transform.localPosition, centerPosition, Time.deltaTime * 5f);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, centerRotation, Time.deltaTime * 5f);
            }
        }
    }
    

    public void StartStirring()
    {
        isStirring = true;
        Debug.Log("Started stirring");
    }
    
    public void StopStirring()
    {
        isStirring = false;
        Debug.Log("Stopped stirring");
    }

    public bool IsStirring()
    {
        return isStirring;
    }
}
