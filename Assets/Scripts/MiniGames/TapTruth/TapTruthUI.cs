using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TapTruthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TextMeshProUGUI[] answerTexts;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor   = Color.red;

    private Action<int> _onAnswerSelected;

    public void Display(TapTruthQuestion question, Action<int> onAnswerSelected)
    {
        _onAnswerSelected = onAnswerSelected;

        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerTexts[i].text = question.answers[i];
            answerButtons[i].image.color = defaultColor;
            answerButtons[i].interactable = true;

            int captured = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => _onAnswerSelected?.Invoke(captured));
        }
    }

    public void ShowFeedback(bool isCorrect, Action onDone)
    {
        foreach (var btn in answerButtons)
            btn.interactable = false;

        StartCoroutine(FeedbackRoutine(isCorrect, onDone));
    }

    private IEnumerator FeedbackRoutine(bool isCorrect, Action onDone)
    {
        Color feedbackColor = isCorrect ? correctColor : wrongColor;

        foreach (var btn in answerButtons)
            btn.image.color = feedbackColor;

        yield return new WaitForSeconds(0.6f);

        onDone?.Invoke();
    }
}