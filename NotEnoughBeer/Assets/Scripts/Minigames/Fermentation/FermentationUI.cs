using TMPro;
using UnityEngine;

public class FermentationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI greenCounterText;
    [SerializeField] private TextMeshProUGUI redCounterText;
    [SerializeField] private TextMeshProUGUI greenPercentageText;
    
    [Header("Script References")]
    [SerializeField] private GreenSectionScript greenSectionScript;
    [SerializeField] private RingScript ringScript;
    
    [Header("Display Settings")]
    [SerializeField] private string greenPrefix = "Green: ";
    [SerializeField] private string redPrefix = "Red: ";
    [SerializeField] private string percentagePrefix = "Green %: ";
    [SerializeField] private int decimalPlaces = 1;
    
    public static FermentationUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Auto-find scripts if not assigned
        if (greenSectionScript == null)
            greenSectionScript = FindFirstObjectByType<GreenSectionScript>();
            
        if (ringScript == null)
            ringScript = FindFirstObjectByType<RingScript>();
    }

    private void Update()
    {
        UpdateCounterDisplays();
    }

    private void UpdateCounterDisplays()
    {
        // Update green counter display
        if (greenCounterText != null && greenSectionScript != null)
        {
            float greenCount = greenSectionScript.GreenSectionCount;
            greenCounterText.text = $"{greenPrefix}{greenCount.ToString($"F{decimalPlaces}")}";
        }
        
        // Update red counter display
        if (redCounterText != null && ringScript != null)
        {
            float redCount = ringScript.RedRingCount;
            redCounterText.text = $"{redPrefix}{redCount.ToString($"F{decimalPlaces}")}";
        }
        
        // Update green percentage display
        if (greenPercentageText != null && greenSectionScript != null)
        {
            float greenPercentage = greenSectionScript.GreenPercentage;
            greenPercentageText.text = $"{percentagePrefix}{greenPercentage.ToString($"F{decimalPlaces}")}%";
        }
    }

    /// <summary>
    /// Manually update the displays (useful for when values change infrequently)
    /// </summary>
    public void RefreshDisplays()
    {
        UpdateCounterDisplays();
    }

    /// <summary>
    /// Reset all counters to zero (call this when starting a new game)
    /// </summary>
    public void ResetCounters()
    {
        if (greenSectionScript != null)
            greenSectionScript.ResetCounter();
            
        if (ringScript != null)
            ringScript.ResetCounter();
            
        UpdateCounterDisplays();
    }
}