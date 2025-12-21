using UnityEngine;

public class RingScript : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float outerRadius = 5f;
    [SerializeField] private float innerRadius = 4.2f;
    [SerializeField] private int segments = 64;
    [SerializeField] private Material ringMaterial;
    
    [Header("Ring Detection")]
    [SerializeField] private float ringStartAngle = 50f;
    [SerializeField] private float ringEndAngle = 300f;
    
    [Header("Counters")]
    [SerializeField] private float redRingCount = 0f;
    [SerializeField] private float countIncrement = 2f;
    [SerializeField] private bool showDebugInfo = false;
    
    public float RedRingCount => redRingCount;
    
    [Header("Needle Reference")]
    [SerializeField] private NeedleScript needleScript;
    [SerializeField] private GreenSectionScript greenSectionScript;
    [SerializeField] private FermentationGameManager gameManager;

    private float lastOuterRadius;
    private float lastInnerRadius;
    private int lastSegments;
    private bool wasInRedRing = false;

    void Start()
    {
        if (needleScript == null)
            needleScript = FindFirstObjectByType<NeedleScript>();
            
        if (greenSectionScript == null)
            greenSectionScript = FindFirstObjectByType<GreenSectionScript>();
            
        if (gameManager == null)
            gameManager = FindFirstObjectByType<FermentationGameManager>();
            
        CreateRing();
        lastOuterRadius = outerRadius;
        lastInnerRadius = innerRadius;
        lastSegments = segments;
    }

    void Update()
    {
        if (outerRadius != lastOuterRadius || innerRadius != lastInnerRadius || segments != lastSegments)
        {
            CreateRing();
            lastOuterRadius = outerRadius;
            lastInnerRadius = innerRadius;
            lastSegments = segments;
        }
        
        if (needleScript != null)
        {
            bool canCountScore = gameManager == null || gameManager.CanCountScore();
            if (canCountScore)
            {
                CheckNeedleCollision();
            }
        }
    }

    void CheckNeedleCollision()
    {
        float needleAngle = needleScript.currentAngle;
        
        bool inRedRing = IsAngleInRange(needleAngle, ringStartAngle, ringEndAngle);
        
        bool inGreenSection = false;
        if (greenSectionScript != null)
        {
            inGreenSection = greenSectionScript.IsNeedleInGreenSection(needleAngle);
        }
        
        if (inRedRing && !inGreenSection)
        {
            redRingCount += countIncrement * Time.deltaTime;
            
            if (!wasInRedRing)
            {
                if (showDebugInfo)
                    Debug.Log($"Needle entered red ring at angle {needleAngle:F1}°! Red count: {redRingCount:F1}");
                wasInRedRing = true;
            }
        }
        else
        {
            if (wasInRedRing)
            {
                if (showDebugInfo)
                {
                    string reason = inGreenSection ? "(entered green section)" : "(left red ring area)";
                    Debug.Log($"Needle left red ring {reason}! Final red count: {redRingCount:F1}");
                }
                wasInRedRing = false;
            }
        }
    }
    
    bool IsAngleInRange(float angle, float startAngle, float endAngle)
    {
        angle = NormalizeAngle(angle);
        startAngle = NormalizeAngle(startAngle);
        endAngle = NormalizeAngle(endAngle);
        
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
        if (Application.isPlaying && lineRenderer != null)
        {
            CreateRing();
        }
    }

    void CreateRing()
    {
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        if (ringMaterial == null)
        {
            ringMaterial = new Material(Shader.Find("Sprites/Default"));
            ringMaterial.color = Color.red;
        }
        
        lineRenderer.material = ringMaterial;
        lineRenderer.startWidth = outerRadius - innerRadius;
        lineRenderer.endWidth = outerRadius - innerRadius;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        
        lineRenderer.enabled = false;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * ((outerRadius + innerRadius) / 2f);
            float y = Mathf.Sin(angle) * ((outerRadius + innerRadius) / 2f);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    public float GetRedRingCount()
    {
        return redRingCount;
    }

    public void ResetRedRingCount()
    {
        redRingCount = 0f;
    }

    public float GetTotalCount()
    {
        float greenCount = greenSectionScript != null ? greenSectionScript.GetGreenSectionCount() : 0f;
        return redRingCount + greenCount;
    }

    public void ResetAllCounts()
    {
        redRingCount = 0f;
        if (greenSectionScript != null)
            greenSectionScript.ResetGreenSectionCount();
    }
    
    public void ResetCounter()
    {
        redRingCount = 0f;
    }
}
