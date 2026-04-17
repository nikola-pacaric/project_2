using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private string arc1SceneName = "Arc_1";

    private void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
    }

    public void Show(int finalScore)
    {
        if (finalScoreText != null) finalScoreText.text = "Score: " + finalScore.ToString();
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScore();
            GameManager.Instance.ClearSavedState();
        }
        SceneManager.LoadScene(arc1SceneName);
    }
}
