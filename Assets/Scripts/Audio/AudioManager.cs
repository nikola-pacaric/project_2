using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Library")]
    [SerializeField] private SoundLibrary library;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SfxVolume";
    [SerializeField] private string uiVolumeParam = "UiVolume";

    [Header("SFX Pool")]
    [SerializeField] private int sfxVoiceCount = 8;

    private AudioSource musicSource;
    private readonly List<AudioSource> sfxPool = new();

    private const float MIN_VOLUME_DB = -80f;
    private const float SILENCE_THRESHOLD = 0.0001f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildMusicSource();
        BuildSfxPool();
    }

    public void PlaySFX(SfxId id)
    {
        if (library == null)
        {
            Debug.LogWarning("[AudioManager] SoundLibrary is not assigned.");
            return;
        }
        if (!library.TryGetSfx(id, out AudioClip clip, out float volume))
        {
            Debug.LogWarning($"[AudioManager] No clip assigned for SfxId.{id}");
            return;
        }

        AudioSource source = GetFreeSfxSource();
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    public void PlaySFXAt(SfxId id, Vector3 worldPosition)
    {
        if (!IsOnScreen(worldPosition)) return;
        PlaySFX(id);
    }

    private bool IsOnScreen(Vector3 worldPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 vp = cam.WorldToViewportPoint(worldPosition);
        return vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
    }

    public void PlayMusic(MusicId id)
    {
        if (library == null)
        {
            Debug.LogWarning("[AudioManager] SoundLibrary is not assigned.");
            return;
        }
        if (!library.TryGetMusic(id, out AudioClip clip, out float volume))
        {
            Debug.LogWarning($"[AudioManager] No clip assigned for MusicId.{id}");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetVolume(AudioChannel channel, float normalized)
    {
        if (mixer == null)
        {
            Debug.LogWarning("[AudioManager] AudioMixer is not assigned.");
            return;
        }

        string param = channel switch
        {
            AudioChannel.Music => musicVolumeParam,
            AudioChannel.Sfx => sfxVolumeParam,
            AudioChannel.Ui => uiVolumeParam,
            _ => null
        };
        if (string.IsNullOrEmpty(param)) return;

        float clamped = Mathf.Clamp01(normalized);
        float db = clamped <= SILENCE_THRESHOLD ? MIN_VOLUME_DB : Mathf.Log10(clamped) * 20f;
        mixer.SetFloat(param, db);
    }

    private void BuildMusicSource()
    {
        GameObject go = new GameObject("MusicSource");
        go.transform.SetParent(transform);
        musicSource = go.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        if (musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;
    }

    private void BuildSfxPool()
    {
        for (int i = 0; i < sfxVoiceCount; i++)
        {
            GameObject go = new GameObject($"SfxSource_{i}");
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            if (sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;
            sfxPool.Add(source);
        }
    }

    private AudioSource GetFreeSfxSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying) return source;
        }
        return sfxPool[0];
    }
}
