using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int savedCurrentSegment;
    private int savedMaxHearts;
    private string targetSpawnPointId;
    private bool hasSavedState;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void SavePlayerState(PlayerHealth player, string spawnPointId)
    {
        savedCurrentSegment = player.currentSegment;
        savedMaxHearts = player.maxHearts;
        targetSpawnPointId = spawnPointId;
        hasSavedState = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasSavedState) return;
        StartCoroutine(ApplyAfterOneFrame());
    }

    private IEnumerator ApplyAfterOneFrame()
    {
        yield return null;

        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        if (player != null)
        {
            SpawnPoint spawn = FindSpawnPoint(targetSpawnPointId);
            if (spawn != null)
            {
                player.transform.position = spawn.transform.position;
                player.respawnPoint = spawn.transform.position;
                player.startingPossPoint = spawn.transform.position;
            }

            player.RestoreState(savedMaxHearts, savedCurrentSegment);
        }

        hasSavedState = false;

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeIn();
        }
    }

    private SpawnPoint FindSpawnPoint(string id)
    {
        SpawnPoint[] all = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        if (all.Length == 0) return null;

        foreach (SpawnPoint sp in all)
        {
            if (sp.Id == id) return sp;
        }
        return null;
    }
}
