using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed for scene changes
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text scoreText;
    public GameObject gameOverPanel; // Game Over panel reference

    [Header("Gameplay Settings")]
    public GameObject game;

    // Static gameplay variables
    public static bool isPlaying = true;
    public static float ObsVelocity = 0.2f;
    public static float BGVelocity = 0.01f;
    public static int Score = 0;

    private float timer = 0f;

    private void Start()
    {
        // Optional: Start game automatically or wait for Play button
        if (game != null)
            game.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateScoreText();
    }

    public void StartGame()
    {
        // From any script:

        isPlaying = true;
        Debug.Log(isPlaying);
        Score = 0;
        ObsVelocity = 0.2f;
        BGVelocity = 0.01f;
        timer = 0f;

        UpdateScoreText();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (isPlaying)
        {
            timer += Time.deltaTime;

            if (timer >= 1f)
            {
                Score++;
                UpdateScoreText();
                timer = 0f;
            }

            ObsVelocity += 0.008f * Time.deltaTime;
            BGVelocity += 0.0005f * Time.deltaTime;
        }
        else
        {
            if (gameOverPanel != null && !gameOverPanel.activeSelf)
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

    // Called from Restart button
    public void RestartGame()
    {
        // Find all obstacles in the scene
        ObstacleLogic[] obstacles = FindObjectsOfType<ObstacleLogic>();
        PlayerController player = FindObjectOfType<PlayerController>();
        // Reset each obstacle's position
        foreach (ObstacleLogic obstacle in obstacles)
        {
            if (obstacle != null)
                obstacle.ResetPosition();
        }
        player.ResetPlayerPosition();

        // Restart the gameplay state
        StartGame();
    }


    public void BACKTOMAinMEnu()
    {
        // Reset everything as if RestartGame was called
        ObstacleLogic[] obstacles = FindObjectsOfType<ObstacleLogic>();
        PlayerController player = FindObjectOfType<PlayerController>();

        foreach (ObstacleLogic obstacle in obstacles)
        {
            if (obstacle != null)
                obstacle.ResetPosition();
        }

        if (player != null)
            player.ResetPlayerPosition();

        // Stop the game and hide the gameplay area
        isPlaying = false;
        game.SetActive(false);

        // Reset score and speeds so it's ready for next play
        Score = 0;
        ObsVelocity = 0.2f;
        BGVelocity = 0.01f;
        timer = 0f;

        UpdateScoreText();

        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }


    public void PlayGameButtonClick()
    {
        StartCoroutine(PlayGame());
    }

    private IEnumerator PlayGame()
    {
        // Show loading and wait until it’s done
        yield return LoadingScreen.ShowLoadingAndWait();

        if (game != null)
            game.SetActive(true);
        else
            Debug.LogWarning("Game object is not assigned in the inspector.");

        StartGame();
    }

}
