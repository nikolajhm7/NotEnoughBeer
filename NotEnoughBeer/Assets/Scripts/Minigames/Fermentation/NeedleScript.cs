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
    }

    void Update()
    {
        rotationTimer += Time.deltaTime;
        if (rotationTimer >= rotationInterval)
        {
            float angleDelta = (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) ? -1f : 1f;
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
