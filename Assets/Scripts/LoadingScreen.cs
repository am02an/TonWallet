using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    [Header("UI References")]
    public GameObject loadingPanel;
    public Slider loadingSlider;
    public TextMeshProUGUI percentText;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartCoroutine(ShowLoadingScreen());
    }
    public void StartLoading()
    {
        StartCoroutine(ShowLoadingScreen());

    }
    IEnumerator ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);
        float elapsed = 0f;

        // Phase 1: 0% to 60%
        while (loadingSlider.value < 0.6f)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(0f, 0.6f, elapsed / 1f); // 1 sec
            loadingSlider.value = value;
            percentText.text = Mathf.RoundToInt(value * 100) + "%";
            yield return null;
        }

        // Phase 2: 60% to 90%
        elapsed = 0f;
        while (loadingSlider.value < 0.9f)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(0.6f, 0.9f, elapsed / 1f); // 1 sec
            loadingSlider.value = value;
            percentText.text = Mathf.RoundToInt(value * 100) + "%";
            yield return null;
        }

        // Phase 3: 90% to 100%
        elapsed = 0f;
        while (loadingSlider.value < 1f)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(0.9f, 1f, elapsed / 1f); // 1 sec
            loadingSlider.value = value;
            percentText.text = Mathf.RoundToInt(value * 100) + "%";
            yield return null;
        }

        // Hold for 0.2s then close
        yield return new WaitForSeconds(0.2f);
        loadingPanel.SetActive(false);
    }
}
