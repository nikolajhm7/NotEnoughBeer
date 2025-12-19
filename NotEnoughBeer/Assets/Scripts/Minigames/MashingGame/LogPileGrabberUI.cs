using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LogPileGrabberUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Log Settings")]
    [Tooltip("Prefab of the log that appears when grabbing (should be a UI Image)")]
    public GameObject logPrefab;
    
    [Tooltip("Canvas for the dragged log")]
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
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
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
        // Don't allow new drag if already dragging
        if (isDragging)
        {
            return;
        }
        
        // Clean up any existing log first
        if (currentLog != null)
        {
            Destroy(currentLog);
            currentLog = null;
            currentLogRect = null;
            currentLogCanvasGroup = null;
        }
        
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
            UpdateLogPosition(eventData);
            
            // Set visual properties
            currentLog.transform.localScale = Vector3.one * logScale;
            currentLogCanvasGroup.alpha = logAlpha;
            currentLogCanvasGroup.blocksRaycasts = false;
            
            isDragging = true;
            Debug.Log($"Grabbed a log from the pile! Fire currently has {logPileCycler?.GetCurrentLogCount() ?? 0} logs");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && currentLog != null && currentLogRect != null)
        {
            UpdateLogPosition(eventData);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging || currentLog == null)
        {
            isDragging = false;
            return;
        }
        
        // Check if dropped over the fire
        bool droppedOnFire = IsPointerOverFire(eventData);
        int logsBefore = logPileCycler?.GetCurrentLogCount() ?? 0;
        
        if (droppedOnFire && logPileCycler != null && logPileCycler.GetCurrentLogCount() < 5)
        {
            // Add log to fire (only adds one)
            logPileCycler.AddOneLog();
            logPileCycler.ResetDecayTimer();
            int logsAfter = logPileCycler.GetCurrentLogCount();
            Debug.Log($"Log added to fire! {logsBefore} -> {logsAfter} logs");
        }
        else if (droppedOnFire && logPileCycler != null)
        {
            Debug.Log($"Fire is full! (5 logs maximum) Current: {logsBefore}");
        }
        else
        {
            Debug.Log($"Missed the fire! Fire has {logsBefore} logs");
        }
        
        // Clean up
        isDragging = false;
        if (currentLog != null)
        {
            Destroy(currentLog);
        }
        currentLog = null;
        currentLogRect = null;
        currentLogCanvasGroup = null;
    }
    
    private void UpdateLogPosition(PointerEventData eventData)
    {
        if (currentLog != null && currentLogRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out localPoint
            );
            currentLogRect.anchoredPosition = localPoint;
        }
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
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera
        );
    }
}
