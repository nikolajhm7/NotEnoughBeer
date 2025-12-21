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
    public Button DeleteSlot1;
    public Button DeleteSlot2;
    public Button DeleteSlot3;


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

        if (DeleteSlot1) DeleteSlot1.onClick.AddListener(() => DeleteSlot(1));
        if (DeleteSlot2) DeleteSlot2.onClick.AddListener(() => DeleteSlot(2));
        if (DeleteSlot3) DeleteSlot3.onClick.AddListener(() => DeleteSlot(3));

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
        bool exists = File.Exists(path);

        Button deleteButton = slot switch
        {
            1 => DeleteSlot1,
            2 => DeleteSlot2,
            3 => DeleteSlot3,
            _ => null
        };

        if (deleteButton)
        {
            deleteButton.interactable = exists;
        }

        if (!exists)
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

    public void DeleteSlot(int slot)
    {
        string path = SaveManager.GetSlotPath(slot);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[MainMenu] Deleted save slot {slot}");
        }

        UpdateSlotLabels();
    }

    [Serializable]
    private class CompactSaveData
    {
        public string SavedAtIsoUtc;
    }
}
