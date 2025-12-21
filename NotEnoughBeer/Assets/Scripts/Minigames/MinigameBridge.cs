using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameBridge : MonoBehaviour
{
    public static MinigameBridge Instance;

    string _returnSceneName;
    float _pendingScore;
    Action<float> _onComplete;

    void Awake()
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
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[MinigameBridge] No minigame scene name given.");
            return;
        }

        _onComplete = onComplete;
        _returnSceneName = SceneManager.GetActiveScene().name;

        
        var saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            saveManager.Save();
        }
        else
        {
            Debug.LogWarning("[MinigameBridge] No SaveManager found before starting minigame.");
        }

        SceneManager.LoadScene(sceneName);
    }

    public void FinishMinigame(float score)
    {
        if (string.IsNullOrEmpty(_returnSceneName))
        {
            Debug.LogWarning("[MinigameBridge] No return scene stored. Staying in current scene.");
            return;
        }

        _pendingScore = score;

        
        SceneManager.sceneLoaded += OnReturnSceneLoaded;
        SceneManager.LoadScene(_returnSceneName);
    }

    void OnReturnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        SceneManager.sceneLoaded -= OnReturnSceneLoaded;

        
        var saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            saveManager.Load();
        }
        else
        {
            Debug.LogWarning("[MinigameBridge] No SaveManager found after returning from minigame.");
        }

        
        _onComplete?.Invoke(_pendingScore);
        _onComplete = null;
    }
}
