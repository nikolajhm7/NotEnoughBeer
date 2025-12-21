using UnityEngine;
using UnityEngine.UI;

public class Thermometer : MonoBehaviour
{
    [Header("Temperature Settings")]
    [Tooltip("Current temperature (0-100 degrees)")]
    [Range(0f, 100f)]
    public float currentTemperature = 60f;
    
    [Tooltip("Maximum temperature")]
    public float maxTemperature = 100f;
    
    [Tooltip("Minimum temperature")]
    public float minTemperature = 0f;
    
    [Header("Temperature Change Rates (per second)")]
    [Tooltip("Temperature decay rate with 0 logs")]
    public float decayRate0Logs = -10f;
    
    [Tooltip("Temperature decay rate with 1 log")]
    public float decayRate1Log = -5f;
    
    [Tooltip("Temperature decay rate with 2 logs")]
    public float decayRate2Logs = -2f;
    
    [Tooltip("Temperature change rate with 3 logs (stable = 0)")]
    public float changeRate3Logs = 0f;
    
    [Tooltip("Temperature increase rate with 4 logs")]
    public float increaseRate4Logs = 3f;
    
    [Tooltip("Temperature increase rate with 5 logs")]
    public float increaseRate5Logs = 6f;
    
    [Header("UI Elements")]
    [Tooltip("Slider to display temperature (0-100)")]
    public Slider temperatureSlider;
    
    [Tooltip("Text to display current temperature value")]
    public TMPro.TextMeshProUGUI temperatureText;
    
    [Tooltip("Image fill to display temperature (alternative to slider)")]
    public Image temperatureFill;
    
    [Header("Temperature Range Indicators")]
    [Tooltip("Indicator at 60 degrees (min optimal temperature)")]
    public RectTransform indicator60Degrees;
    
    [Tooltip("Indicator at 75 degrees (max optimal temperature)")]
    public RectTransform indicator75Degrees;
    
    [Header("Current State")]
    [Tooltip("Current number of logs affecting temperature")]
    public int currentLogCount = 0;
    
    [Tooltip("Current temperature change rate per second")]
    public float currentChangeRate = 0f;
    
    void Start()
    {
        PositionIndicators();
    }
    
    void Update()
    {
        if (currentChangeRate != 0f)
        {
            currentTemperature += currentChangeRate * Time.deltaTime;
            currentTemperature = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);
            
            UpdateUI();
        }
    }
    

    public void OnLogCountChanged(int logCount)
    {
        currentLogCount = logCount;
        
        switch (logCount)
        {
            case 0:
                currentChangeRate = decayRate0Logs;
                Debug.Log($"Thermometer: 0 logs - decaying at {currentChangeRate}°/s");
                break;
            case 1:
                currentChangeRate = decayRate1Log;
                Debug.Log($"Thermometer: 1 log - decaying at {currentChangeRate}°/s");
                break;
            case 2:
                currentChangeRate = decayRate2Logs;
                Debug.Log($"Thermometer: 2 logs - decaying at {currentChangeRate}°/s");
                break;
            case 3:
                currentChangeRate = changeRate3Logs;
                Debug.Log($"Thermometer: 3 logs - stable at {currentChangeRate}°/s");
                break;
            case 4:
                currentChangeRate = increaseRate4Logs;
                Debug.Log($"Thermometer: 4 logs - increasing at {currentChangeRate}°/s");
                break;
            case 5:
                currentChangeRate = increaseRate5Logs;
                Debug.Log($"Thermometer: 5 logs - increasing rapidly at {currentChangeRate}°/s");
                break;
            default:
                currentChangeRate = 0f;
                Debug.Log($"Thermometer: {logCount} logs - no change");
                break;
        }
    }
    
    private void UpdateUI()
    {
        if (temperatureSlider != null)
        {
            temperatureSlider.value = currentTemperature / maxTemperature;
        }
        
        if (temperatureText != null)
        {
            temperatureText.text = $"{Mathf.RoundToInt(currentTemperature)}°";
        }
        
        if (temperatureFill != null)
        {
            temperatureFill.fillAmount = currentTemperature / maxTemperature;
        }
    }
    
    public void SetTemperature(float temperature)
    {
        currentTemperature = Mathf.Clamp(temperature, minTemperature, maxTemperature);
        UpdateUI();
    }
    
    public void IncreaseTemperature(float amount)
    {
        currentTemperature = Mathf.Clamp(currentTemperature + amount, minTemperature, maxTemperature);
        UpdateUI();
    }
    
    public void DecreaseTemperature(float amount)
    {
        currentTemperature = Mathf.Clamp(currentTemperature - amount, minTemperature, maxTemperature);
        UpdateUI();
    }

    public bool IsAtMaxTemperature()
    {
        return currentTemperature >= maxTemperature;
    }
    
    public bool IsAtMinTemperature()
    {
        return currentTemperature <= minTemperature;
    }
    

    public float GetTemperaturePercentage()
    {
        return currentTemperature / maxTemperature;
    }
    

    public void ResetTemperature(float temperature = 0f)
    {
        SetTemperature(temperature);
    }
    

    private void PositionIndicators()
    {
        if (temperatureSlider == null) return;
        
        RectTransform fillRect = temperatureSlider.fillRect;
        if (fillRect == null)
        {
            Debug.LogWarning("Thermometer: Slider doesn't have a Fill Rect assigned!");
            return;
        }
        
        if (indicator60Degrees != null)
        {
            float position60 = 60f / maxTemperature;
            PositionIndicatorOnSlider(indicator60Degrees, position60, fillRect);
        }
        
        if (indicator75Degrees != null)
        {
            float position75 = 75f / maxTemperature;
            PositionIndicatorOnSlider(indicator75Degrees, position75, fillRect);
        }
    }
    

    private void PositionIndicatorOnSlider(RectTransform indicator, float normalizedPosition, RectTransform fillRect)
    {
        indicator.anchorMin = new Vector2(0.5f, 0);
        indicator.anchorMax = new Vector2(0.5f, 0);
        indicator.pivot = new Vector2(0.5f, 0.5f);
        
        RectTransform parent = fillRect.parent as RectTransform;
        if (parent != null)
        {
            float fillAreaHeight = parent.rect.height;
            float yPosition = fillAreaHeight * normalizedPosition;
            
            indicator.anchoredPosition = new Vector2(0, yPosition);
        }
    }
}
