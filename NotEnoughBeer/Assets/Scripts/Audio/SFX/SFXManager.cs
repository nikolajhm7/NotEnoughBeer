using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] private SFXLibrary library;
    [SerializeField] private AudioSource source;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Play(SFX sfx)
    {
        if (library == null || source == null)
            return;

        var entry = library.Get(sfx);

        if (entry.clip == null)
            return;

        Debug.Log($"[SFXManager] Playing SFX: {sfx}");

        source.PlayOneShot(entry.clip, entry.volume);
    }

    public void PlayAt(SFX sfx, Vector3 position)
    {
        var entry = library.Get(sfx);
        if (entry.clip == null) return;

        AudioSource.PlayClipAtPoint(entry.clip, position, entry.volume);
    }

    public void Stop()
    {
        if (source != null)
        {
            source.Stop();
        }
    }
}
