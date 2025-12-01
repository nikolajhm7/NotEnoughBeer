using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameBridge : MonoBehaviour
{
    public static MinigameBridge Instance;

    private Action<float> _onComplete;
    [SerializeField] private string mainSceneName = "Main"; // your garage scene name

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartMinigame(string sceneName, Action<float> onComplete)
    {
        _onComplete = onComplete;
        SceneManager.LoadScene(sceneName);
    }

    public void FinishMinigame(float score)
    {
        _onComplete?.Invoke(score);
        _onComplete = null;
        SceneManager.LoadScene(mainSceneName);
    }
}
