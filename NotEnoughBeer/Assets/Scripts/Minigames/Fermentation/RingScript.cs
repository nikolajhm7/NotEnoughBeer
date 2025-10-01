using UnityEngine;

public class RingScript : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float outerRadius = 5f;
    [SerializeField] private float innerRadius = 4.2f;
    [SerializeField] private int segments = 64;
    [SerializeField] private Material ringMaterial;
    
    [Header("Ring Detection")]
    [SerializeField] private float ringStartAngle = 50f; // Start angle of the red ring
    [SerializeField] private float ringEndAngle = 300f;   // End angle of the red ring
    
    [Header("Counters")]
    [SerializeField] private float redRingCount = 0f;
    [SerializeField] private float countIncrement = 1f; // Points per second
    
    [Header("Needle Reference")]
    [SerializeField] private NeedleScript needleScript;
    [SerializeField] private GreenSectionScript greenSectionScript; // Reference to green section

    private float lastOuterRadius;
    private float lastInnerRadius;
    private int lastSegments;
    private bool wasInRedRing = false;

    void Start()
    {
        // Auto-find needle if not assigned
        if (needleScript == null)
            needleScript = FindFirstObjectByType<NeedleScript>();
            
        // Auto-find green section if not assigned
        if (greenSectionScript == null)
            greenSectionScript = FindFirstObjectByType<GreenSectionScript>();
            
        CreateRing();
        // Store initial values
        lastOuterRadius = outerRadius;
        lastInnerRadius = innerRadius;
        lastSegments = segments;
    }

    void Update()
    {
        // Check if values have changed
        if (outerRadius != lastOuterRadius || innerRadius != lastInnerRadius || segments != lastSegments)
        {
            CreateRing();
            lastOuterRadius = outerRadius;
            lastInnerRadius = innerRadius;
            lastSegments = segments;
        }
        
        // Check needle collision with ring
        if (needleScript != null)
        {
            CheckNeedleCollision();
        }
    }

    void CheckNeedleCollision()
    {
        float needleAngle = needleScript.currentAngle;
        
        // Check if needle is within the red ring angle range
        bool inRedRing = IsAngleInRange(needleAngle, ringStartAngle, ringEndAngle);
        
        // Check if needle is in green section (green takes priority)
        bool inGreenSection = false;
        if (greenSectionScript != null)
        {
            inGreenSection = greenSectionScript.CheckIfNeedleInGreen(needleAngle);
        }
        
        // Only increment red ring counter if in red ring AND NOT in green section
        if (inRedRing && !inGreenSection)
        {
            redRingCount += countIncrement * Time.deltaTime;
            
            if (!wasInRedRing)
            {
                Debug.Log("Needle entered red ring!");
                wasInRedRing = true;
            }
        }
        else
        {
            if (wasInRedRing)
            {
                Debug.Log("Needle left red ring!");
                wasInRedRing = false;
            }
        }
    }
    
    bool IsAngleInRange(float angle, float startAngle, float endAngle)
    {
        // Normalize angles to 0-360 range
        angle = NormalizeAngle(angle);
        startAngle = NormalizeAngle(startAngle);
        endAngle = NormalizeAngle(endAngle);
        
        // Handle wrap-around case (e.g., 350° to 10°)
        if (startAngle > endAngle)
        {
            return angle >= startAngle || angle <= endAngle;
        }
        else
        {
            return angle >= startAngle && angle <= endAngle;
        }
    }
    
    float NormalizeAngle(float angle)
    {
        while (angle < 0) angle += 360f;
        while (angle >= 360) angle -= 360f;
        return angle;
    }

    void OnValidate()
    {
        // This runs when values change in the Inspector (even in edit mode)
        if (Application.isPlaying && lineRenderer != null)
        {
            CreateRing();
        }
    }

    void CreateRing()
    {
        // Setup LineRenderer
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Use default material if none assigned
        if (ringMaterial == null)
        {
            // Create a simple default material
            ringMaterial = new Material(Shader.Find("Sprites/Default"));
            ringMaterial.color = Color.red;
        }
        
        lineRenderer.material = ringMaterial;
        lineRenderer.startWidth = outerRadius - innerRadius;
        lineRenderer.endWidth = outerRadius - innerRadius;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        // Generate ring points
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * ((outerRadius + innerRadius) / 2f);
            float y = Mathf.Sin(angle) * ((outerRadius + innerRadius) / 2f);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}
