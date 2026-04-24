using TMPro;
using UnityEngine;

public class LeaderboardRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;

    public void Bind(LeaderboardRow row)
    {
        if (rankText != null) rankText.text = row.Rank.ToString();
        if (nameText != null) nameText.text = row.PlayerName;
        if (scoreText != null) scoreText.text = row.Score.ToString();
        if (timeText != null) timeText.text = LeaderboardClient.FormatTime(row.TimePlayed);
    }
}
