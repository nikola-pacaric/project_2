using UnityEngine;

[CreateAssetMenu(fileName = "LeaderboardConfig", menuName = "Platformer/Leaderboard Config")]
public class LeaderboardConfig : ScriptableObject
{
    [SerializeField] private string leaderboardId = "main_scores";
    [SerializeField] private int defaultFetchLimit = 50;

    public string LeaderboardId => leaderboardId;
    public int DefaultFetchLimit => defaultFetchLimit;
}
