using UnityEngine;
using UnityEngine.UI;

public class Thermometer : MonoBehaviour
{
    [Header("Temperature Settings")]
    [Tooltip("Current temperature (0-100 degrees)")]
    [Range(0f, 100f)]
    public float currentTemperature = 0f;
    
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
    
    [Header("Current State")]
    [Tooltip("Current number of logs affecting temperature")]
    public int currentLogCount = 0;
    
    [Tooltip("Current temperature change rate per second")]
    public float currentChangeRate = 0f;
    
    void Update()
    {
        // Update temperature based on current change rate
        if (currentChangeRate != 0f)
        {
            currentTemperature += currentChangeRate * Time.deltaTime;
            currentTemperature = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);
            
            UpdateUI();
        }
    }
    
    /// <summary>
    /// Called when the log count changes to update the temperature change rate
    /// </summary>
    public void OnLogCountChanged(int logCount)
    {
        currentLogCount = logCount;
        
        // Determine the temperature change rate based on log count
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
    
    /// <summary>
    /// Updates all UI elements to display current temperature
    /// </summary>
    private void UpdateUI()
    {
        // Update slider
        if (temperatureSlider != null)
        {
            temperatureSlider.value = currentTemperature / maxTemperature;
        }
        
        // Update text
        if (temperatureText != null)
        {
            temperatureText.text = $"{Mathf.RoundToInt(currentTemperature)}°";
        }
        
        // Update fill image
        if (temperatureFill != null)
        {
            temperatureFill.fillAmount = currentTemperature / maxTemperature;
        }
    }
    
    /// <summary>
    /// Manually set the temperature
    /// </summary>
    public void SetTemperature(float temperature)
    {
        currentTemperature = Mathf.Clamp(temperature, minTemperature, maxTemperature);
        UpdateUI();
    }
    
    /// <summary>
    /// Increase temperature by a specific amount
    /// </summary>
    public void IncreaseTemperature(float amount)
    {
        currentTemperature = Mathf.Clamp(currentTemperature + amount, minTemperature, maxTemperature);
        UpdateUI();
    }
    
    /// <summary>
    /// Decrease temperature by a specific amount
    /// </summary>
    public void DecreaseTemperature(float amount)
    {
        currentTemperature = Mathf.Clamp(currentTemperature - amount, minTemperature, maxTemperature);
        UpdateUI();
    }
    
    /// <summary>
    /// Check if temperature is at maximum
    /// </summary>
    public bool IsAtMaxTemperature()
    {
        return currentTemperature >= maxTemperature;
    }
    
    /// <summary>
    /// Check if temperature is at minimum
    /// </summary>
    public bool IsAtMinTemperature()
    {
        return currentTemperature <= minTemperature;
    }
    
    /// <summary>
    /// Get temperature as a percentage (0-1)
    /// </summary>
    public float GetTemperaturePercentage()
    {
        return currentTemperature / maxTemperature;
    }
    
    /// <summary>
    /// Reset temperature to a specific value
    /// </summary>
    public void ResetTemperature(float temperature = 0f)
    {
        SetTemperature(temperature);
    }
}
