using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
    public GameObject popupPanel;
    public float showTime = 5f;

    void Start()
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

    void Hide()
    {
        popupPanel.SetActive(false);
    }
}
