using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsBackButton;

    [Header("Scenes")]
    [SerializeField] private string arc1SceneName = "Arc_1";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(CloseSettings);
        if (panel != null) panel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.escapeKey.wasPressedThisFrame) Toggle();
    }

    private void Toggle()
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }
        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        isPaused = true;
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
        RunTimer.Instance?.Pause();
    }

    private void Resume()
    {
        isPaused = false;
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
        RunTimer.Instance?.Resume();
    }

    private void OpenSettings()
    {
        if (panel != null) panel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (panel != null) panel.SetActive(true);
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
