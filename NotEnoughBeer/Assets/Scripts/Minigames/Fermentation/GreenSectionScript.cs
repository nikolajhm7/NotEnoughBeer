using UnityEngine;

public class GreenSectionScript : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float outerRadius = 23.55f;
    [SerializeField] private float innerRadius = 23.95f;
    [SerializeField] private int segments = 20;
    [SerializeField] private Material greenMaterial;
    
    [Header("Green Section Settings")]
    [SerializeField] public float startAngle = 255f;
    [SerializeField] public float endAngle = 306f;

    [Header("Randomizer Settings")]
    [SerializeField] private bool enableRandomizer = true;
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float randomizeInterval = 3f;
    [SerializeField] private float minSectionSize = 35f;
    [SerializeField] private float maxSectionSize = 70f;
    [SerializeField] private float minAngle = 45f;
    [SerializeField] private float maxAngle = 308f;
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 0.5f;

    [Header("Detection & Counter")]
    [SerializeField] private float greenSectionCount = 0f;
    [SerializeField] private float countIncrement = 2f;
    [SerializeField] private NeedleScript needleScript;
    [SerializeField] private FermentationGameManager gameManager;
    [SerializeField] private bool showDebugInfo = false;
    
    private float totalGameTime = 0f;
    private float timeInGreenSection = 0f;
    
    public float GreenSectionCount => greenSectionCount;
    
    public float GreenPercentage => totalGameTime > 0 ? (timeInGreenSection / totalGameTime) * 100f : 0f;

    private float randomizeTimer = 0f;
    private float targetStartAngle;
    private float targetEndAngle;
    private float currentStartAngle;
    private float currentEndAngle;
    private bool isFirstRandomization = true;

    private float lastOuterRadius;
    private float lastInnerRadius;
    private float lastStartAngle;
    private float lastEndAngle;
    private int lastSegments;
    private bool wasInGreenSection = false;

    void Start()
    {
        if (needleScript == null)
            needleScript = FindFirstObjectByType<NeedleScript>();
            
        if (gameManager == null)
            gameManager = FindFirstObjectByType<FermentationGameManager>();
            
        if (enableRandomizer)
        {
            currentStartAngle = startAngle;
            currentEndAngle = endAngle;
            targetStartAngle = startAngle;
            targetEndAngle = endAngle;
            randomizeTimer = initialDelay;
        }
            
        CreateGreenArc();
        StoreLastValues();
    }

    void Update()
    {
        bool shouldUpdate = gameManager == null || gameManager.CanCountScore();
        
        if (shouldUpdate)
        {
            totalGameTime += Time.deltaTime;
        }
        
        if (enableRandomizer && shouldUpdate)
        {
            HandleRandomizer();
        }
        
        if (HasValuesChanged())
        {
            CreateGreenArc();
            StoreLastValues();
        }
        
        if (needleScript != null && shouldUpdate)
        {
            CheckNeedleInGreenSection();
        }
    }

    void CheckNeedleInGreenSection()
    {
        float needleAngle = needleScript.currentAngle;
        float currentStart = enableRandomizer ? currentStartAngle : startAngle;
        float currentEnd = enableRandomizer ? currentEndAngle : endAngle;
        bool inGreenSection = IsAngleInRange(needleAngle, currentStart, currentEnd);
        
        if (inGreenSection)
        {
            greenSectionCount += countIncrement * Time.deltaTime;
            timeInGreenSection += Time.deltaTime;
            
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
        
        float currentInterval = isFirstRandomization ? initialDelay : randomizeInterval;
        
        if (randomizeTimer >= currentInterval)
        {
            GenerateRandomGreenSection();
            randomizeTimer = 0f;
            isFirstRandomization = false;
        }
        
        if (smoothTransition)
        {
            currentStartAngle = Mathf.Lerp(currentStartAngle, targetStartAngle, transitionSpeed * Time.deltaTime);
            currentEndAngle = Mathf.Lerp(currentEndAngle, targetEndAngle, transitionSpeed * Time.deltaTime);
            
            startAngle = currentStartAngle;
            endAngle = currentEndAngle;
        }
        else
        {
            startAngle = targetStartAngle;
            endAngle = targetEndAngle;
            currentStartAngle = targetStartAngle;
            currentEndAngle = targetEndAngle;
        }
    }
    
    void GenerateRandomGreenSection()
    {
        float sectionSize = Random.Range(minSectionSize, maxSectionSize);
        
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
            targetStartAngle = minAngle;
            targetEndAngle = maxAngle;
            
            if (showDebugInfo)
                Debug.Log($"Green section too big for range, using full available: {targetStartAngle:F1}° to {targetEndAngle:F1}°");
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
        if (Application.isPlaying && lineRenderer != null)
        {
            CreateGreenArc();
        }
    }

    void CreateGreenArc()
    {
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        if (greenMaterial == null)
        {
            greenMaterial = new Material(Shader.Find("Sprites/Default"));
            greenMaterial.color = new Color(0f, 1f, 0f, 1f);
        }
        else
        {
            Color materialColor = greenMaterial.color;
            materialColor.a = 1f;
            greenMaterial.color = materialColor;
        }
        
        lineRenderer.material = greenMaterial; 
        lineRenderer.startWidth = outerRadius - innerRadius;
        lineRenderer.endWidth = outerRadius - innerRadius;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = false;
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float needleAngle = Mathf.Lerp(startAngle, endAngle, t);
            float visualAngle = (needleAngle - 85f) * Mathf.Deg2Rad;
            float radius = (outerRadius + innerRadius) / 2f;
            float x = Mathf.Cos(visualAngle) * radius;
            float y = Mathf.Sin(visualAngle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 2f));
        }
    }

    public bool IsNeedleInGreenSection(float needleAngle)
    {
        float currentStart = enableRandomizer ? currentStartAngle : startAngle;
        float currentEnd = enableRandomizer ? currentEndAngle : endAngle;
        return IsAngleInRange(needleAngle, currentStart, currentEnd);
    }

    public bool CheckIfNeedleInGreen(float needleAngle)
    {
        return IsAngleInRange(needleAngle, startAngle, endAngle);
    }

    public float GetGreenSectionCount()
    {
        return greenSectionCount;
    }

    public void ResetGreenSectionCount()
    {
        greenSectionCount = 0f;
    }
    
    public void ResetCounter()
    {
        greenSectionCount = 0f;
        totalGameTime = 0f;
        timeInGreenSection = 0f;
        
        isFirstRandomization = true;
        randomizeTimer = 0f;
    }
}