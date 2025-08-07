using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed for restarting

public class GameManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public GameObject gameOverPanel; // Reference to the Game Over panel

    public static bool isPlaying = true;
    public static float ObsVelocity = 0.2f;
    public static float BGVelocity = 0.01f;
    public static int Score = 0;

    private float timer = 0f;

    private void Start()
    {
        isPlaying = true;
        Score = 0;
        ObsVelocity = 0.2f;
        BGVelocity = 0.01f;
        UpdateScoreText();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // Hide at start
        }
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
            {
                gameOverPanel.SetActive(true); // Show Game Over UI
            }
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "" + Score;
        }
        else
        {
            Debug.LogWarning("scoreText is not assigned in the GameManager script.");
        }
    }

    // Called from the Restart Button
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reloads current scene
    }
}
