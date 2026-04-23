using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text textArea;
    [SerializeField] private TMP_Text statusText;

    // Adjust in Inspector to match your font size (em units force monospace per character)
    [SerializeField] private float charWidthEm = 0.62f;

    private const string HEADER_FORMAT = " #   {0,-16}  {1,5}   {2}";
    private const int NAME_WIDTH = 16;

    public void ShowLoading()
    {
        SetHeaderVisible(false);
        if (textArea != null) textArea.text = string.Empty;
        if (statusText != null) statusText.text = "Loading...";
    }

    public void ShowError(string message)
    {
        SetHeaderVisible(false);
        if (textArea != null) textArea.text = string.Empty;
        if (statusText != null) statusText.text = message;
    }

    public void Populate(List<LeaderboardRow> rows)
    {
        if (statusText != null) statusText.text = string.Empty;
        if (textArea == null) return;

        if (rows == null || rows.Count == 0)
        {
            SetHeaderVisible(false);
            textArea.text = "No scores yet.";
            return;
        }

        SetHeaderVisible(true);

        string mOpen = $"<mspace={charWidthEm}em>";
        const string mClose = "</mspace>";

        StringBuilder sb = new StringBuilder(rows.Count * 50);
        sb.Append(mOpen);
        foreach (LeaderboardRow row in rows)
        {
            sb.Append(row.Rank.ToString().PadLeft(2));
            sb.Append(".  ");
            sb.Append(TruncateName(row.PlayerName, NAME_WIDTH).PadRight(NAME_WIDTH));
            sb.Append("  ");
            sb.Append(row.Score.ToString().PadLeft(6));
            sb.Append("  ");
            sb.AppendLine(LeaderboardClient.FormatTime(row.TimePlayed));
        }
        sb.Append(mClose);
        textArea.text = sb.ToString();
    }

    private void SetHeaderVisible(bool visible)
    {
        if (headerText == null) return;
        headerText.gameObject.SetActive(visible);
        if (!visible) return;

        string mOpen = $"<mspace={charWidthEm}em>";
        const string mClose = "</mspace>";
        headerText.text = mOpen
            + string.Format(HEADER_FORMAT, "NAME", "SCORE", "TIME")
            + mClose;
    }

    private static string TruncateName(string name, int maxLen)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        return name.Length <= maxLen ? name : name.Substring(0, maxLen);
    }
}
