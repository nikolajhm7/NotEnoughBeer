using System.Collections.Generic;
using UnityEngine;

public class NotificationService : MonoBehaviour
{
    public static NotificationService Instance { get; private set; }

    [SerializeField] private NotificationText notificationPrefab;
    [SerializeField] private Canvas uiCanvas;

    struct Pending
    {
        public string msg;
        public bool hasColor;
        public Color color;
    }

    static readonly Queue<Pending> _queue = new Queue<Pending>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        
        FlushQueue();
    }

    public static void Enqueue(string message, Color? color = null)
    {
        _queue.Enqueue(new Pending
        {
            msg = message,
            hasColor = color.HasValue,
            color = color.GetValueOrDefault()
        });

        
        if (Instance != null)
            Instance.FlushQueue();
    }

    public void Show(string message, Color? color = null)
    {
       
        if (notificationPrefab == null || uiCanvas == null)
        {
            Enqueue(message, color);
            return;
        }

        var notification = Instantiate(notificationPrefab, uiCanvas.transform);
        if (color.HasValue) notification.Play(message, color.Value);
        else notification.Play(message);
    }

    void FlushQueue()
    {
        if (notificationPrefab == null || uiCanvas == null) return;

        while (_queue.Count > 0)
        {
            var p = _queue.Dequeue();
            Show(p.msg, p.hasColor ? p.color : (Color?)null);
        }
    }
}
