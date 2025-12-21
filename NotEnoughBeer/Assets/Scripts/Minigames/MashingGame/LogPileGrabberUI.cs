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
        
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        
        if (fireDropZone == null)
        {
            fireDropZone = FindObjectOfType<FireDropZone>();
        }
        
        if (logPileCycler == null)
        {
            logPileCycler = FindObjectOfType<LogPileCycler>();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging)
        {
            return;
        }
        
        if (currentLog != null)
        {
            Destroy(currentLog);
            currentLog = null;
            currentLogRect = null;
            currentLogCanvasGroup = null;
        }
        
        if (logPrefab != null && canvas != null)
        {
            currentLog = Instantiate(logPrefab, canvas.transform);
            currentLogRect = currentLog.GetComponent<RectTransform>();
            
            currentLogCanvasGroup = currentLog.GetComponent<CanvasGroup>();
            if (currentLogCanvasGroup == null)
            {
                currentLogCanvasGroup = currentLog.AddComponent<CanvasGroup>();
            }
            
            UpdateLogPosition(eventData);
            
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
        
        bool droppedOnFire = IsPointerOverFire(eventData);
        int logsBefore = logPileCycler?.GetCurrentLogCount() ?? 0;
        
        if (droppedOnFire && logPileCycler != null && logPileCycler.GetCurrentLogCount() < 5)
        {
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
        
        return RectTransformUtility.RectangleContainsScreenPoint(
            fireDropZone.GetComponent<RectTransform>(),
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera
        );
    }
}
