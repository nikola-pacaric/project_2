using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] private TMP_Text textArea;
    [SerializeField] private TMP_Text statusText;

    public void ShowLoading()
    {
        if (textArea != null) textArea.text = string.Empty;
        if (statusText != null) statusText.text = "Loading...";
    }

    public void ShowError(string message)
    {
        if (textArea != null) textArea.text = string.Empty;
        if (statusText != null) statusText.text = message;
    }

    public void Populate(List<LeaderboardRow> rows)
    {
        if (statusText != null) statusText.text = string.Empty;
        if (textArea == null) return;

        if (rows == null || rows.Count == 0)
        {
            textArea.text = "No scores yet.";
            return;
        }

        StringBuilder sb = new StringBuilder(rows.Count * 40);
        foreach (LeaderboardRow row in rows)
        {
            sb.Append(row.Rank.ToString().PadLeft(3));
            sb.Append(".  ");
            sb.Append(TruncateName(row.PlayerName, 18).PadRight(18));
            sb.Append("  ");
            sb.Append(row.Score.ToString().PadLeft(5));
            sb.Append("  ");
            sb.AppendLine(LeaderboardClient.FormatTime(row.TimePlayed));
        }
        textArea.text = sb.ToString();
    }

    private static string TruncateName(string name, int maxLen)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        return name.Length <= maxLen ? name : name.Substring(0, maxLen);
    }
}
