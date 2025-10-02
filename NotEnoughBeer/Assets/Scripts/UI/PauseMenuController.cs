using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    public GameObject MenuRoot;
    public SaveManager SaveManager;

    [Header("Settings")]
    public string MainMenuSceneName = "Main Menu";
    public bool LockCursorWhenPlaying = true;

    bool isOpen;

    private void Start()
    {
        SetOpen(false, force: true);        
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        SetOpen(!isOpen);
    }

    public void OnClickResume()
    {
        SetOpen(false);
    }

    public void OnClickQuitToMainMenu()
    {
        if (SaveManager != null) SaveManager.Save();
                
        Time.timeScale = 1f;
        SetCursorLocked(false);
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void SetOpen(bool open, bool force = false)
    {
        if (isOpen == open && !force) return;

        isOpen = open;

        if (MenuRoot) MenuRoot.SetActive(isOpen);

        if (open)
        {
            Time.timeScale = 0f;
            SetCursorLocked(false);
        }
        else
        {
            Time.timeScale = 1f;
            SetCursorLocked(LockCursorWhenPlaying);
        }
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }
}