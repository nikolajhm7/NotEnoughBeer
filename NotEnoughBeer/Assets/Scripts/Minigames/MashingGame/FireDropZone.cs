using UnityEngine;
using UnityEngine.EventSystems;

public class FireDropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [Tooltip("The LogPileCycler to add logs to when dropped")]
    public LogPileCycler logPileCycler;
    
    [Header("Settings")]
    [Tooltip("Maximum logs that can be added (optional limit)")]
    public int maxLogs = 5;
    
    [Header("Visual Feedback")]
    [Tooltip("Highlight color when log is hovering")]
    public Color highlightColor = new Color(1f, 1f, 0.5f, 0.3f);
    
    private UnityEngine.UI.Image imageComponent;
    private Color originalColor;
    private bool isHighlighted = false;
    
    void Start()
    {
        if (logPileCycler == null)
        {
            logPileCycler = FindObjectOfType<LogPileCycler>();
        }
        
        imageComponent = GetComponent<UnityEngine.UI.Image>();
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowHighlight();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        RemoveHighlight();
    }
    
    public bool CanAcceptLog()
    {
        if (logPileCycler == null)
        {
            return false;
        }
        
        return logPileCycler.GetCurrentLogCount() < maxLogs;
    }
    
    private void ShowHighlight()
    {
        if (imageComponent != null && !isHighlighted)
        {
            imageComponent.color = highlightColor;
            isHighlighted = true;
        }
    }
    
    private void RemoveHighlight()
    {
        if (imageComponent != null && isHighlighted)
        {
            imageComponent.color = originalColor;
            isHighlighted = false;
        }
    }
}
     