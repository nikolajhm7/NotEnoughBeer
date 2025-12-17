using UnityEngine;

[CreateAssetMenu(menuName = "SFX Library", fileName = "SFX Library")]
public class SFXLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public SFX sfx;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        public Vector2 pitchRange;
    }

    public Entry[] entries;

    public Entry Get(SFX sfx)
    {
        foreach (var entry in entries)
        {
            if (entry.sfx == sfx)
                return entry;
        }

        Debug.LogWarning($"[SFXLibrary] Missing SFX: {sfx}");
        return default;
    }
}
