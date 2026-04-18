using System.Collections.Generic;
using UnityEngine;

public class TapTruthMiniGame : MiniGameBase
{
    [SerializeField] private TapTruthData data;
    [SerializeField] private TapTruthUI ui;

    private TapTruthQuestion _currentQuestion;
    private List<TapTruthQuestion> _remainingQuestions;
    private int _correctAnswers;

    private const int AnswersToWin = 3;

    public override void Begin()
    {
        base.Begin();
        _correctAnswers = 0;
        _remainingQuestions = new List<TapTruthQuestion>(data.questions);
        ShuffleList(_remainingQuestions);
        ShowNextQuestion();
    }

    private void ShowNextQuestion()
    {
        if (_remainingQuestions.Count == 0)
            _remainingQuestions = new List<TapTruthQuestion>(data.questions);

        _currentQuestion = _remainingQuestions[0];
        _remainingQuestions.RemoveAt(0);

        ui.Display(_currentQuestion, OnAnswerSelected);
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        bool isCorrect = selectedIndex == _currentQuestion.correctAnswerIndex;

        ui.ShowFeedback(isCorrect, () =>
        {
            if (isCorrect)
            {
                _correctAnswers++;
                Debug.Log($"Correct answers: {_correctAnswers}/{AnswersToWin}");

                if (_correctAnswers >= AnswersToWin)
                    Finish(MiniGameResult.Win);
                else
                    ShowNextQuestion();
            }
            else
            {
                // Добавляем вопрос обратно в конец списка
                _remainingQuestions.Add(_currentQuestion);
                ShowNextQuestion();
            }
        });
    }

    private void ShuffleList(List<TapTruthQuestion> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}