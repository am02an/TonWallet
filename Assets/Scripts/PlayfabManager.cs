using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class PlayFabManager : MonoBehaviour
{
    [Header("PlayFab Settings")]
    public string titleId;
    public bool alwaysCreateNewLogin = true;

    public static PlayFabManager Instance { get; private set; }

    public string CurrentCustomId { get; private set; }
    public string CurrentPlayFabId { get; private set; }
    public bool IsLoggedIn { get; private set; }
    [Header("UI Free Leaderboard References")]
    public Transform leaderboardContainer;          // Parent where entries will be spawned
    public GameObject leaderboardEntryPrefab;       // Prefab for each row
    public Sprite defaultAvatar;                    // Fallback image if no avatar set
    public GameObject top1;
    public GameObject top2;
    public GameObject top3;
    private List<GameObject> spawnedEntries = new List<GameObject>();
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
        // Check if we already saved a CustomId
        if (PlayerPrefs.HasKey("CustomId"))
        {
            CurrentCustomId = PlayerPrefs.GetString("CustomId");
        }
        else
        {
            CurrentCustomId = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString("CustomId", CurrentCustomId);
        }

        var request = new LoginWithCustomIDRequest
        {
            TitleId = titleId,
            CustomId = CurrentCustomId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            Debug.Log("✅ Login Successful! ID: " + CurrentCustomId);

                IsLoggedIn = true;
            // Check if this is a NEW account
            if (result.NewlyCreated)
            {
                Debug.Log("🎉 New account created, setting display name...");
                UpdateDisplayName(username);
            }
            else
            {
                Debug.Log("🔑 Existing account, skipping display name update.");
            }

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
                _ =>
                {
                // ✅ Fetch DisplayName
                var accountInfoRequest = new GetAccountInfoRequest();
                    PlayFabClientAPI.GetAccountInfo(accountInfoRequest, accountResult =>
                    {
                        string displayName = accountResult.AccountInfo.TitleInfo.DisplayName ?? "NoName";
                        Debug.Log($"✅ {displayName} updated '{leaderboardName}' from {currentScore} → {newScore}");
                    },
                    OnPlayFabError);
                },
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

    public void OnLeaderboardReceived(List<PlayerLeaderboardEntry> leaderboard)
    {
        Debug.Log("Leaderboard received! Count: " + leaderboard.Count);
        ClearContainer(leaderboardContainer);
        // Clear old entries
        foreach (var entry in spawnedEntries)
            Destroy(entry);
       // spawnedEntries.Clear();

        // Create new entries in sequence
        foreach (var entry in leaderboard)
        {
            GameObject go;
            bool showRank = true;

            // Top 3 → use special slots
            if (entry.Position == 0 && top1 != null)
            {
                go = top1;
                showRank = false; // no rank text for top 3
            }
            else if (entry.Position == 1 && top2 != null)
            {
                go = top2;
                showRank = false;
            }
            else if (entry.Position == 2 && top3 != null)
            {
                go = top3;
                showRank = false;
            }
            else
            {
                // 4 onwards → instantiate prefab
                go = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            }

            // Get refs from prefab/slot
            var rank = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var name = go.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var score = go.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            // Rank logic
            if (showRank)
            {

                var avatar = go.transform.GetChild(3).GetComponent<Image>();
                rank.text = (entry.Position + 1).ToString();
            if (defaultAvatar != null)
                avatar.sprite = defaultAvatar;
            }
             // hide text for top 3

            // Assign values
            name.text = entry.DisplayName ?? "Guest";
            score.text = entry.StatValue.ToString();

        }


    }
    public void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

}
