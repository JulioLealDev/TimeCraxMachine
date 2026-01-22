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
    /// Gerencia a instanciação de prefabs de quiz e feedback visual.
    /// </summary>
    public class QuizUI : MonoBehaviour
    {
        [Header("Quiz Canvas")]
        [SerializeField] private GameObject quizCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image blocker;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI quizTypeLabel;
        [SerializeField] private Transform container;

        [Header("Quiz Prefabs")]
        [SerializeField] private GameObject textQuizPrefab;
        [SerializeField] private GameObject imageQuizPrefab;
        [SerializeField] private GameObject trueOrFalseQuizPrefab;
        [SerializeField] private GameObject correlationQuizPrefab;

        [Header("Feedback Settings")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color wrongColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private float feedbackDuration = 2f;

        [Header("Audio")]
        [SerializeField] private SoundEffects soundEffects;

        [Header("Correlation Number Sprites")]
        [SerializeField] private Sprite emptyNumberSprite;
        [SerializeField] private Sprite number1Sprite;
        [SerializeField] private Sprite number2Sprite;
        [SerializeField] private Sprite number3Sprite;

        // Referências do quiz instanciado
        private GameObject currentQuizInstance;
        private QuizManager quizManager;
        private bool isInteractable = true;

        // Referências cacheadas do quiz atual
        private TextMeshProUGUI questionText;
        private List<Button> optionButtons = new List<Button>();
        private List<TextMeshProUGUI> optionTexts = new List<TextMeshProUGUI>();
        private List<RawImage> optionImages = new List<RawImage>();
        private Button trueButton;
        private Button falseButton;

        // Correlação
        private List<int> correlationOrder;
        private List<RawImage> correlationImages = new List<RawImage>();
        private List<TextMeshProUGUI> correlationTexts = new List<TextMeshProUGUI>();

        // Novo sistema de correlação com botões de número
        private int[] playerSelections = new int[3]; // Valores selecionados pelo jogador (0=vazio, 1, 2, 3)
        private int[] correctValues = new int[3]; // Valores corretos para cada imagem
        private List<Image> numberImageComponents = new List<Image>(); // Imagens de número nos botões
        private List<Button> correlationButtons = new List<Button>(); // Botões de seleção
        private Button confirmButton; // Botão de confirmar correlação

        private void Start()
        {
            quizManager = QuizManager.Instance;

            if (quizManager != null)
            {
                quizManager.OnTimerUpdated += UpdateTimer;
            }

            // Inicialmente escondido
            if (quizCanvas != null)
                quizCanvas.SetActive(false);
        }

        private void OnDestroy()
        {
            if (quizManager != null)
            {
                quizManager.OnTimerUpdated -= UpdateTimer;
            }

            // Limpar instância atual se existir
            if (currentQuizInstance != null)
            {
                Destroy(currentQuizInstance);
            }
        }

        #region Public Methods

        /// <summary>
        /// Exibe o quiz para a carta especificada
        /// </summary>
        public void ShowQuiz(ThemeCard card, QuizType quizType)
        {
            if (card == null || quizCanvas == null) return;

            DebugHelper.Log($"[QuizUI] Exibindo quiz tipo {quizType}");

            // Limpar instância anterior
            if (currentQuizInstance != null)
            {
                Destroy(currentQuizInstance);
                currentQuizInstance = null;
            }

            // Limpar listas
            ClearCachedReferences();

            quizCanvas.SetActive(true);
            isInteractable = true;

            // Definir label do tipo de quiz
            SetQuizTypeLabel(quizType);

            // Instanciar prefab correto
            switch (quizType)
            {
                case QuizType.ImageQuiz:
                    InstantiateImageQuiz(card.quizData.imageQuiz);
                    break;

                case QuizType.TextQuiz:
                    InstantiateTextQuiz(card.quizData.textQuiz);
                    break;

                case QuizType.TrueFalseQuiz:
                    InstantiateTrueFalseQuiz(card.quizData.trueFalseQuiz);
                    break;

                case QuizType.CorrelationQuiz:
                    InstantiateCorrelationQuiz(card.quizData.correlationQuiz);
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

            // Mostrar feedback visual
            ShowFeedback(correct);

            // Esconder após delay
            this.DelayedCall(feedbackDuration, () =>
            {
                if (canvasGroup != null)
                {
                    LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f).setOnComplete(() =>
                    {
                        CleanupQuiz();
                    });
                }
                else
                {
                    CleanupQuiz();
                }
            });
        }

        #endregion

        #region Prefab Instantiation

        private void InstantiateTextQuiz(TextQuiz quiz)
        {
            if (quiz == null || textQuizPrefab == null || container == null) return;

            currentQuizInstance = Instantiate(textQuizPrefab, container);

            // Buscar referências na hierarquia
            // Header/Question
            var questionTransform = currentQuizInstance.transform.Find("Header/Question");
            if (questionTransform != null)
            {
                questionText = questionTransform.GetComponent<TextMeshProUGUI>();
                if (questionText != null)
                    questionText.text = quiz.question;
            }

            // Options/AnswerButton_01, 02, 03, 04
            var optionsTransform = currentQuizInstance.transform.Find("Options");
            if (optionsTransform != null)
            {
                for (int i = 1; i <= 4; i++)
                {
                    var buttonTransform = optionsTransform.Find($"AnswerButton_0{i}");
                    if (buttonTransform != null)
                    {
                        var button = buttonTransform.GetComponent<Button>();
                        var answerTextTransform = buttonTransform.Find("AnswerText");
                        var answerText = answerTextTransform?.GetComponent<TextMeshProUGUI>();

                        if (button != null && answerText != null)
                        {
                            int optionIndex = i - 1;
                            optionButtons.Add(button);
                            optionTexts.Add(answerText);

                            // Configurar texto se houver opção
                            if (optionIndex < quiz.options.Count)
                            {
                                answerText.text = quiz.options[optionIndex].text;
                                button.gameObject.SetActive(true);
                                button.onClick.AddListener(() => OnOptionClicked(optionIndex));
                            }
                            else
                            {
                                button.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private void InstantiateImageQuiz(ImageQuiz quiz)
        {
            if (quiz == null || imageQuizPrefab == null || container == null)
            {
                DebugHelper.Log($"[QuizUI] InstantiateImageQuiz ERRO: quiz={quiz != null}, prefab={imageQuizPrefab != null}, container={container != null}");
                return;
            }

            DebugHelper.Log($"[QuizUI] InstantiateImageQuiz: question={quiz.question}, options={quiz.options?.Count ?? 0}");

            currentQuizInstance = Instantiate(imageQuizPrefab, container);

            // Header/Question
            var questionTransform = currentQuizInstance.transform.Find("Header/Question");
            if (questionTransform != null)
            {
                questionText = questionTransform.GetComponent<TextMeshProUGUI>();
                if (questionText != null)
                    questionText.text = quiz.question;
            }

            // Options/AnswerButton_01, 02, 03, 04
            var optionsTransform = currentQuizInstance.transform.Find("Options");

            if (optionsTransform != null)
            {
                for (int i = 1; i <= 4; i++)
                {
                    var buttonTransform = optionsTransform.Find($"AnswerButton_0{i}");

                    if (buttonTransform != null)
                    {
                        var button = buttonTransform.GetComponent<Button>();
                        var imageTransform = buttonTransform.Find("Image");

                        // Suportar tanto RawImage quanto Image (UI)
                        var rawImage = imageTransform?.GetComponent<RawImage>();
                        var uiImage = imageTransform?.GetComponent<Image>();

                        if (button != null)
                        {
                            int optionIndex = i - 1;
                            optionButtons.Add(button);

                            // Verificar se há opção para este índice
                            if (optionIndex < quiz.options.Count)
                            {
                                var option = quiz.options[optionIndex];

                                // Carregar imagem do arquivo local
                                if (!string.IsNullOrEmpty(option.localImagePath))
                                {
                                    var texture = ThemeStorage.LoadLocalImage(option.localImagePath);
                                    if (texture != null)
                                    {
                                        // Se tem RawImage, usar texture diretamente
                                        if (rawImage != null)
                                        {
                                            rawImage.texture = texture;
                                            optionImages.Add(rawImage);
                                        }
                                        // Se tem Image (UI), criar sprite da texture
                                        else if (uiImage != null)
                                        {
                                            var sprite = Sprite.Create(
                                                texture,
                                                new Rect(0, 0, texture.width, texture.height),
                                                new Vector2(0.5f, 0.5f)
                                            );
                                            uiImage.sprite = sprite;
                                        }
                                        DebugHelper.Log($"[QuizUI] Imagem {optionIndex} carregada com sucesso");
                                    }
                                    else
                                    {
                                        DebugHelper.Log($"[QuizUI] ERRO: Falha ao carregar imagem: {option.localImagePath}");
                                    }
                                }

                                button.gameObject.SetActive(true);
                                button.onClick.AddListener(() => OnOptionClicked(optionIndex));
                            }
                            else
                            {
                                button.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
            else
            {
                DebugHelper.Log("[QuizUI] ERRO: 'Options' não encontrado no prefab!");
            }
        }

        private void InstantiateTrueFalseQuiz(TrueFalseQuiz quiz)
        {
            if (quiz == null || trueOrFalseQuizPrefab == null || container == null) return;

            currentQuizInstance = Instantiate(trueOrFalseQuizPrefab, container);

            // Header/Question (statement)
            var questionTransform = currentQuizInstance.transform.Find("Header/Question");
            if (questionTransform != null)
            {
                questionText = questionTransform.GetComponent<TextMeshProUGUI>();
                if (questionText != null)
                    questionText.text = quiz.statement;
            }

            // Options/TrueButton
            var optionsTransform = currentQuizInstance.transform.Find("Options");
            if (optionsTransform != null)
            {
                var trueTransform = optionsTransform.Find("TrueButton");
                if (trueTransform != null)
                {
                    trueButton = trueTransform.GetComponent<Button>();
                    if (trueButton != null)
                    {
                        trueButton.onClick.AddListener(() => OnTrueFalseClicked(true));
                    }
                }

                var falseTransform = optionsTransform.Find("FalseButton");
                if (falseTransform != null)
                {
                    falseButton = falseTransform.GetComponent<Button>();
                    if (falseButton != null)
                    {
                        falseButton.onClick.AddListener(() => OnTrueFalseClicked(false));
                    }
                }
            }
        }

        private void InstantiateCorrelationQuiz(CorrelationQuiz quiz)
        {
            if (quiz == null || correlationQuizPrefab == null || container == null) return;

            DebugHelper.Log($"[QuizUI] InstantiateCorrelationQuiz: items={quiz.items?.Count ?? 0}");

            currentQuizInstance = Instantiate(correlationQuizPrefab, container);

            // Header/Question
            var questionTransform = currentQuizInstance.transform.Find("Header/Question");
            if (questionTransform != null)
            {
                questionText = questionTransform.GetComponent<TextMeshProUGUI>();
                if (questionText != null)
                    questionText.text = "Associe as imagens aos textos corretos";
            }

            // Resetar arrays
            playerSelections = new int[3] { 0, 0, 0 }; // 0 = vazio
            correctValues = new int[3];
            numberImageComponents.Clear();
            correlationButtons.Clear();

            // Criar lista de índices e embaralhar para as IMAGENS
            correlationOrder = new List<int>();
            for (int i = 0; i < quiz.items.Count && i < 3; i++)
            {
                correlationOrder.Add(i);
            }
            ShuffleList(correlationOrder);

            var optionsTransform = currentQuizInstance.transform.Find("Options");
            if (optionsTransform == null)
            {
                DebugHelper.Log("[QuizUI] ERRO: 'Options' não encontrado no prefab!");
                return;
            }

            // Configurar TEXTOS em ordem fixa (posição 1 = texto 1, posição 2 = texto 2, etc)
            for (int i = 1; i <= 3; i++)
            {
                var textTransform = optionsTransform.Find($"AnswerTextImage_0{i}/AnswerText");
                if (textTransform != null)
                {
                    var tmpText = textTransform.GetComponent<TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        correlationTexts.Add(tmpText);
                        int textIndex = i - 1;
                        if (textIndex < quiz.items.Count)
                        {
                            // Textos sempre em ordem fixa (items[0] na posição 1, items[1] na posição 2, etc)
                            tmpText.text = quiz.items[textIndex].text;
                            DebugHelper.Log($"[QuizUI] Texto {i}: {quiz.items[textIndex].text}");
                        }
                    }
                }
            }

            // Configurar IMAGENS em ordem embaralhada
            // Cada imagem guarda seu valor correto (índice original + 1)
            for (int i = 1; i <= 3; i++)
            {
                int posIndex = i - 1;
                if (posIndex >= correlationOrder.Count) continue;

                int originalIndex = correlationOrder[posIndex]; // Índice original no array
                correctValues[posIndex] = originalIndex + 1; // Valor correto é o índice original + 1 (1, 2 ou 3)

                DebugHelper.Log($"[QuizUI] Imagem posição {i}: originalIndex={originalIndex}, correctValue={correctValues[posIndex]}");

                // Carregar imagem embaralhada
                var imageTransform = optionsTransform.Find($"AnswerImage_0{i}/AnswerImageContent");
                if (imageTransform != null && originalIndex < quiz.items.Count)
                {
                    var rawImage = imageTransform.GetComponent<RawImage>();
                    var uiImage = imageTransform.GetComponent<Image>();
                    var item = quiz.items[originalIndex];

                    if (!string.IsNullOrEmpty(item.localImagePath))
                    {
                        var texture = ThemeStorage.LoadLocalImage(item.localImagePath);
                        if (texture != null)
                        {
                            if (rawImage != null)
                            {
                                rawImage.texture = texture;
                                correlationImages.Add(rawImage);
                            }
                            else if (uiImage != null)
                            {
                                var sprite = Sprite.Create(
                                    texture,
                                    new Rect(0, 0, texture.width, texture.height),
                                    new Vector2(0.5f, 0.5f)
                                );
                                uiImage.sprite = sprite;
                            }
                            DebugHelper.Log($"[QuizUI] Imagem {i} carregada: {item.localImagePath}");
                        }
                    }
                }

                // Configurar botão de número para cada imagem
                var buttonTransform = optionsTransform.Find($"ButtonAnswerImage_0{i}");
                if (buttonTransform != null)
                {
                    var button = buttonTransform.GetComponent<Button>();
                    var numberImageTransform = buttonTransform.Find("NumberImage");
                    var numberImage = numberImageTransform?.GetComponent<Image>();

                    if (button != null)
                    {
                        correlationButtons.Add(button);

                        // Guardar referência da imagem de número
                        if (numberImage != null)
                        {
                            numberImageComponents.Add(numberImage);
                            // Iniciar com sprite vazio
                            numberImage.sprite = emptyNumberSprite;
                        }

                        // Adicionar listener para ciclar o valor
                        int buttonIndex = posIndex; // Capturar para o closure
                        button.onClick.AddListener(() => OnCorrelationButtonClicked(buttonIndex));

                        DebugHelper.Log($"[QuizUI] Botão {i} configurado");
                    }
                }
            }

            // Configurar botão de confirmar
            // Tentar encontrar em Options primeiro, depois na raiz do prefab
            var confirmButtonTransform = optionsTransform.Find("ConfirmButton");
            if (confirmButtonTransform == null)
            {
                confirmButtonTransform = currentQuizInstance.transform.Find("ConfirmButton");
                DebugHelper.Log($"[QuizUI] ConfirmButton não encontrado em Options, buscando na raiz: {confirmButtonTransform != null}");
            }

            if (confirmButtonTransform != null)
            {
                confirmButton = confirmButtonTransform.GetComponent<Button>();
                DebugHelper.Log($"[QuizUI] ConfirmButton encontrado, Button component: {confirmButton != null}");
                if (confirmButton != null)
                {
                    confirmButton.onClick.RemoveAllListeners(); // Limpar listeners anteriores
                    confirmButton.onClick.AddListener(OnCorrelationConfirmed);
                    DebugHelper.Log("[QuizUI] ConfirmButton configurado com sucesso!");
                }
                else
                {
                    DebugHelper.Log("[QuizUI] ERRO: ConfirmButton não tem componente Button!");
                }
            }
            else
            {
                DebugHelper.Log("[QuizUI] ERRO: ConfirmButton não encontrado no prefab!");
            }
        }

        #endregion

        #region Button Handlers

        private void OnOptionClicked(int index)
        {
            if (!isInteractable || quizManager == null) return;

            DebugHelper.Log($"[QuizUI] Opção selecionada: {index}");
            isInteractable = false;

            // Feedback visual no botão clicado
            if (index < optionButtons.Count)
            {
                HighlightButton(optionButtons[index]);
            }

            quizManager.SubmitAnswer(index);
        }

        private void OnTrueFalseClicked(bool answer)
        {
            if (!isInteractable || quizManager == null) return;

            DebugHelper.Log($"[QuizUI] V/F selecionado: {answer}");
            isInteractable = false;

            // Feedback visual
            if (answer && trueButton != null)
                HighlightButton(trueButton);
            else if (!answer && falseButton != null)
                HighlightButton(falseButton);

            quizManager.SubmitTrueFalseAnswer(answer);
        }

        /// <summary>
        /// Handler para clique nos botões de número da correlação
        /// Cicla os valores: 0 (vazio) → 1 → 2 → 3 → 1 → ...
        /// </summary>
        private void OnCorrelationButtonClicked(int buttonIndex)
        {
            if (!isInteractable) return;

            // Ciclar valor: 0→1→2→3→1→2→3...
            int currentValue = playerSelections[buttonIndex];
            int newValue;

            if (currentValue == 0)
                newValue = 1;
            else if (currentValue >= 3)
                newValue = 1;
            else
                newValue = currentValue + 1;

            playerSelections[buttonIndex] = newValue;

            // Atualizar imagem do número
            if (buttonIndex < numberImageComponents.Count && numberImageComponents[buttonIndex] != null)
            {
                Sprite newSprite = GetNumberSprite(newValue);
                numberImageComponents[buttonIndex].sprite = newSprite;
            }

            DebugHelper.Log($"[QuizUI] Botão {buttonIndex + 1} clicado: {currentValue} → {newValue}");
        }

        /// <summary>
        /// Retorna o sprite correspondente ao valor do número
        /// </summary>
        private Sprite GetNumberSprite(int value)
        {
            switch (value)
            {
                case 1: return number1Sprite;
                case 2: return number2Sprite;
                case 3: return number3Sprite;
                default: return emptyNumberSprite;
            }
        }

        private void OnCorrelationConfirmed()
        {
            DebugHelper.Log($"[QuizUI] OnCorrelationConfirmed chamado! isInteractable={isInteractable}, quizManager={quizManager != null}");

            if (!isInteractable)
            {
                DebugHelper.Log("[QuizUI] BLOQUEADO: isInteractable é false");
                return;
            }

            if (quizManager == null)
            {
                DebugHelper.Log("[QuizUI] BLOQUEADO: quizManager é null");
                return;
            }

            DebugHelper.Log("[QuizUI] Correlação confirmada - verificando respostas...");

            // Verificar se todas as posições foram preenchidas
            bool allFilled = true;
            for (int i = 0; i < playerSelections.Length; i++)
            {
                if (playerSelections[i] == 0)
                {
                    allFilled = false;
                    break;
                }
            }

            if (!allFilled)
            {
                DebugHelper.Log("[QuizUI] Nem todas as imagens foram marcadas!");
                // Poderia mostrar feedback visual aqui
                return;
            }

            // Verificar se as seleções estão corretas
            bool correct = true;
            for (int i = 0; i < playerSelections.Length && i < correctValues.Length; i++)
            {
                DebugHelper.Log($"[QuizUI] Posição {i + 1}: seleção={playerSelections[i]}, correto={correctValues[i]}");
                if (playerSelections[i] != correctValues[i])
                {
                    correct = false;
                }
            }

            DebugHelper.Log($"[QuizUI] Resultado da correlação: {(correct ? "CORRETO" : "ERRADO")}");
            isInteractable = false;

            // Enviar resultado para o QuizManager
            // Criamos uma lista com os valores na ordem correta se acertou, ou na ordem errada se errou
            var resultOrder = new List<int>();
            for (int i = 0; i < playerSelections.Length; i++)
            {
                // Se a seleção for igual ao valor correto, consideramos que está na posição correta
                if (playerSelections[i] == correctValues[i])
                    resultOrder.Add(i); // Posição correta
                else
                    resultOrder.Add(-1); // Posição incorreta (qualquer valor diferente de i)
            }

            quizManager.SubmitCorrelationAnswer(resultOrder);
        }

        #endregion

        #region UI Helpers

        private void SetQuizTypeLabel(QuizType quizType)
        {
            if (quizTypeLabel == null) return;

            switch (quizType)
            {
                case QuizType.ImageQuiz:
                    quizTypeLabel.text = "QUIZ DE IMAGENS";
                    break;
                case QuizType.TextQuiz:
                    quizTypeLabel.text = "QUIZ DE TEXTO";
                    break;
                case QuizType.TrueFalseQuiz:
                    quizTypeLabel.text = "VERDADEIRO OU FALSO";
                    break;
                case QuizType.CorrelationQuiz:
                    quizTypeLabel.text = "CORRELAÇÃO";
                    break;
                default:
                    quizTypeLabel.text = "QUIZ";
                    break;
            }
        }

        private void UpdateTimer(float normalizedTime)
        {
            // Timer pode ser implementado no prefab se necessário
            // Por enquanto, mudar cor do background quando estiver acabando
            if (backgroundImage != null && normalizedTime < 0.25f)
            {
                backgroundImage.color = Color.Lerp(backgroundImage.color, wrongColor, Time.deltaTime * 2f);
            }
        }

        private void HighlightButton(Button button)
        {
            if (button == null) return;

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.8f, 0.8f, 0.2f); // Amarelo para selecionado
            }
        }

        private void ShowFeedback(bool correct)
        {
            // Tocar som
            if (soundEffects != null)
            {
                if (correct)
                    soundEffects.PlayRightSlotSound();
                else
                    soundEffects.PlayWrongSlotSound();
            }

            // Mudar cor do background para feedback
            if (backgroundImage != null)
            {
                backgroundImage.color = correct ? correctColor : wrongColor;
            }

            // Atualizar label
            if (quizTypeLabel != null)
            {
                quizTypeLabel.text = correct ? "CORRETO!" : "ERRADO!";
                quizTypeLabel.color = correct ? correctColor : wrongColor;
            }
        }

        private void CleanupQuiz()
        {
            if (currentQuizInstance != null)
            {
                Destroy(currentQuizInstance);
                currentQuizInstance = null;
            }

            ClearCachedReferences();

            if (quizCanvas != null)
                quizCanvas.SetActive(false);

            // Resetar cor do background
            if (backgroundImage != null)
                backgroundImage.color = Color.white;

            // Resetar cor do label
            if (quizTypeLabel != null)
                quizTypeLabel.color = Color.white;
        }

        private void ClearCachedReferences()
        {
            // Remover listeners antes de limpar
            foreach (var button in optionButtons)
            {
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }

            foreach (var button in correlationButtons)
            {
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }

            if (trueButton != null)
                trueButton.onClick.RemoveAllListeners();

            if (falseButton != null)
                falseButton.onClick.RemoveAllListeners();

            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();

            optionButtons.Clear();
            optionTexts.Clear();
            optionImages.Clear();
            correlationImages.Clear();
            correlationTexts.Clear();
            correlationButtons.Clear();
            numberImageComponents.Clear();
            correlationOrder = null;
            questionText = null;
            trueButton = null;
            falseButton = null;
            confirmButton = null;
            playerSelections = new int[3];
            correctValues = new int[3];
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        #endregion
    }
}
