using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardView : MonoBehaviour
{
    [Header("Header (optional, shown only when rows are populated)")]
    [SerializeField] private GameObject headerGroup;

    [Header("Row list")]
    [SerializeField] private LeaderboardRowUI rowPrefab;
    [SerializeField] private RectTransform contentRoot;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;

    private readonly List<LeaderboardRowUI> spawnedRows = new List<LeaderboardRowUI>();

    public void ShowLoading()
    {
        ClearRows();
        SetHeaderVisible(false);
        SetStatus("Loading...");
    }

    public void ShowError(string message)
    {
        ClearRows();
        SetHeaderVisible(false);
        SetStatus(message);
    }

    public void Populate(List<LeaderboardRow> rows)
    {
        ClearRows();

        if (rows == null || rows.Count == 0)
        {
            SetHeaderVisible(false);
            SetStatus("No scores yet.");
            return;
        }

        if (rowPrefab == null || contentRoot == null)
        {
            Debug.LogWarning("[LeaderboardView] rowPrefab or contentRoot not assigned.");
            return;
        }

        SetHeaderVisible(true);
        SetStatus(string.Empty);

        foreach (LeaderboardRow row in rows)
        {
            LeaderboardRowUI rowUI = Instantiate(rowPrefab, contentRoot);
            rowUI.Bind(row);
            spawnedRows.Add(rowUI);
        }
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null) Destroy(spawnedRows[i].gameObject);
        }
        spawnedRows.Clear();
    }

    private void SetHeaderVisible(bool visible)
    {
        if (headerGroup != null) headerGroup.SetActive(visible);
    }

    private void SetStatus(string message)
    {
        if (statusText == null) return;
        statusText.text = message;
        statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }
}
