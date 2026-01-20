using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Themes;

namespace TimeCrax.Quiz
{
    /// <summary>
    /// Gerenciador de quiz para o sistema de temas.
    /// Controla o fluxo de quiz após o jogador acertar o slot.
    /// </summary>
    public class QuizManager : MonoBehaviourPunCallbacks
    {
        private static QuizManager _instance;
        public static QuizManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<QuizManager>();
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private float quizTimeLimit = 30f;

        // Eventos
        public event Action<bool> OnQuizCompleted;
        public event Action<ThemeCard, QuizType> OnQuizStarted;
        public event Action<float> OnTimerUpdated;

        // Estado atual
        private ThemeCard currentCard;
        private int currentSlotCount;
        private QuizType currentQuizType;
        private bool isQuizActive;
        private float remainingTime;

        // Referência ao UI
        private QuizUI quizUI;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void Start()
        {
            quizUI = FindFirstObjectByType<QuizUI>();
        }

        private void Update()
        {
            if (isQuizActive && remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                OnTimerUpdated?.Invoke(remainingTime / quizTimeLimit);

                if (remainingTime <= 0)
                {
                    // Tempo esgotado - quiz falhou
                    HandleQuizTimeout();
                }
            }
        }

        #region Public Methods

        /// <summary>
        /// Inicia um quiz para a carta especificada (chamado pelo jogador do turno)
        /// </summary>
        public void StartQuiz(ThemeCard card, int slotCount)
        {
            if (card == null || !card.quizData.HasQuiz)
            {
                DebugHelper.Log("[QuizManager] Carta sem quiz, completando diretamente");
                OnQuizCompleted?.Invoke(true);
                return;
            }

            currentCard = card;
            currentSlotCount = slotCount;
            currentQuizType = card.quizData.GetAvailableQuizType();

            DebugHelper.Log($"[QuizManager] Iniciando quiz tipo {currentQuizType} para slot {slotCount}");

            // Sincronizar início do quiz com todos os jogadores
            photonView.RPC("RPC_StartQuiz", RpcTarget.All, slotCount, (int)currentQuizType);
        }

        /// <summary>
        /// Submete resposta para quiz de múltipla escolha (ImageQuiz ou TextQuiz)
        /// </summary>
        public void SubmitAnswer(int selectedIndex)
        {
            if (!isQuizActive) return;

            bool correct = false;

            switch (currentQuizType)
            {
                case QuizType.ImageQuiz:
                    correct = currentCard.quizData.imageQuiz.correctIndex == selectedIndex;
                    break;

                case QuizType.TextQuiz:
                    correct = currentCard.quizData.textQuiz.correctIndex == selectedIndex;
                    break;
            }

            DebugHelper.Log($"[QuizManager] Resposta {selectedIndex} - Correto: {correct}");
            FinishQuiz(correct);
        }

        /// <summary>
        /// Submete resposta para quiz de verdadeiro/falso
        /// </summary>
        public void SubmitTrueFalseAnswer(bool answer)
        {
            if (!isQuizActive || currentQuizType != QuizType.TrueFalseQuiz) return;

            bool correct = currentCard.quizData.trueFalseQuiz.answer == answer;

            DebugHelper.Log($"[QuizManager] Resposta V/F: {answer} - Correto: {correct}");
            FinishQuiz(correct);
        }

        /// <summary>
        /// Submete resposta para quiz de correlação
        /// </summary>
        public void SubmitCorrelationAnswer(List<int> order)
        {
            if (!isQuizActive || currentQuizType != QuizType.CorrelationQuiz) return;

            // Verifica se a ordem está correta (0, 1, 2, 3...)
            bool correct = true;
            for (int i = 0; i < order.Count; i++)
            {
                if (order[i] != i)
                {
                    correct = false;
                    break;
                }
            }

            DebugHelper.Log($"[QuizManager] Correlação - Correto: {correct}");
            FinishQuiz(correct);
        }

        /// <summary>
        /// Verifica se há um quiz ativo
        /// </summary>
        public bool IsQuizActive => isQuizActive;

        /// <summary>
        /// Retorna a carta atual do quiz
        /// </summary>
        public ThemeCard GetCurrentCard() => currentCard;

        /// <summary>
        /// Retorna o tipo de quiz atual
        /// </summary>
        public QuizType GetCurrentQuizType() => currentQuizType;

        #endregion

        #region Private Methods

        private void FinishQuiz(bool correct)
        {
            isQuizActive = false;

            // Sincronizar resultado com todos os jogadores
            photonView.RPC("RPC_QuizResult", RpcTarget.All, currentSlotCount, correct);
        }

        private void HandleQuizTimeout()
        {
            DebugHelper.Log("[QuizManager] Tempo esgotado!");
            FinishQuiz(false);
        }

        #endregion

        #region RPCs

        [PunRPC]
        public void RPC_StartQuiz(int slotCount, int quizType)
        {
            currentSlotCount = slotCount;
            currentQuizType = (QuizType)quizType;
            isQuizActive = true;
            remainingTime = quizTimeLimit;

            // Buscar dados da carta no RandomMaterial
            var randomMaterial = FindFirstObjectByType<RandomMaterial>();
            if (randomMaterial != null)
            {
                var selectedCards = randomMaterial.GetSelectedCards();
                if (selectedCards != null)
                {
                    // Encontrar a carta pelo slotCount
                    var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
                    foreach (var eventCard in eventCards)
                    {
                        if (eventCard.slotCount == slotCount)
                        {
                            currentCard = eventCard.GetThemeCard();
                            break;
                        }
                    }
                }
            }

            DebugHelper.Log($"[QuizManager] RPC_StartQuiz - Slot: {slotCount}, Tipo: {currentQuizType}");

            // Notificar UI e listeners
            OnQuizStarted?.Invoke(currentCard, currentQuizType);

            // Mostrar UI do quiz
            if (quizUI != null)
            {
                quizUI.ShowQuiz(currentCard, currentQuizType);
            }
        }

        [PunRPC]
        public void RPC_QuizResult(int slotCount, bool correct)
        {
            DebugHelper.Log($"[QuizManager] RPC_QuizResult - Slot: {slotCount}, Correto: {correct}");

            isQuizActive = false;

            // Esconder UI do quiz
            if (quizUI != null)
            {
                quizUI.HideQuiz(correct);
            }

            // Notificar listeners
            OnQuizCompleted?.Invoke(correct);
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Limpar eventos para evitar memory leaks
            OnQuizCompleted = null;
            OnQuizStarted = null;
            OnTimerUpdated = null;

            // Limpar referência singleton se este for o instance
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            // Garantir limpeza ao sair da aplicação
            OnQuizCompleted = null;
            OnQuizStarted = null;
            OnTimerUpdated = null;
            _instance = null;
        }

        #endregion
    }
}
