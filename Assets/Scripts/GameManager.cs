using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum GameMode
{
    Free,
    Easy,
    Medium,
    Hard
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text finalScore;
    public GameObject gameOverPanel;
    public TextMeshProUGUI UserName;

    [Header("Gameplay Settings")]
    public GameObject game;

    // Gameplay variables
    public static bool isPlaying = false;
    public static float ObsVelocity = 0.2f;
    public static float BGVelocity = 0.01f;
    public  int Score = 0;
    public string TelegramUsername;
    public float timer = 0f;
    public static GameMode CurrentMode = GameMode.Free;
    private string _currentMode;
    private readonly Dictionary<GameMode, (float obsSpeed, float bgSpeed)> modeSettings =
        new Dictionary<GameMode, (float, float)>
        {
            { GameMode.Free,   (0.2f, 0.01f) },
            { GameMode.Easy,   (0.3f, 0.015f) },
            { GameMode.Medium, (0.4f, 0.02f) },
            { GameMode.Hard,   (0.5f, 0.025f) }
        };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GetUsername();

        if (game != null) game.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateScoreText();
    }

    public void StartGame(GameMode mode)
    {
        CurrentMode = mode;
        isPlaying = true;
        Score = 0;
        timer = 0f;

        // Set speeds based on mode
        if (modeSettings.TryGetValue(mode, out var settings))
        {
            ObsVelocity = settings.obsSpeed;
            BGVelocity = settings.bgSpeed;
        }

        UpdateScoreText();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Debug.Log($"Game started in {mode} mode.");
    }

    private float nextTime = 0f;

    private void Update()
    {
        if (isPlaying)
        {
            if (isPlaying)
            {
                timer += Time.deltaTime;
                if (timer >= 1f)
                {
                    Score++;
                    UpdateScoreText();
                    timer -= 1f;
                }
            }


            ObsVelocity += 0.008f * Time.deltaTime;
            BGVelocity += 0.0005f * Time.deltaTime;
        }
        else if (gameOverPanel != null && !gameOverPanel.activeSelf)
        {
            finalScore.text = scoreText != null ? scoreText.text : "0";
            gameOverPanel.SetActive(true);
        }
    }


    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = Score.ToString();
        else
            Debug.LogWarning("ScoreText is not assigned in the inspector.");
    }

    public void GetUsername()
    {
        TelegramBridge bridge = new TelegramBridge();
        string username = bridge.GetUsername();
        SetTelegramUsername(username);
        Debug.Log("Telegram username: " + username);
    }

    public void SetTelegramUsername(string username)
    {
        TelegramUsername = username;
        if (UserName != null)
            UserName.text = TelegramUsername;
        Debug.Log("Telegram Username: " + username);
    }

    public void SubmitFinalScoreToLeaderboard()
    {
        string leaderboardName = CurrentMode switch
        {
            GameMode.Free => "Leaderboard_Free",
            GameMode.Easy => "Leaderboard_Easy",
            GameMode.Medium => "Leaderboard_Medium",
            GameMode.Hard => "Leaderboard_Hard",
            _ => "Leaderboard_Free"
        };

        if (PlayFabManager.Instance != null)
            PlayFabManager.Instance.SubmitScore(leaderboardName, Score);
        else
            Debug.LogError("PlayFabManager instance not found!");
    }

    public void RestartGame()
    {
        ResetGameEntities();
        StartGame(CurrentMode);
    }

    public void BACKTOMAinMEnu()
    {
        ResetGameEntities();
        isPlaying = false;

        if (game != null) game.SetActive(false);

        Score = 0;
        ObsVelocity = 0.2f;
        BGVelocity = 0.01f;
        timer = 0f;
        UpdateScoreText();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void ResetGameEntities()
    {
        foreach (ObstacleLogic obstacle in FindObjectsOfType<ObstacleLogic>())
        {
            if (obstacle != null)
                obstacle.ResetPosition();
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.ResetPlayerPosition();
    }

    public void PlayGameButtonClick(string gameMode)
    {
        StartCoroutine(PlayGame(gameMode));
    }

    private IEnumerator PlayGame(string gameMode)
    {
        SetCurrentMode(gameMode);
         yield return LoadingScreen.ShowLoadingAndWait();

        if (game != null)
            game.SetActive(true);
        else
            Debug.LogWarning("Game object is not assigned in the inspector.");

        StartGame(CurrentMode);
    }
    public void SetCurrentMode(string mode)
    {
        if (System.Enum.TryParse(mode, true, out GameMode parsedMode))
        {
            CurrentMode = parsedMode;
            Debug.Log("Current mode set to: " + CurrentMode);
        }
        else
        {
            Debug.LogWarning("Invalid mode! Use Free, Easy, Medium, or Hard.");
        }
    }

}
