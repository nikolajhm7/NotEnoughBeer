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
        if (logPileCycler == null)
        {
            logPileCycler = FindObjectOfType<LogPileCycler>();
        }
        
        if (thermometer == null)
        {
            thermometer = FindObjectOfType<Thermometer>();
        }
        
        if (logPileCycler != null)
        {
            UpdateFiresBasedOnLogs(logPileCycler.GetCurrentLogCount());
        }
    }
    
    void Update()
    {
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
    
    public void UpdateFiresBasedOnLogs(int logCount)
    {
        currentLogCount = logCount;
        
        if (fireObjects == null || fireObjects.Length == 0)
        {
            Debug.LogWarning("No fire objects assigned to FireManager!");
            return;
        }
        
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
        
        for (int i = 0; i < fireObjects.Length; i++)
        {
            if (fireObjects[i] != null)
            {
                fireObjects[i].SetActive(i < activeFireCount);
            }
        }
        
        if (thermometer != null)
        {
            thermometer.OnLogCountChanged(logCount);
        }
    }
    
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
    
    public void EnableAllFires()
    {
        SetActiveFireCount(fireObjects.Length);
    }
    
    public void DisableAllFires()
    {
        SetActiveFireCount(0);
    }
    
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
