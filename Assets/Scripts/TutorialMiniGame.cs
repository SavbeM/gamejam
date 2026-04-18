using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMiniGame : MiniGameBase
{
    [Header("Refs")]
    [SerializeField] private List<GameObject> panels; // Panel_1, Panel_2, Panel_3

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    private int currentPanel = 0;
    private bool isAnimating = false;

    public override void Setup(System.Action<MiniGameResult> onFinished)
    {
        base.Setup(onFinished);

        // Показываем только первую панель
        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(i == 0);
        }
    }

    public override void Begin()
    {
        base.Begin();
    }

    public override void Cleanup()
    {
        base.Cleanup();
    }

    private void Update()
    {
        if (!IsRunning || IsFinished || isAnimating) return;

        if (Input.GetMouseButtonDown(0) ||
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            NextPanel();
        }
    }

    private void NextPanel()
    {
        currentPanel++;

        if (currentPanel >= panels.Count)
        {
            Finish(MiniGameResult.Win);
            return;
        }

        StartCoroutine(SwitchPanel(panels[currentPanel - 1], panels[currentPanel]));
    }

    private IEnumerator SwitchPanel(GameObject from, GameObject to)
    {
        isAnimating = true;

        // Fade out старой панели
        CanvasGroup cgFrom = GetOrAddCanvasGroup(from);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cgFrom.alpha = 1f - t / fadeDuration;
            yield return null;
        }
        from.SetActive(false);

        // Fade in новой панели
        to.SetActive(true);
        CanvasGroup cgTo = GetOrAddCanvasGroup(to);
        cgTo.alpha = 0f;
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cgTo.alpha = t / fadeDuration;
            yield return null;
        }
        cgTo.alpha = 1f;

        isAnimating = false;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }
}