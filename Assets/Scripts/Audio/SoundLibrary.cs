using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    private struct SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Serializable]
    private struct MusicEntry
    {
        public MusicId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Header("SFX")]
    [SerializeField] private List<SfxEntry> sfxEntries = new();

    [Header("Music")]
    [SerializeField] private List<MusicEntry> musicEntries = new();

    private Dictionary<SfxId, SfxEntry> sfxMap;
    private Dictionary<MusicId, MusicEntry> musicMap;

    private void OnEnable()
    {
        BuildMaps();
    }

    public bool TryGetSfx(SfxId id, out AudioClip clip, out float volume)
    {
        if (sfxMap == null) BuildMaps();
        if (sfxMap.TryGetValue(id, out SfxEntry entry) && entry.clip != null)
        {
            clip = entry.clip;
            volume = entry.volume <= 0f ? 1f : entry.volume;
            return true;
        }
        clip = null;
        volume = 0f;
        return false;
    }

    public bool TryGetMusic(MusicId id, out AudioClip clip, out float volume)
    {
        if (musicMap == null) BuildMaps();
        if (musicMap.TryGetValue(id, out MusicEntry entry) && entry.clip != null)
        {
            clip = entry.clip;
            volume = entry.volume <= 0f ? 1f : entry.volume;
            return true;
        }
        clip = null;
        volume = 0f;
        return false;
    }

    private void BuildMaps()
    {
        sfxMap = new Dictionary<SfxId, SfxEntry>(sfxEntries.Count);
        foreach (SfxEntry e in sfxEntries)
        {
            if (!sfxMap.ContainsKey(e.id)) sfxMap.Add(e.id, e);
        }

        musicMap = new Dictionary<MusicId, MusicEntry>(musicEntries.Count);
        foreach (MusicEntry e in musicEntries)
        {
            if (!musicMap.ContainsKey(e.id)) musicMap.Add(e.id, e);
        }
    }
}
