using UnityEngine;
using UnityEngine.EventSystems;

public class StirZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [Tooltip("The spoon that will stir")]
    public SpoonStirrer spoon;
    
    [Header("Visual Feedback")]
    [Tooltip("Highlight color when hovering")]
    public Color highlightColor = new Color(1f, 1f, 1f, 0.1f);
    
    private UnityEngine.UI.Image imageComponent;
    private Color originalColor;
    
    void Start()
    {
        // Try to find spoon if not assigned
        if (spoon == null)
        {
            spoon = FindAnyObjectByType<SpoonStirrer>();
        }
        
        // Get image component for visual feedback
        imageComponent = GetComponent<UnityEngine.UI.Image>();
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Start stirring when clicked
        if (spoon != null)
        {
            spoon.StartStirring();
        }
        
        // Visual feedback
        if (imageComponent != null)
        {
            imageComponent.color = highlightColor;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // Stop stirring when released
        if (spoon != null)
        {
            spoon.StopStirring();
        }
        
        // Remove visual feedback
        if (imageComponent != null)
        {
            imageComponent.color = originalColor;
        }
    }
}
