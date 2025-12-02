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

        // 🔹 Save current world state before leaving
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

        // When the main scene is reloaded, call OnReturnSceneLoaded
        SceneManager.sceneLoaded += OnReturnSceneLoaded;
        SceneManager.LoadScene(_returnSceneName);
    }

    void OnReturnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Make sure we only run once
        SceneManager.sceneLoaded -= OnReturnSceneLoaded;

        // 🔹 Reload saved world state
        var saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            saveManager.Load();
        }
        else
        {
            Debug.LogWarning("[MinigameBridge] No SaveManager found after returning from minigame.");
        }

        // 🔹 Give the score back to the machine / batch
        _onComplete?.Invoke(_pendingScore);
        _onComplete = null;
    }
}
