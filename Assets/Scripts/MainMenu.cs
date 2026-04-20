using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button settingsButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button leaderboardBackButton;

    [Header("Scenes")]
    [SerializeField] private string arc1SceneName = "Arc_1";

    private void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (leaderboardButton != null) leaderboardButton.onClick.AddListener(OpenLeaderboard);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(ShowMainPanel);
        if (leaderboardBackButton != null) leaderboardBackButton.onClick.AddListener(ShowMainPanel);

        ShowMainPanel();
        Time.timeScale = 1f;
    }

    private void Play()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResetRun();
        else if (RunTimer.Instance != null) RunTimer.Instance.StartRun();
        SceneManager.LoadScene(arc1SceneName);
    }

    private void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void OpenLeaderboard()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
    }

    private void ShowMainPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }
}
