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
        if (logPrefab != null && canvas != null)
        {
            currentLog = Instantiate(logPrefab, canvas.transform);
            currentLogRect = currentLog.GetComponent<RectTransform>();
            
            currentLogCanvasGroup = currentLog.GetComponent<CanvasGroup>();
            if (currentLogCanvasGroup == null)
            {
                currentLogCanvasGroup = currentLog.AddComponent<CanvasGroup>();
            }
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint
            );
            currentLogRect.anchoredPosition = localPoint;
            
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
        
        bool droppedOnFire = IsPointerOverFire(eventData);
        
        if (droppedOnFire && logPileCycler != null)
        {
            logPileCycler.AddOneLog();
            logPileCycler.ResetDecayTimer();
            Debug.Log("Log added to fire!");
        }
        else
        {
            Debug.Log("Missed the fire!");
        }
        
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
        
        return RectTransformUtility.RectangleContainsScreenPoint(
            fireDropZone.GetComponent<RectTransform>(),
            eventData.position,
            canvas.worldCamera
        );
    }
}
