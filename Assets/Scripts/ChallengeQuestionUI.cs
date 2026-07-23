using UnityEngine;
using TMPro;

public class ChallengeQuestionUI : MonoBehaviour
{
    public static ChallengeQuestionUI Instance { get; private set; }

    [SerializeField] private CanvasGroup background;
    [SerializeField] private TMP_Text questionText;

    private void Awake()
    {
        Instance = this;
        if (background != null)
        {
            background.alpha = 0f;
            background.blocksRaycasts = false;
        }
        GameStateManager.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        GameStateManager.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(GamePhase previous, GamePhase next)
    {
        if (next == GamePhase.IM_ChallengeFeedback)
            Hide();
    }

    public void Show(string question)
    {
        if (questionText != null)
            questionText.text = question ?? string.Empty;

        if (background != null)
        {
            LeanTween.cancel(background.gameObject);
            background.alpha = 0f;
            LeanTween.alphaCanvas(background, 1f, 1f);
        }
    }

    public void Hide()
    {
        if (background != null)
        {
            LeanTween.cancel(background.gameObject);
            LeanTween.alphaCanvas(background, 0f, 0.3f);
        }

        questionText.text = string.Empty;
    }
}
