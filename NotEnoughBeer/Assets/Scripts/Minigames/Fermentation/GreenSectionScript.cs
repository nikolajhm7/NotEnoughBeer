using UnityEngine;

public class GreenSectionScript : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float outerRadius = 5f;
    [SerializeField] private float innerRadius = 4.2f;
    [SerializeField] private int segments = 20; // Fewer segments for just the arc
    [SerializeField] private Material greenMaterial;
    
    [Header("Green Section Settings")]
    [SerializeField] public float startAngle = 120f; // Start angle of the green section
    [SerializeField] public float endAngle = 140f;   // End angle of the green section

    [Header("Detection & Counter")]
    [SerializeField] private float greenSectionCount = 0f;
    [SerializeField] private float countIncrement = 2f; // Higher points for green section
    [SerializeField] private NeedleScript needleScript;
    [SerializeField] private bool showDebugInfo = false; // Toggle debug messages

    private float lastOuterRadius;
    private float lastInnerRadius;
    private float lastStartAngle;
    private float lastEndAngle;
    private int lastSegments;
    private bool wasInGreenSection = false;

    void Start()
    {
        // Auto-find needle if not assigned
        if (needleScript == null)
            needleScript = FindFirstObjectByType<NeedleScript>();
            
        CreateGreenArc();
        // Store initial values
        StoreLastValues();
    }

    void Update()
    {
        // Check if values have changed
        if (HasValuesChanged())
        {
            CreateGreenArc();
            StoreLastValues();
        }
        
        // Check needle collision with green section
        if (needleScript != null)
        {
            CheckNeedleInGreenSection();
        }
    }

    void CheckNeedleInGreenSection()
    {
        float needleAngle = needleScript.currentAngle;
        bool inGreenSection = IsAngleInRange(needleAngle, startAngle, endAngle);
        
        if (inGreenSection)
        {
            greenSectionCount += countIncrement * Time.deltaTime;
            
            if (!wasInGreenSection)
            {
                if (showDebugInfo)
                    Debug.Log($"Needle entered GREEN section at angle {needleAngle:F1}°! Bonus points! Green count: {greenSectionCount:F1}");
                wasInGreenSection = true;
            }
        }
        else
        {
            if (wasInGreenSection)
            {
                if (showDebugInfo)
                    Debug.Log($"Needle left GREEN section! Final green count: {greenSectionCount:F1}");
                wasInGreenSection = false;
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

    bool HasValuesChanged()
    {
        return outerRadius != lastOuterRadius || 
               innerRadius != lastInnerRadius || 
               startAngle != lastStartAngle || 
               endAngle != lastEndAngle ||
               segments != lastSegments;
    }

    void StoreLastValues()
    {
        lastOuterRadius = outerRadius;
        lastInnerRadius = innerRadius;
        lastStartAngle = startAngle;
        lastEndAngle = endAngle;
        lastSegments = segments;
    }

    void OnValidate()
    {
        // This runs when values change in the Inspector (even in edit mode)
        if (Application.isPlaying && lineRenderer != null)
        {
            CreateGreenArc();
        }
    }

    void CreateGreenArc()
    {
        // Setup LineRenderer
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Use default green material if none assigned
        if (greenMaterial == null)
        {
            greenMaterial = new Material(Shader.Find("Sprites/Default"));
            greenMaterial.color = Color.green;
        }
        
        lineRenderer.material = greenMaterial; 
        lineRenderer.startWidth = outerRadius - innerRadius;
        lineRenderer.endWidth = outerRadius - innerRadius;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = false; // This is an arc, not a full circle
        lineRenderer.positionCount = segments + 1;

        // Convert needle coordinate system to visual coordinate system
        // Based on observations: Up=175°, Left=265° in needle coordinates
        // Standard coordinates: Up=90°, Left=180°
        // Needle appears to have an 85° offset: needle = standard + 85°
        // So: standard = needle - 85°
        
        // Generate green arc points
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float needleAngle = Mathf.Lerp(startAngle, endAngle, t);
            // Convert needle angle to standard visual coordinate system
            float visualAngle = (needleAngle - 85f) * Mathf.Deg2Rad;
            float radius = (outerRadius + innerRadius) / 2f;
            float x = Mathf.Cos(visualAngle) * radius;
            float y = Mathf.Sin(visualAngle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0.01f)); // Slightly forward to appear on top
        }
    }

    // Public method to check if needle is in green section (for RingScript)
    public bool IsNeedleInGreenSection(float needleAngle)
    {
        return IsAngleInRange(needleAngle, startAngle, endAngle);
    }

    // Alternative method name in case of compilation issues
    public bool CheckIfNeedleInGreen(float needleAngle)
    {
        return IsAngleInRange(needleAngle, startAngle, endAngle);
    }

    // Public method to get the current green section count
    public float GetGreenSectionCount()
    {
        return greenSectionCount;
    }

    // Public method to reset the green section count
    public void ResetGreenSectionCount()
    {
        greenSectionCount = 0f;
    }
}