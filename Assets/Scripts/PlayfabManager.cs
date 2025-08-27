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
    public Transform leaderboardContainerCompete;          // Parent where entries will be spawned
    public GameObject leaderboardEntryPrefab;       // Prefab for each row
    public Sprite defaultAvatar;                    // Fallback image if no avatar set
    [Header("Top Slots - Free")]
    public GameObject top1Free;
    public GameObject top2Free;
    public GameObject top3Free;

    [Header("Top Slots - Compete")]
    public GameObject top1Compete;
    public GameObject top2Compete;
    public GameObject top3Compete;
    private List<GameObject> spawnedEntries = new List<GameObject>();

    [Header(" Leaderboard UI refs")]
    
    public LeaderboardUI[] leaderboards;   // Drag your 3 leaderboards here in Inspector
    private Dictionary<string, LeaderboardUI> leaderboardMap;

    private TimeSpan resetCooldown = TimeSpan.FromDays(1);
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        leaderboardMap = new Dictionary<string, LeaderboardUI>();
        foreach (var lb in leaderboards)
        {
            if (!leaderboardMap.ContainsKey(lb.statName))
                leaderboardMap.Add(lb.statName, lb);
        }
    }
   

    void Update()
    {
        UpdateTimers();
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
                SavePlayerData();
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

    private void GetTopLeaderboard(string leaderboardName, int maxResults, Action<string, List<PlayerLeaderboardEntry>> onDone)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = leaderboardName,
            StartPosition = 0,
            MaxResultsCount = Mathf.Clamp(maxResults, 1, 100)
        };

        PlayFabClientAPI.GetLeaderboard(request,
            r => onDone?.Invoke(leaderboardName, r.Leaderboard),
            OnPlayFabError);
    }
    public void OnLeaderboardReceived(string leaderboardName, List<PlayerLeaderboardEntry> leaderboard)
    {
        Debug.Log($"Leaderboard received: {leaderboardName} Count: {leaderboard.Count}");

        // Choose the correct container & top slots
        Transform targetContainer;
        GameObject top1Ref = null, top2Ref = null, top3Ref = null;

        if (leaderboardName == "Leaderboard_Free")
        {
            Debug.Log("Free leaderboard");
            targetContainer = leaderboardContainer;
            top1Ref = top1Free;
            top2Ref = top2Free;
            top3Ref = top3Free;
        }
        else
        {
            Debug.Log("Compete leaderboard");
            targetContainer = leaderboardContainerCompete;
            top1Ref = top1Compete;
            top2Ref = top2Compete;
            top3Ref = top3Compete;
        }

        // Clear container
        ClearContainer(targetContainer);

        // ✅ If no entries, add placeholder
        if (leaderboard.Count == 0)
        {
            GameObject go1 = top1Ref;
            GameObject go2 = top2Ref;
            GameObject go3 = top3Ref;

            // --- Top 1 ---
            var name1 = go1.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var score1 = go1.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            var avatar1 = go1.transform.GetChild(3).GetComponent<Image>();

            name1.text = "-------";
            score1.text = "0";
          //  if (defaultAvatar != null) avatar1.sprite = defaultAvatar;

            // --- Top 2 ---
            var name2 = go2.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var score2 = go2.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            var avatar2 = go2.transform.GetChild(3).GetComponent<Image>();

            name2.text = "-------";
            score2.text = "0";
           // if (defaultAvatar != null) avatar2.sprite = defaultAvatar;

            // --- Top 3 ---
            var name3 = go3.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var score3 = go3.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            var avatar3 = go3.transform.GetChild(3).GetComponent<Image>();

            name3.text = "-------";
            score3.text = "0";
           // if (defaultAvatar != null) avatar3.sprite = defaultAvatar;

            Debug.Log($"[{leaderboardName}] Empty → Showing placeholder rows for Top 3.");
            return; // stop here
        }


        // ✅ Otherwise, build normally
        foreach (var entry in leaderboard)
        {
            GameObject go;
            bool showRank = true;

            // Top 3 → use the correct slots
            if (entry.Position == 0 && top1Ref != null)
            {
                go = top1Ref;
                showRank = false;
            }
            else if (entry.Position == 1 && top2Ref != null)
            {
                go = top2Ref;
                showRank = false;
            }
            else if (entry.Position == 2 && top3Ref != null)
            {
                go = top3Ref;
                showRank = false;
            }
            else
            {
                // 4 onwards → instantiate prefab into the right container
                go = Instantiate(leaderboardEntryPrefab, targetContainer);
            }

            // Get refs
            var rank = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var name = go.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var score = go.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            if (showRank)
            {
                var avatar = go.transform.GetChild(3).GetComponent<Image>();
                rank.text = (entry.Position + 1).ToString();

                if (defaultAvatar != null)
                    avatar.sprite = defaultAvatar;

                Debug.Log($"[{leaderboardName}] Rank: {rank.text}, Player: {entry.DisplayName}, Score: {entry.StatValue}");
            }
            else
            {
                Debug.Log($"[{leaderboardName}] Top {entry.Position + 1} Slot → Player: {entry.DisplayName}, Score: {entry.StatValue}");
            }

            string displayName = entry.DisplayName ?? "Guest";

            // If the name has more than 6 characters, cut it and add "..."
            if (displayName.Length > 6)
            {
                displayName = displayName.Substring(0, 6) + "...";
            }

            name.text = displayName;

            score.text = entry.StatValue.ToString();
        }
    }


    public void FetchLeaderboard(string statName)
    {
        if (!leaderboardMap.ContainsKey(statName))
        {
            Debug.LogError("No UI reference found for leaderboard: " + statName);
            return;
        }

        var lb = leaderboardMap[statName];

        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statName,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request,
            result =>
            {
                if (result.Leaderboard.Count > 0)
                {
                    var entry = result.Leaderboard[0];
                    lb.rankText.text = "#" + (entry.Position + 1);
                    lb.scoreText.text = entry.StatValue.ToString();
                }
                else
                {
                    lb.rankText.text = "-";
                    lb.scoreText.text = "0";
                }
            },
            error => Debug.LogError("Leaderboard error: " + error.GenerateErrorReport()));
    }
    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error getting leaderboard: " + error.GenerateErrorReport());
    }
    // ✅ Updates countdown for all leaderboards
    private void UpdateTimers()
    {
        DateTime now = DateTime.UtcNow;
        DateTime nextReset = DateTime.UtcNow.Date.AddDays(1); // midnight reset
        TimeSpan timeLeft = nextReset - now;

        foreach (var lb in leaderboardMap.Values)
        {
            lb.timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
        }
    }
    public void SavePlayerData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "PlayerGameData", SaveManager.Insatnce.GetPlayerdata() }  // saving the whole JSON as a string
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSendSuccess, OnDataSendError);
    }
    private void OnDataSendSuccess(UpdateUserDataResult result)
    {
        Debug.Log("✅ Player data saved successfully to PlayFab.");
    }

    private void OnDataSendError(PlayFabError error)
    {
        Debug.LogError("❌ Failed to save player data: " + error.GenerateErrorReport());
    }
    public void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

}
[System.Serializable]
public class LeaderboardUI
{
    public string statName;        // Leaderboard name in PlayFab
    public TextMeshProUGUI rankText;          // UI reference for Rank
    public TextMeshProUGUI scoreText;         // UI reference for Score
    public TextMeshProUGUI timerText;         // UI reference for countdown
}