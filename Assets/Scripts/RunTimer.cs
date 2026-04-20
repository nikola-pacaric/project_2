using UnityEngine;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance { get; private set; }

    public double ElapsedSeconds { get; private set; }
    public bool IsRunning { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("RunTimer");
        go.AddComponent<RunTimer>();
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
    }

    private void Update()
    {
        if (!IsRunning) return;
        ElapsedSeconds += Time.unscaledDeltaTime;
    }

    public void StartRun()
    {
        ElapsedSeconds = 0d;
        IsRunning = true;
    }

    public void Pause() => IsRunning = false;

    public void Resume() => IsRunning = true;

    public void Stop()
    {
        IsRunning = false;
        ElapsedSeconds = 0d;
    }

    public string GetFormatted(bool withMilliseconds = false)
    {
        int totalSeconds = (int)ElapsedSeconds;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (!withMilliseconds) return $"{minutes:00}:{seconds:00}";

        int milliseconds = (int)((ElapsedSeconds - totalSeconds) * 1000d);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
}
