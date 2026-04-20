using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [SerializeField] private MusicId track = MusicId.Gameplay;
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        if (!playOnStart) return;
        AudioManager.Instance?.PlayMusic(track);
    }
}
