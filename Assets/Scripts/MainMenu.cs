using System;
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

    [Header("Leaderboard")]
    [SerializeField] private LeaderboardConfig leaderboardConfig;
    [SerializeField] private LeaderboardView leaderboardView;

    private async void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (leaderboardButton != null) leaderboardButton.onClick.AddListener(OpenLeaderboard);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(ShowMainPanel);
        if (leaderboardBackButton != null) leaderboardBackButton.onClick.AddListener(ShowMainPanel);

        ShowMainPanel();
        Time.timeScale = 1f;
        AudioManager.Instance?.StopMusic();

        if (leaderboardConfig != null)
        {
            try
            {
                await LeaderboardClient.EnsureInitializedAsync(leaderboardConfig);
                Debug.Log("[MainMenu] Leaderboard init OK");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MainMenu] Leaderboard init failed: {e}");
            }
        }
    }

    private async void Play()
    {
        if (playButton != null) playButton.interactable = false;

        if (leaderboardConfig != null)
        {
            try
            {
                await LeaderboardClient.EnsureInitializedAsync(leaderboardConfig);
                await LeaderboardClient.StartNewRunIdentityAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MainMenu] New run identity failed (proceeding anyway): {e.Message}");
            }
        }

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

    private async void OpenLeaderboard()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);

        if (leaderboardView == null || leaderboardConfig == null) return;

        leaderboardView.ShowLoading();
        try
        {
            await LeaderboardClient.EnsureInitializedAsync(leaderboardConfig);
            var rows = await LeaderboardClient.FetchTopNAsync();
            leaderboardView.Populate(rows);
        }
        catch (Exception e)
        {
            leaderboardView.ShowError("Could not load leaderboard.");
            Debug.LogWarning($"[MainMenu] FetchTopN failed: {e}");
        }
    }

    private void ShowMainPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }
}
