using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    void OnEnable()
    {
        GameManager.OnScoreChanged += UpdateScore;
    }

    void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScore;
    }

    void Start()
    {
        UpdateScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
