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

    private void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show(int finalScore)
    {
        if (finalScoreText != null) finalScoreText.text = "Score: " + finalScore.ToString();

        RunTimer.Instance?.Pause();
        if (finalTimeText != null && RunTimer.Instance != null)
        {
            finalTimeText.text = "Time: " + RunTimer.Instance.GetFormatted(true);
        }

        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
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
