using UnityEngine;

public class FireManager : MonoBehaviour
{
    [Header("Fire Objects")]
    [Tooltip("Array of all fire GameObjects. Enable/disable based on log count")]
    public GameObject[] fireObjects;
    
    [Header("Fire Rules")]
    [Tooltip("Minimum logs required for first fire")]
    public int logsForFirstFire = 1;
    
    [Tooltip("Minimum logs required for second fire")]
    public int logsForSecondFire = 3;
    
    [Tooltip("Minimum logs required for third fire")]
    public int logsForThirdFire = 5;
    
    [Header("Debug Info")]
    [Tooltip("Shows current log count for debugging")]
    public int currentLogCount = 0;
    
    [Header("Thermometer Integration")]
    [Tooltip("Optional: Reference to Thermometer to notify of log changes")]
    public Thermometer thermometer;
    
    [Header("References")]
    [Tooltip("Optional: Reference to LogPileCycler to auto-update fires")]
    public LogPileCycler logPileCycler;
    
    private int lastLogCount = -1;
    
    void Start()
    {
        // Try to find LogPileCycler if not assigned
        if (logPileCycler == null)
        {
            logPileCycler = FindObjectOfType<LogPileCycler>();
        }
        
        // Try to find Thermometer if not assigned
        if (thermometer == null)
        {
            thermometer = FindObjectOfType<Thermometer>();
        }
        
        // Initial update
        if (logPileCycler != null)
        {
            UpdateFiresBasedOnLogs(logPileCycler.GetCurrentLogCount());
        }
    }
    
    void Update()
    {
        // Auto-update fires when log count changes
        if (logPileCycler != null)
        {
            int currentLogCount = logPileCycler.GetCurrentLogCount();
            if (currentLogCount != lastLogCount)
            {
                UpdateFiresBasedOnLogs(currentLogCount);
                lastLogCount = currentLogCount;
            }
        }
    }
    
    /// <summary>
    /// Enable/disable fire objects based on the number of logs
    /// </summary>
    public void UpdateFiresBasedOnLogs(int logCount)
    {
        currentLogCount = logCount; // Store for debugging
        
        if (fireObjects == null || fireObjects.Length == 0)
        {
            Debug.LogWarning("No fire objects assigned to FireManager!");
            return;
        }
        
        // Determine how many fires should be active
        int activeFireCount = 0;
        
        if (logCount >= logsForThirdFire && fireObjects.Length >= 3)
        {
            activeFireCount = 3;
            Debug.Log($"Activating 3 fires (log count: {logCount})");
        }
        else if (logCount >= logsForSecondFire && fireObjects.Length >= 2)
        {
            activeFireCount = 2;
            Debug.Log($"Activating 2 fires (log count: {logCount})");
        }
        else if (logCount >= logsForFirstFire)
        {
            activeFireCount = 1;
            Debug.Log($"Activating 1 fire (log count: {logCount})");
        }
        else
        {
            Debug.Log($"No fires active (log count: {logCount})");
        }
        
        // Enable/disable fires
        for (int i = 0; i < fireObjects.Length; i++)
        {
            if (fireObjects[i] != null)
            {
                fireObjects[i].SetActive(i < activeFireCount);
            }
        }
        
        // Notify thermometer of log count change
        if (thermometer != null)
        {
            thermometer.OnLogCountChanged(logCount);
        }
    }
    
    /// <summary>
    /// Manually set the number of active fires
    /// </summary>
    public void SetActiveFireCount(int count)
    {
        if (fireObjects == null || fireObjects.Length == 0)
        {
            return;
        }
        
        count = Mathf.Clamp(count, 0, fireObjects.Length);
        
        for (int i = 0; i < fireObjects.Length; i++)
        {
            if (fireObjects[i] != null)
            {
                fireObjects[i].SetActive(i < count);
            }
        }
    }
    
    /// <summary>
    /// Enable all fires
    /// </summary>
    public void EnableAllFires()
    {
        SetActiveFireCount(fireObjects.Length);
    }
    
    /// <summary>
    /// Disable all fires
    /// </summary>
    public void DisableAllFires()
    {
        SetActiveFireCount(0);
    }
    
    /// <summary>
    /// Get the number of currently active fires
    /// </summary>
    public int GetActiveFireCount()
    {
        if (fireObjects == null)
        {
            return 0;
        }
        
        int count = 0;
        foreach (GameObject fire in fireObjects)
        {
            if (fire != null && fire.activeSelf)
            {
                count++;
            }
        }
        return count;
    }
}
