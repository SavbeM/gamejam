using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroComicsController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image comicsImage;
    [SerializeField] private Image fadeImage;

    [Header("Data")]
    [SerializeField] private Sprite[] panels;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private string nextSceneName = "UIScene";
    [SerializeField] private bool showEndingButtonsWhenNoNextScene = true;
    [SerializeField] private string restartSceneName = "UIScene";
    [SerializeField] private string exitSceneName = "IntroScene";

    private int currentPanel = 0;
    private bool isTransitioning = false;
    private bool IsEndingComics => showEndingButtonsWhenNoNextScene && string.IsNullOrWhiteSpace(nextSceneName);

    private void Start()
    {
        fadeImage.color = Color.black;
        comicsImage.sprite = panels[0];
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (isTransitioning) return;
        if (IsEndingComics && currentPanel >= panels.Length - 1) return;

        if (Input.GetMouseButtonDown(0) || 
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            NextPanel();
        }
    }

    private void NextPanel()
    {
        if (currentPanel >= panels.Length - 1)
        {
            if (IsEndingComics)
            {
                return;
            }

            StartCoroutine(FadeToScene());
            return;
        }

        currentPanel++;
        StartCoroutine(FadeToPanel(panels[currentPanel]));
    }

    private IEnumerator FadeIn()
    {
        isTransitioning = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - t / fadeDuration);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
        isTransitioning = false;
    }

    private IEnumerator FadeToPanel(Sprite next)
    {
        isTransitioning = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }

        comicsImage.sprite = next;

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - t / fadeDuration);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        isTransitioning = false;
    }

    private IEnumerator FadeToScene()
    {
        isTransitioning = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnGUI()
    {
        if (!IsEndingComics || isTransitioning || currentPanel < panels.Length - 1)
        {
            return;
        }

        float buttonWidth = 220f;
        float buttonHeight = 70f;
        float spacing = 24f;
        float totalWidth = buttonWidth * 2f + spacing;
        float startX = (Screen.width - totalWidth) * 0.5f;
        float y = Screen.height - buttonHeight - 40f;

        if (GUI.Button(new Rect(startX, y, buttonWidth, buttonHeight), "Restart"))
        {
            RestartGame();
        }

        if (GUI.Button(new Rect(startX + buttonWidth + spacing, y, buttonWidth, buttonHeight), "Exit"))
        {
            ExitGame();
        }
    }

    private void RestartGame()
    {
        if (string.IsNullOrWhiteSpace(restartSceneName))
        {
            Debug.LogError("[IntroComicsController] Restart scene name is empty.");
            return;
        }

        SceneManager.LoadScene(restartSceneName);
    }

    private void ExitGame()
    {
        if (!string.IsNullOrWhiteSpace(exitSceneName))
        {
            SceneManager.LoadScene(exitSceneName);
            return;
        }

        Application.Quit();
    }
}
