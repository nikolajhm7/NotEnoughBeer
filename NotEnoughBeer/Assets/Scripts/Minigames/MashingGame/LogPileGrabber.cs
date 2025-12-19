using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LogPileGrabber : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Log Settings")]
    [Tooltip("Prefab of the log that appears when grabbing")]
    public GameObject logPrefab;
    
    [Tooltip("Canvas for proper positioning")]
    public Canvas canvas;
    
    [Header("Visual Settings")]
    [Tooltip("Scale of the grabbed log")]
    public float logScale = 1f;
    
    [Tooltip("Alpha of the grabbed log")]
    [Range(0f, 1f)]
    public float logAlpha = 0.8f;
    
    [Header("References")]
    [Tooltip("The fire drop zone")]
    public FireDropZone fireDropZone;
    
    [Tooltip("The LogPileCycler to add logs to")]
    public LogPileCycler logPileCycler;
    
    private GameObject currentLog;
    private RectTransform currentLogRect;
    private CanvasGroup currentLogCanvasGroup;
    private bool isDragging = false;
    
    void Start()
    {
        // Try to find canvas if not assigned
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        
        // Try to find fire drop zone
        if (fireDropZone == null)
        {
            fireDropZone = FindObjectOfType<FireDropZone>();
        }
        
        // Try to find log pile cycler
        if (logPileCycler == null)
        {
            logPileCycler = FindObjectOfType<LogPileCycler>();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Create a log when clicking on the pile
        if (logPrefab != null && canvas != null)
        {
            // Instantiate the log
            currentLog = Instantiate(logPrefab, canvas.transform);
            currentLogRect = currentLog.GetComponent<RectTransform>();
            
            // Add canvas group for alpha control
            currentLogCanvasGroup = currentLog.GetComponent<CanvasGroup>();
            if (currentLogCanvasGroup == null)
            {
                currentLogCanvasGroup = currentLog.AddComponent<CanvasGroup>();
            }
            
            // Set position to pointer
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint
            );
            currentLogRect.anchoredPosition = localPoint;
            
            // Set visual properties
            currentLog.transform.localScale = Vector3.one * logScale;
            currentLogCanvasGroup.alpha = logAlpha;
            currentLogCanvasGroup.blocksRaycasts = false;
            
            isDragging = true;
            Debug.Log("Grabbed a log from the pile!");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && currentLog != null && currentLogRect != null)
        {
            // Move log with pointer
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint
            );
            currentLogRect.anchoredPosition = localPoint;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging || currentLog == null)
        {
            return;
        }
        
        isDragging = false;
        
        // Check if dropped over the fire
        bool droppedOnFire = IsPointerOverFire(eventData);
        
        if (droppedOnFire && logPileCycler != null)
        {
            // Add log to fire
            logPileCycler.AddOneLog();
            logPileCycler.ResetDecayTimer();
            Debug.Log("Log added to fire!");
        }
        else
        {
            Debug.Log("Missed the fire!");
        }
        
        // Destroy the dragged log
        Destroy(currentLog);
        currentLog = null;
        currentLogRect = null;
        currentLogCanvasGroup = null;
    }
    
    private bool IsPointerOverFire(PointerEventData eventData)
    {
        if (fireDropZone == null)
        {
            return false;
        }
        
        // Check if pointer is over the fire drop zone
        return RectTransformUtility.RectangleContainsScreenPoint(
            fireDropZone.GetComponent<RectTransform>(),
            eventData.position,
            canvas.worldCamera
        );
    }
}
