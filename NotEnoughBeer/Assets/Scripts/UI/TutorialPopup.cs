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
        if (!PlayerPrefs.HasKey("TutorialShown"))
        {
            popupPanel.SetActive(true);
            PlayerPrefs.SetInt("TutorialShown", 1);
            Invoke(nameof(Hide), showTime);
        }
        else
        {
            popupPanel.SetActive(false);
        }
    }
}
