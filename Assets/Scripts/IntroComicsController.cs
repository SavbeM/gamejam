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

    private int currentPanel = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        fadeImage.color = Color.black;
        comicsImage.sprite = panels[0];
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (isTransitioning) return;

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
            // Уже на последней картинке — переходим в игру
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
}