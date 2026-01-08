using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Themes;

namespace TimeCrax.Quiz
{
    /// <summary>
    /// Interface do usuário para o sistema de quiz.
    /// Gerencia os 4 painéis de quiz e feedback visual.
    /// </summary>
    public class QuizUI : MonoBehaviour
    {
        [Header("Main Canvas")]
        [SerializeField] private GameObject quizCanvas;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Common Elements")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Image timerBar;
        [SerializeField] private GameObject resultFeedback;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Image resultIcon;

        [Header("Image Quiz Panel")]
        [SerializeField] private GameObject imageQuizPanel;
        [SerializeField] private List<Button> imageOptionButtons;
        [SerializeField] private List<RawImage> imageOptionImages;

        [Header("Text Quiz Panel")]
        [SerializeField] private GameObject textQuizPanel;
        [SerializeField] private List<Button> textOptionButtons;
        [SerializeField] private List<TextMeshProUGUI> textOptionLabels;

        [Header("True/False Panel")]
        [SerializeField] private GameObject trueFalsePanel;
        [SerializeField] private Button trueButton;
        [SerializeField] private Button falseButton;
        [SerializeField] private TextMeshProUGUI statementText;

        [Header("Correlation Panel")]
        [SerializeField] private GameObject correlationPanel;
        [SerializeField] private List<RawImage> correlationImages;
        [SerializeField] private List<TextMeshProUGUI> correlationTexts;

        [Header("Feedback Settings")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color wrongColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private float feedbackDuration = 2f;

        private QuizManager quizManager;
        private List<int> correlationOrder;
        private bool isInteractable = true;

        private void Start()
        {
            quizManager = QuizManager.Instance;

            if (quizManager != null)
            {
                quizManager.OnTimerUpdated += UpdateTimer;
            }

            // Configurar botões
            SetupButtons();

            // Inicialmente escondido
            HideAllPanels();
            if (quizCanvas != null)
                quizCanvas.SetActive(false);
        }

        private void OnDestroy()
        {
            if (quizManager != null)
            {
                quizManager.OnTimerUpdated -= UpdateTimer;
            }
        }

        #region Setup

        private void SetupButtons()
        {
            // Image Quiz buttons
            for (int i = 0; i < imageOptionButtons.Count; i++)
            {
                int index = i;
                imageOptionButtons[i].onClick.AddListener(() => OnImageOptionClicked(index));
            }

            // Text Quiz buttons
            for (int i = 0; i < textOptionButtons.Count; i++)
            {
                int index = i;
                textOptionButtons[i].onClick.AddListener(() => OnTextOptionClicked(index));
            }

            // True/False buttons
            if (trueButton != null)
                trueButton.onClick.AddListener(() => OnTrueFalseClicked(true));
            if (falseButton != null)
                falseButton.onClick.AddListener(() => OnTrueFalseClicked(false));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Exibe o quiz para a carta especificada
        /// </summary>
        public void ShowQuiz(ThemeCard card, QuizType quizType)
        {
            if (card == null || quizCanvas == null) return;

            DebugHelper.Log($"[QuizUI] Exibindo quiz tipo {quizType}");

            HideAllPanels();
            quizCanvas.SetActive(true);
            isInteractable = true;

            if (resultFeedback != null)
                resultFeedback.SetActive(false);

            switch (quizType)
            {
                case QuizType.ImageQuiz:
                    ShowImageQuiz(card.quizData.imageQuiz);
                    break;

                case QuizType.TextQuiz:
                    ShowTextQuiz(card.quizData.textQuiz);
                    break;

                case QuizType.TrueFalseQuiz:
                    ShowTrueFalseQuiz(card.quizData.trueFalseQuiz);
                    break;

                case QuizType.CorrelationQuiz:
                    ShowCorrelationQuiz(card.quizData.correlationQuiz);
                    break;
            }

            // Fade in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f);
            }
        }

        /// <summary>
        /// Esconde o quiz e mostra feedback
        /// </summary>
        public void HideQuiz(bool correct)
        {
            isInteractable = false;

            // Mostrar feedback
            ShowFeedback(correct);

            // Esconder após delay
            this.DelayedCall(feedbackDuration, () =>
            {
                if (canvasGroup != null)
                {
                    LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f).setOnComplete(() =>
                    {
                        quizCanvas.SetActive(false);
                        HideAllPanels();
                    });
                }
                else
                {
                    quizCanvas.SetActive(false);
                    HideAllPanels();
                }
            });
        }

        #endregion

        #region Quiz Display Methods

        private void ShowImageQuiz(ImageQuiz quiz)
        {
            if (quiz == null || imageQuizPanel == null) return;

            imageQuizPanel.SetActive(true);

            if (questionText != null)
                questionText.text = quiz.question;

            // Carregar imagens das opções
            for (int i = 0; i < imageOptionImages.Count && i < quiz.options.Count; i++)
            {
                var option = quiz.options[i];
                var rawImage = imageOptionImages[i];

                if (!string.IsNullOrEmpty(option.localImagePath))
                {
                    var texture = ThemeStorage.LoadLocalImage(option.localImagePath);
                    if (texture != null)
                        rawImage.texture = texture;
                }

                imageOptionButtons[i].gameObject.SetActive(true);
            }

            // Esconder botões extras
            for (int i = quiz.options.Count; i < imageOptionButtons.Count; i++)
            {
                imageOptionButtons[i].gameObject.SetActive(false);
            }
        }

        private void ShowTextQuiz(TextQuiz quiz)
        {
            if (quiz == null || textQuizPanel == null) return;

            textQuizPanel.SetActive(true);

            if (questionText != null)
                questionText.text = quiz.question;

            // Configurar opções de texto
            for (int i = 0; i < textOptionLabels.Count && i < quiz.options.Count; i++)
            {
                textOptionLabels[i].text = quiz.options[i].text;
                textOptionButtons[i].gameObject.SetActive(true);
            }

            // Esconder botões extras
            for (int i = quiz.options.Count; i < textOptionButtons.Count; i++)
            {
                textOptionButtons[i].gameObject.SetActive(false);
            }
        }

        private void ShowTrueFalseQuiz(TrueFalseQuiz quiz)
        {
            if (quiz == null || trueFalsePanel == null) return;

            trueFalsePanel.SetActive(true);

            if (statementText != null)
                statementText.text = quiz.statement;

            if (questionText != null)
                questionText.text = "Verdadeiro ou Falso?";
        }

        private void ShowCorrelationQuiz(CorrelationQuiz quiz)
        {
            if (quiz == null || correlationPanel == null) return;

            correlationPanel.SetActive(true);

            if (questionText != null)
                questionText.text = "Associe as imagens aos textos corretos";

            // Inicializar ordem de correlação (embaralhada)
            correlationOrder = new List<int>();
            for (int i = 0; i < quiz.items.Count; i++)
            {
                correlationOrder.Add(i);
            }
            // Embaralhar
            for (int i = correlationOrder.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = correlationOrder[i];
                correlationOrder[i] = correlationOrder[j];
                correlationOrder[j] = temp;
            }

            // Carregar imagens (ordem original)
            for (int i = 0; i < correlationImages.Count && i < quiz.items.Count; i++)
            {
                var item = quiz.items[i];
                if (!string.IsNullOrEmpty(item.localImagePath))
                {
                    var texture = ThemeStorage.LoadLocalImage(item.localImagePath);
                    if (texture != null)
                        correlationImages[i].texture = texture;
                }
            }

            // Carregar textos (ordem embaralhada)
            for (int i = 0; i < correlationTexts.Count && i < quiz.items.Count; i++)
            {
                int shuffledIndex = correlationOrder[i];
                correlationTexts[i].text = quiz.items[shuffledIndex].text;
            }
        }

        #endregion

        #region Button Handlers

        private void OnImageOptionClicked(int index)
        {
            if (!isInteractable || quizManager == null) return;

            DebugHelper.Log($"[QuizUI] Imagem selecionada: {index}");
            isInteractable = false;
            quizManager.SubmitAnswer(index);
        }

        private void OnTextOptionClicked(int index)
        {
            if (!isInteractable || quizManager == null) return;

            DebugHelper.Log($"[QuizUI] Texto selecionado: {index}");
            isInteractable = false;
            quizManager.SubmitAnswer(index);
        }

        private void OnTrueFalseClicked(bool answer)
        {
            if (!isInteractable || quizManager == null) return;

            DebugHelper.Log($"[QuizUI] V/F selecionado: {answer}");
            isInteractable = false;
            quizManager.SubmitTrueFalseAnswer(answer);
        }

        /// <summary>
        /// Chamado quando o jogador confirma a correlação (botão de confirmar)
        /// </summary>
        public void OnCorrelationConfirmed()
        {
            if (!isInteractable || quizManager == null || correlationOrder == null) return;

            DebugHelper.Log("[QuizUI] Correlação confirmada");
            isInteractable = false;
            quizManager.SubmitCorrelationAnswer(correlationOrder);
        }

        #endregion

        #region UI Helpers

        private void HideAllPanels()
        {
            if (imageQuizPanel != null) imageQuizPanel.SetActive(false);
            if (textQuizPanel != null) textQuizPanel.SetActive(false);
            if (trueFalsePanel != null) trueFalsePanel.SetActive(false);
            if (correlationPanel != null) correlationPanel.SetActive(false);
        }

        private void UpdateTimer(float normalizedTime)
        {
            if (timerBar != null)
            {
                timerBar.fillAmount = normalizedTime;

                // Mudar cor quando estiver acabando
                if (normalizedTime < 0.25f)
                    timerBar.color = wrongColor;
                else
                    timerBar.color = Color.white;
            }
        }

        private void ShowFeedback(bool correct)
        {
            if (resultFeedback == null) return;

            resultFeedback.SetActive(true);

            if (resultText != null)
            {
                resultText.text = correct ? "CORRETO!" : "ERRADO!";
                resultText.color = correct ? correctColor : wrongColor;
            }

            if (resultIcon != null)
            {
                resultIcon.color = correct ? correctColor : wrongColor;
            }
        }

        #endregion
    }
}
