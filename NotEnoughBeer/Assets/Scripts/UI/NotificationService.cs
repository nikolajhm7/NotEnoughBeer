using UnityEngine;

public class NotificationService : MonoBehaviour
{
    public static NotificationService Instance { get; private set; }

    [SerializeField] private NotificationText notificationPrefab;
    [SerializeField] private Canvas uiCanvas;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(string message, Color? color = null)
    {
        var notification = Instantiate(notificationPrefab, uiCanvas.transform);

        if (color.HasValue)
            notification.Play(message, color.Value);
        else
            notification.Play(message);
    }
}
