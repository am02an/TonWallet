using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManager : MonoBehaviour
{
    [Header("PlayFab Settings")]
    public string titleId;
    public bool alwaysCreateNewLogin = true;

    public static PlayFabManager Instance { get; private set; }

    public string CurrentCustomId { get; private set; }
    public string CurrentPlayFabId { get; private set; }
    public bool IsLoggedIn { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (string.IsNullOrEmpty(titleId))
        {
            Debug.LogError("Title ID is empty.");
            return;
        }

        PlayFabSettings.staticSettings.TitleId = titleId;
        //LoginNewAccount();
    }

    public void LoginNewAccount(string username)
    {
        // Generate unique ID for this player (used for login)
        CurrentCustomId = Guid.NewGuid().ToString("N");

        var request = new LoginWithCustomIDRequest
        {
            TitleId = titleId,
            CustomId = CurrentCustomId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            Debug.Log("Login Successful!");
            CreateAllLeaderboards();
            // After login, set the username
            UpdateDisplayName(username);

        }, OnPlayFabError);
    }

    private void UpdateDisplayName(string username)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = username
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Debug.Log("✅ Username set to: " + result.DisplayName);
            },
            error =>
            {
                if (error.Error == PlayFabErrorCode.NameNotAvailable)
                {
                // Add random number to make it unique
                string newName = username + "_" + UnityEngine.Random.Range(1000, 9999);
        IsLoggedIn = true;
                    Debug.LogWarning($"⚠️ Name not available. Trying new name: {newName}");
                    UpdateDisplayName(newName); // retry
            }
                else
                {
                    OnPlayFabError(error);
                }
            });
    }

    private void OnLoginSuccess(LoginResult result)
    {
        CurrentPlayFabId = result.PlayFabId;
        Debug.Log($"Logged in. PlayFabId: {CurrentPlayFabId}");
    }

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    /// <summary>
    /// Set score for an existing leaderboard by name.
    /// </summary>
    public void SubmitScore(string leaderboardName, int scoreToAdd)
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("SubmitScore called before login complete.");
            return;
        }

        // Step 1: Get current leaderboard score
        var getStatsRequest = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { leaderboardName }
        };

        PlayFabClientAPI.GetPlayerStatistics(getStatsRequest, result =>
        {
            int currentScore = 0;

            // If player already has a score, store it
            var stat = result.Statistics.Find(s => s.StatisticName == leaderboardName);
            if (stat != null)
                currentScore = stat.Value;

            int newScore = currentScore + scoreToAdd;

            // Step 2: Update leaderboard with new total
            var updateRequest = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = leaderboardName,
                    Value = newScore
                }
            }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(updateRequest,
                _ => Debug.Log($"Updated '{leaderboardName}' from {currentScore} to {newScore}"),
                OnPlayFabError);

        }, OnPlayFabError);
    }



    /// <summary>
    /// Fetch top N entries from a leaderboard.
    /// </summary>
    /// public 
    public void CreateAllLeaderboards()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "Leaderboard_Free", Value = 0 },
                new StatisticUpdate { StatisticName = "Leaderboard_Easy", Value = 0 },
                new StatisticUpdate { StatisticName = "Leaderboard_Medium", Value = 0 },
                new StatisticUpdate { StatisticName = "Leaderboard_Hard", Value = 0 },
                new StatisticUpdate { StatisticName = "Leaderboard_Compete", Value = 0 }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => Debug.Log("✅ All leaderboards created (Free, Easy, Medium, Hard)"),
            error => Debug.LogError("❌ Error creating leaderboards: " + error.GenerateErrorReport()));
    }
    public void GetLeaderboardShow(string leaderBoardName)
    {
        // Pass callback to handle leaderboard results
        GetTopLeaderboard(leaderBoardName, 10, OnLeaderboardReceived);
    }

    private void GetTopLeaderboard(string leaderboardName, int maxResults, Action<List<PlayerLeaderboardEntry>> onDone)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = leaderboardName,
            StartPosition = 0,
            MaxResultsCount = Mathf.Clamp(maxResults, 1, 100)
        };

        PlayFabClientAPI.GetLeaderboard(request,
            r => onDone?.Invoke(r.Leaderboard),
            OnPlayFabError);
    }

    private void OnLeaderboardReceived(List<PlayerLeaderboardEntry> leaderboard)
    {
        Debug.Log("Leaderboard received! Count: " + leaderboard.Count);
        foreach (var entry in leaderboard)
        {
            Debug.Log($"{entry.Position + 1}. {entry.DisplayName} - {entry.StatValue}");
        }
    }
}
