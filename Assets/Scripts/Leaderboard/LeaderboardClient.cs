using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public static class LeaderboardClient
{
    private const string DEFAULT_DISPLAY_NAME = "Player";

    private static LeaderboardConfig config;
    private static Task initializeTask;
    private static bool initialized;

    public static bool IsInitialized => initialized;

    public static string PlayerName =>
        initialized && !string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName)
            ? AuthenticationService.Instance.PlayerName
            : DEFAULT_DISPLAY_NAME;

    public static Task EnsureInitializedAsync(LeaderboardConfig cfg)
    {
        if (cfg == null) return Task.FromException(new ArgumentNullException(nameof(cfg)));
        if (config == null) config = cfg;
        if (initialized) return Task.CompletedTask;
        if (initializeTask != null) return initializeTask;

        initializeTask = InitializeInternalAsync();
        return initializeTask;
    }

    private static async Task InitializeInternalAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
        Debug.Log($"[Leaderboard] Services state: {UnityServices.State}");

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Debug.Log($"[Leaderboard] Signed in. PlayerId: {AuthenticationService.Instance.PlayerId}");

        try
        {
            var probe = LeaderboardsService.Instance;
            Debug.Log($"[Leaderboard] LeaderboardsService.Instance probe OK: {probe.GetType().FullName}");
        }
        catch (Exception)
        {
            Debug.LogWarning("[Leaderboard] LeaderboardsService.Instance null after sign-in; recovering from CoreRegistry.");
            RecoverLeaderboardsInstance();
            var probe = LeaderboardsService.Instance;
            Debug.Log($"[Leaderboard] Recovered LeaderboardsService.Instance: {probe.GetType().FullName}");
        }

        initialized = true;
    }

    private static void RecoverLeaderboardsInstance()
    {
        Type coreRegistryType = Type.GetType("Unity.Services.Core.Internal.CoreRegistry, Unity.Services.Core.Internal");
        if (coreRegistryType == null)
        {
            throw new InvalidOperationException("CoreRegistry type not found.");
        }

        PropertyInfo registryInstanceProp = coreRegistryType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        object registry = registryInstanceProp?.GetValue(null);
        if (registry == null)
        {
            throw new InvalidOperationException("CoreRegistry.Instance is null.");
        }

        MethodInfo getServiceOpen = coreRegistryType.GetMethod("GetService", BindingFlags.Public | BindingFlags.Instance);
        MethodInfo getServiceClosed = getServiceOpen?.MakeGenericMethod(typeof(ILeaderboardsService));
        var service = getServiceClosed?.Invoke(registry, null) as ILeaderboardsService;
        if (service == null)
        {
            throw new InvalidOperationException("ILeaderboardsService not found in CoreRegistry.");
        }

        PropertyInfo instanceProp = typeof(LeaderboardsService).GetProperty(
            nameof(LeaderboardsService.Instance),
            BindingFlags.Public | BindingFlags.Static);
        if (instanceProp == null)
        {
            throw new InvalidOperationException("LeaderboardsService.Instance property not found.");
        }

        instanceProp.SetValue(null, service);
    }

    public static async Task SubmitScoreAsync(int score, double timePlayed)
    {
        EnsureReady();

        if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
        {
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(DEFAULT_DISPLAY_NAME);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] Failed to set default player name: {e.Message}");
            }
        }

        var metadata = new Dictionary<string, object>
        {
            { "timePlayed", Math.Round(timePlayed, 2) }
        };
        var options = new AddPlayerScoreOptions { Metadata = metadata };
        await LeaderboardsService.Instance.AddPlayerScoreAsync(config.LeaderboardId, score, options);
    }

    public static async Task SubmitNameAsync(string name)
    {
        EnsureReady();
        if (string.IsNullOrWhiteSpace(name)) return;
        await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
    }

    public static async Task StartNewRunIdentityAsync()
    {
        if (config == null) throw new InvalidOperationException("LeaderboardClient config missing. Call EnsureInitializedAsync first.");

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut(clearCredentials: true);
        }
        AuthenticationService.Instance.ClearSessionToken();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log($"[Leaderboard] New run identity. PlayerId: {AuthenticationService.Instance.PlayerId}");

        initialized = true;
    }

    public static async Task<List<LeaderboardRow>> FetchTopNAsync(int? limit = null)
    {
        EnsureReady();
        int fetchLimit = limit ?? config.DefaultFetchLimit;
        var options = new GetScoresOptions { Limit = fetchLimit, IncludeMetadata = true };
        LeaderboardScoresPage page = await LeaderboardsService.Instance.GetScoresAsync(config.LeaderboardId, options);

        var rows = new List<LeaderboardRow>(page.Results.Count);
        foreach (LeaderboardEntry entry in page.Results)
        {
            rows.Add(new LeaderboardRow
            {
                Rank = entry.Rank + 1,
                PlayerName = string.IsNullOrEmpty(entry.PlayerName) ? DEFAULT_DISPLAY_NAME : StripAuthSuffix(entry.PlayerName),
                Score = (int)entry.Score,
                TimePlayed = ParseTimePlayed(entry.Metadata)
            });
        }
        return rows;
    }

    public static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        int m = total / 60;
        int s = total % 60;
        return $"{m:00}:{s:00}";
    }

    private static void EnsureReady()
    {
        if (!initialized) throw new InvalidOperationException("LeaderboardClient not initialized. Call EnsureInitializedAsync first.");
        if (config == null) throw new InvalidOperationException("LeaderboardClient config missing.");
    }

    private static float ParseTimePlayed(string metadata)
    {
        if (string.IsNullOrEmpty(metadata)) return 0f;
        try
        {
            var parsed = JsonUtility.FromJson<TimeMetadata>(metadata);
            return parsed != null ? parsed.timePlayed : 0f;
        }
        catch
        {
            return 0f;
        }
    }

    private static string StripAuthSuffix(string rawName)
    {
        int hashIdx = rawName.LastIndexOf('#');
        return hashIdx > 0 ? rawName.Substring(0, hashIdx) : rawName;
    }

    [Serializable]
    private class TimeMetadata
    {
        public float timePlayed;
    }
}
