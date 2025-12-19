using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource SourceA;
    public AudioSource SourceB;

    [Header("Tracks")]
    public List<NamedClip> Tracks = new();

    [Header("Settings")]
    public bool PlayFirstClipOnStart = true;
    public float CrossfadeDuration = 1.5f;
    [Range(0f, 1f)]
    public float Volume = 1f;

    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private Coroutine crossfadeRoutine;
    private Dictionary<string, AudioClip> TrackDict;

    [System.Serializable]
    public class NamedClip
    {
        public string Name;
        public AudioClip Clip;
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            activeSource = SourceA;
            inactiveSource = SourceB;

            activeSource.volume = Volume;
            inactiveSource.volume = 0f;

            TrackDict = new();
            foreach (var entry in Tracks)
            {
                if (!string.IsNullOrEmpty(entry.Name) && entry.Clip != null)
                {
                    TrackDict[entry.Name] = entry.Clip;
                }
            }

            if (PlayFirstClipOnStart && Tracks.Count > 0)
            {
                Play(Tracks[0].Name);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Play a new music track immediately
    public void Play(string trackName, bool loop = true)
    {
        if (!TrackDict.TryGetValue(trackName, out var audioClip))
        {
            Debug.LogWarning($"MusicManager: Track '{trackName}' not found.");
            return;
        }

        activeSource.clip = audioClip;
        activeSource.loop = loop;
        activeSource.volume = Volume;
        activeSource.Play();

        inactiveSource.Stop();
        inactiveSource.clip = null;
        inactiveSource.volume = 0f;
    }

    // Crossfade to a new music track
    public void CrossfadeTo(string trackName, float? fadeTime = null, bool loop = true)
    {
        if (!TrackDict.TryGetValue(trackName, out var audioClip))
        {
            Debug.LogWarning($"MusicManager: Track '{trackName}' not found.");
            return;
        }

        inactiveSource.clip = audioClip;
        inactiveSource.loop = loop;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        if (crossfadeRoutine != null)
        {
            StopCoroutine(crossfadeRoutine);
        }

        crossfadeRoutine = StartCoroutine(CrossfadeCoroutine(fadeTime ?? CrossfadeDuration));
    }

    public void SetMusicVolume(float volume)
    {
        Volume = Mathf.Clamp01(volume);

        activeSource.volume = Volume;
    }

    public bool IsPlayingClip(AudioClip audioClip)
    {
        return 
            activeSource != null &&
            activeSource.clip == audioClip &&
            activeSource.isPlaying;
    }

    private IEnumerator CrossfadeCoroutine(float fadeDuration)
    {
        float t = 0f;

        float startVolumeActive = activeSource.volume;
        float targetVolumeInactive = Volume;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // unscaled for at pausning ikke påvirker fade
            float lerp = t / fadeDuration;

            activeSource.volume = Mathf.Lerp(startVolumeActive, 0f, lerp);
            inactiveSource.volume = Mathf.Lerp(0f, targetVolumeInactive, lerp);

            yield return null;
        }

        activeSource.volume = 0f;
        inactiveSource.volume = targetVolumeInactive;

        activeSource.Stop();
        activeSource.clip = null;

        // Swap sources
        (inactiveSource, activeSource) = (activeSource, inactiveSource);
        crossfadeRoutine = null;
    }
}
