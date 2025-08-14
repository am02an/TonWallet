using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressSlider;
    public TMP_Text progressText;
    public GameObject loadingPanel;

    public static LoadingScreen Instance; // Singleton for easy access

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    private void Start()
    {
        StartCoroutine(StartLoading());
    }

    public IEnumerator StartLoading()
    {
        loadingPanel.SetActive(true);
        progressSlider.maxValue = 100;
        progressSlider.value = 40f;
        progressText.text = "40%";

        yield return StartCoroutine(FakeLoading());

        loadingPanel.SetActive(false);
    }

    private IEnumerator FakeLoading()
    {
        float currentProgress = 40f;
        progressSlider.value = currentProgress;
        progressText.text = Mathf.RoundToInt(currentProgress) + "%";

        while (currentProgress < 100f)
        {
            // Pick a new random target between current and 100
            float targetProgress = Mathf.Min(currentProgress + Random.Range(5f, 15f), 100f);

            // Smoothly move to the target
            float elapsed = 0f;
            float duration = 0.5f; // Smooth movement time

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, elapsed / duration);
                progressSlider.value = currentProgress;
                progressText.text = Mathf.RoundToInt(currentProgress) + "%";
                yield return null; // wait until next frame
            }

            currentProgress = targetProgress; // snap exactly to target
            yield return new WaitForSeconds(0.2f); // small pause before next jump
        }
    }


    // 🔹 Call this from anywhere without needing to write StartCoroutine manually
    public static IEnumerator ShowLoadingAndWait()
    {
        if (Instance != null)
        {
            yield return Instance.StartLoading(); // Wait until StartLoading finishes
        }
        else
        {
            Debug.LogError("LoadingScreen instance not found in the scene.");
        }
    }

}
