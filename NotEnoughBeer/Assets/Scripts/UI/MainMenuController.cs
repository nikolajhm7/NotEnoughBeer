using System;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    public Button SaveSlot1;
    public Button SaveSlot2;
    public Button SaveSlot3;

    public TMP_Text SaveSlot1Label;
    public TMP_Text SaveSlot2Label;
    public TMP_Text SaveSlot3Label;

    [Header("Config")]
    public string gameSceneName = "Main";

    private void Start()
    {
        if (SaveSlot1) SaveSlot1.onClick.AddListener(() => SelectSlot(1));
        if (SaveSlot2) SaveSlot2.onClick.AddListener(() => SelectSlot(2));
        if (SaveSlot3) SaveSlot3.onClick.AddListener(() => SelectSlot(3));

        UpdateSlotLabels();
    }

    private void UpdateSlotLabels()
    {
        SetLabelFor(1, SaveSlot1Label);
        SetLabelFor(2, SaveSlot2Label);
        SetLabelFor(3, SaveSlot3Label);
    }

    private void SetLabelFor(int slot, TMP_Text label)
    {
        if (!label) return;

        string path = SaveManager.GetSlotPath(slot);

        if (!File.Exists(path))
        {
            label.text = $"Slot {slot}: <i>Empty</i>";
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<CompactSaveData>(json);
            string when = !string.IsNullOrEmpty(data.SavedAtIsoUtc)
                ? DateTime.Parse(data.SavedAtIsoUtc, null, DateTimeStyles.RoundtripKind).ToLocalTime().ToString("dd-MM-yyyy HH:mm")
                : File.GetLastWriteTime(path).ToString("dd-MM-yyyy HH:mm");

            label.text = $"Slot {slot}: {when}\n";
        }
        catch
        {
            label.text = $"Slot {slot}: <i>Unknown</i>";
        }
    }

    public void SelectSlot(int slot)
    {
        SaveManager.CurrentSlot = Mathf.Clamp(slot, 1, 3);
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();

        #if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

        #endif
    }

    [Serializable]
    private class CompactSaveData
    {
        public string SavedAtIsoUtc;
    }
}
