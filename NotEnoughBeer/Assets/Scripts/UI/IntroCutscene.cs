using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class LineItem
{
    [SerializeField]
    public string Text;
    [SerializeField]
    public float Duration;
    [SerializeField]
    public Sprite Image;
}

public class IntroCutscene : MonoBehaviour
{
    public GameObject cutscenePanel;
    public TextMeshProUGUI cutsceneText;
    public Image cutsceneImage;

    [SerializeField]
    public LineItem[] lines;

    void Start()
    {
        if (!PlayerPrefs.HasKey("IntroCutscenePlayed"))
        {
            StartCoroutine(PlayCutscene());
        }
        else
        {
            cutscenePanel.SetActive(false);
        }
    }

    IEnumerator PlayCutscene()
    {
        cutscenePanel.SetActive(true);

        var volume = MusicManager.Instance.Volume;
        MusicManager.Instance.SetMusicVolume(0f);

        SFXManager.Instance.Play(SFX.IntroSpeech);

        foreach (LineItem line in lines)
        {
            cutsceneText.text = line.Text;

            if (cutsceneImage != null)
            {
                cutsceneImage.sprite = line.Image;
                cutsceneImage.enabled = line.Image != null;
            }


            float timer = 0f;
            while (timer < line.Duration)
            {
                if (Keyboard.current != null && 
                    (Keyboard.current.spaceKey.wasPressedThisFrame || 
                    Keyboard.current.enterKey.wasPressedThisFrame))
                {
                    goto EndCutscene;
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        EndCutscene:
        cutscenePanel.SetActive(false);
        PlayerPrefs.SetInt("IntroCutscenePlayed", 1);
        PlayerPrefs.Save();
        SFXManager.Instance.Stop();

        MusicManager.Instance.Play("Main");
        MusicManager.Instance.SetMusicVolume(volume);
    }
}
