using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float moveUpDistance = 40f;
    [SerializeField] private float fadeDuration = 0.5f;

    RectTransform rect;
    CanvasGroup canvasGroup;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Play(string message)
    {
        text.text = message;
        StartCoroutine(Animate());
    }

    public void Play(string message, Color color)
    {
        text.text = message;
        text.color = color;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * moveUpDistance;

        float t = 0f;
        while (t < lifetime)
        {
            float progress = t / lifetime;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);

            if (t > lifetime - fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (t - (lifetime - fadeDuration)) / fadeDuration);
            }

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
