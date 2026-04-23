using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string arc1SceneName = "Arc_1";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Leaderboard")]
    [SerializeField] private LeaderboardConfig leaderboardConfig;

    [Header("Name Input")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text submitStatusText;

    private int pendingScore;
    private double pendingTime;
    private bool scoreSubmitted;

    private void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (submitButton != null) submitButton.onClick.AddListener(OnUpdateNameClicked);
    }

    public void Show(int finalScore)
    {
        pendingScore = finalScore;
        scoreSubmitted = false;

        if (finalScoreText != null) finalScoreText.text = "Score: " + finalScore;

        RunTimer.Instance?.Pause();
        pendingTime = RunTimer.Instance != null ? RunTimer.Instance.ElapsedSeconds : 0d;

        if (finalTimeText != null && RunTimer.Instance != null)
            finalTimeText.text = "Time: " + RunTimer.Instance.GetFormatted(true);

        if (nameInputField != null)
        {
            string saved = PlayerPrefs.GetString("leaderboard_player_name", string.Empty);
            nameInputField.text = string.IsNullOrEmpty(saved) ? string.Empty : saved;
            nameInputField.onSubmit.AddListener(_ => OnUpdateNameClicked());
        }

        if (submitStatusText != null) submitStatusText.text = string.Empty;
        if (submitButton != null) submitButton.interactable = false;

        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;

        if (leaderboardConfig != null)
            _ = AutoSubmitScoreAsync();
    }

    private async Task AutoSubmitScoreAsync()
    {
        if (submitStatusText != null) submitStatusText.text = "Saving score...";

        try
        {
            await LeaderboardClient.EnsureInitializedAsync(leaderboardConfig);
            await LeaderboardClient.SubmitScoreAsync(pendingScore, pendingTime);
            scoreSubmitted = true;

            if (submitStatusText != null) submitStatusText.text = "Score saved! Enter your name:";
            if (submitButton != null) submitButton.interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameOver] Auto-submit failed: {e.Message}");
            if (submitStatusText != null) submitStatusText.text = "Score save failed.";
        }
    }

    private async void OnUpdateNameClicked()
    {
        if (!scoreSubmitted || leaderboardConfig == null) return;

        string raw = nameInputField != null ? nameInputField.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(raw)) return;

        if (submitButton != null) submitButton.interactable = false;
        if (submitStatusText != null) submitStatusText.text = "Updating name...";

        try
        {
            await LeaderboardClient.SubmitNameAsync(raw);
            if (submitStatusText != null) submitStatusText.text = "Name updated!";
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameOver] Name update failed: {e.Message}");
            if (submitStatusText != null) submitStatusText.text = "Name update failed.";
            if (submitButton != null) submitButton.interactable = true;
        }
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.ResetRun();
        SceneManager.LoadScene(arc1SceneName);
    }

    private void OnMainMenuClicked()
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
