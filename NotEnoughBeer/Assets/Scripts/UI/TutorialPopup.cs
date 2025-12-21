using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
    public static TutorialPopup Instance { get; private set; }

    public GameObject popupPanel;
    public float showTime = 3f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Hide()
    {
        popupPanel.SetActive(false);
    }

    public void TryShowTutorial()
    {
        if (!SaveManager.Instance.TutorialShown)
        {
            popupPanel.SetActive(true);
            SaveManager.Instance.MarkTutorialAsShown();
            Invoke(nameof(Hide), showTime);
        }
        else
        {
            popupPanel.SetActive(false);
        }
    }
}
