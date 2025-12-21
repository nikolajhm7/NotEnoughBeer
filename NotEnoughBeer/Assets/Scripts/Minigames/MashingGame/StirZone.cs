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
        if (spoon == null)
        {
            spoon = FindAnyObjectByType<SpoonStirrer>();
        }
        
        imageComponent = GetComponent<UnityEngine.UI.Image>();
        if (imageComponent != null)
        {
            originalColor = imageComponent.color;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (spoon != null)
        {
            spoon.StartStirring();
        }
        
        if (imageComponent != null)
        {
            imageComponent.color = highlightColor;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (spoon != null)
        {
            spoon.StopStirring();
        }
        
        if (imageComponent != null)
        {
            imageComponent.color = originalColor;
        }
    }
}
