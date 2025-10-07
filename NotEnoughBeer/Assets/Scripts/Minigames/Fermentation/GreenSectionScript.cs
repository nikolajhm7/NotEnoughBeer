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

    [Header("Randomizer Settings")]
    [SerializeField] private bool enableRandomizer = false;
    [SerializeField] private float randomizeInterval = 3f; // How often to randomize (seconds)
    [SerializeField] private float minSectionSize = 10f; // Minimum size of green section in degrees
    [SerializeField] private float maxSectionSize = 30f; // Maximum size of green section in degrees
    [SerializeField] private float minAngle = 60f; // Minimum angle where green section can appear
    [SerializeField] private float maxAngle = 300f; // Maximum angle where green section can appear
    [SerializeField] private bool smoothTransition = true; // Smooth transition between positions
    [SerializeField] private float transitionSpeed = 2f; // Speed of smooth transition

    [Header("Detection & Counter")]
    [SerializeField] private float greenSectionCount = 0f;
    [SerializeField] private float countIncrement = 2f; // Higher points for green section
    [SerializeField] private NeedleScript needleScript;
    [SerializeField] private bool showDebugInfo = false; // Toggle debug messages
    
    // Percentage tracking variables
    private float totalGameTime = 0f;
    private float timeInGreenSection = 0f;
    
    // Public property to expose the counter value for UI
    public float GreenSectionCount => greenSectionCount;
    
    // Public property to expose the percentage
    public float GreenPercentage => totalGameTime > 0 ? (timeInGreenSection / totalGameTime) * 100f : 0f;

    // Randomizer variables
    private float randomizeTimer = 0f;
    private float targetStartAngle;
    private float targetEndAngle;
    private float currentStartAngle;
    private float currentEndAngle;

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
            
        // Initialize randomizer values
        if (enableRandomizer)
        {
            currentStartAngle = startAngle;
            currentEndAngle = endAngle;
            targetStartAngle = startAngle;
            targetEndAngle = endAngle;
            randomizeTimer = randomizeInterval; // Start with first randomization
        }
            
        CreateGreenArc();
        // Store initial values
        StoreLastValues();
    }

    void Update()
    {
        // Track total game time for percentage calculation
        totalGameTime += Time.deltaTime;
        
        // Handle randomizer
        if (enableRandomizer)
        {
            HandleRandomizer();
        }
        
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
        // Use current angles (which may be interpolated for smooth transitions)
        float currentStart = enableRandomizer ? currentStartAngle : startAngle;
        float currentEnd = enableRandomizer ? currentEndAngle : endAngle;
        bool inGreenSection = IsAngleInRange(needleAngle, currentStart, currentEnd);
        
        if (inGreenSection)
        {
            greenSectionCount += countIncrement * Time.deltaTime;
            timeInGreenSection += Time.deltaTime; // Track time in green section
            
            if (!wasInGreenSection)
            {
                if (showDebugInfo)
                    Debug.Log($"Needle entered GREEN section at angle {needleAngle:F1}°! Bonus points! Green count: {greenSectionCount:F1}, Percentage: {GreenPercentage:F1}%");
                wasInGreenSection = true;
            }
        }
        else
        {
            if (wasInGreenSection)
            {
                if (showDebugInfo)
                    Debug.Log($"Needle left GREEN section! Final green count: {greenSectionCount:F1}, Percentage: {GreenPercentage:F1}%");
                wasInGreenSection = false;
            }
        }
    }
    
    void HandleRandomizer()
    {
        randomizeTimer += Time.deltaTime;
        
        // Time to generate new random position and size
        if (randomizeTimer >= randomizeInterval)
        {
            GenerateRandomGreenSection();
            randomizeTimer = 0f;
        }
        
        // Smooth transition to target position
        if (smoothTransition)
        {
            currentStartAngle = Mathf.Lerp(currentStartAngle, targetStartAngle, transitionSpeed * Time.deltaTime);
            currentEndAngle = Mathf.Lerp(currentEndAngle, targetEndAngle, transitionSpeed * Time.deltaTime);
            
            // Update the actual angles used for rendering
            startAngle = currentStartAngle;
            endAngle = currentEndAngle;
        }
        else
        {
            // Instant transition
            startAngle = targetStartAngle;
            endAngle = targetEndAngle;
            currentStartAngle = targetStartAngle;
            currentEndAngle = targetEndAngle;
        }
    }
    
    void GenerateRandomGreenSection()
    {
        // Generate random size
        float sectionSize = Random.Range(minSectionSize, maxSectionSize);
        
        // Generate random start position (ensuring it fits within bounds)
        float availableRange = maxAngle - minAngle - sectionSize;
        if (availableRange > 0)
        {
            float randomStart = Random.Range(minAngle, minAngle + availableRange);
            targetStartAngle = randomStart;
            targetEndAngle = randomStart + sectionSize;
            
            if (showDebugInfo)
                Debug.Log($"New green section: {targetStartAngle:F1}° to {targetEndAngle:F1}° (size: {sectionSize:F1}°)");
        }
        else
        {
            // Fallback if section is too big for the available range
            targetStartAngle = minAngle;
            targetEndAngle = maxAngle;
            
            if (showDebugInfo)
                Debug.Log($"Green section too big for range, using full available: {targetStartAngle:F1}° to {targetEndAngle:F1}°");
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
        // Use current angles if randomizer is enabled, otherwise use set angles
        float currentStart = enableRandomizer ? currentStartAngle : startAngle;
        float currentEnd = enableRandomizer ? currentEndAngle : endAngle;
        return IsAngleInRange(needleAngle, currentStart, currentEnd);
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
    
    // Alias method for UI compatibility
    public void ResetCounter()
    {
        greenSectionCount = 0f;
        totalGameTime = 0f;
        timeInGreenSection = 0f;
    }
}