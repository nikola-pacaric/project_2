using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scenes")]
    [SerializeField] private string arc1SceneName = "Arc_1";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.escapeKey.wasPressedThisFrame) Toggle();
    }

    private void Toggle()
    {
        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        isPaused = true;
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
        RunTimer.Instance?.Pause();
    }

    private void Resume()
    {
        isPaused = false;
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
        RunTimer.Instance?.Resume();
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.ResetRun();
        SceneManager.LoadScene(arc1SceneName);
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        RunTimer.Instance?.Stop();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScore();
            GameManager.Instance.ClearSavedState();
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }

}
