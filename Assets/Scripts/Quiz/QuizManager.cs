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

        // Sistema de rotação de quizzes - rastreia quais tipos já foram usados por carta
        // Key: slotCount da carta, Value: lista de tipos de quiz já usados
        private Dictionary<int, List<QuizType>> usedQuizTypesByCard = new Dictionary<int, List<QuizType>>();

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
            DebugHelper.Log($"[QuizManager] StartQuiz chamado - card={card != null}, slotCount={slotCount}");

            if (card == null)
            {
                DebugHelper.Log("[QuizManager] ERRO: card é null!");
                OnQuizCompleted?.Invoke(true);
                return;
            }

            if (card.quizData == null)
            {
                DebugHelper.Log("[QuizManager] ERRO: card.quizData é null!");
                OnQuizCompleted?.Invoke(true);
                return;
            }

            if (!card.quizData.HasQuiz)
            {
                DebugHelper.Log("[QuizManager] Carta sem quiz (HasQuiz=false), completando diretamente");
                OnQuizCompleted?.Invoke(true);
                return;
            }

            currentCard = card;
            currentSlotCount = slotCount;

            // Selecionar tipo de quiz usando o sistema de rotação
            currentQuizType = GetNextAvailableQuizType(card, slotCount);

            if (currentQuizType == QuizType.None)
            {
                DebugHelper.Log("[QuizManager] Nenhum quiz disponível após rotação, completando diretamente");
                OnQuizCompleted?.Invoke(true);
                return;
            }

            // Marcar este tipo como usado para esta carta
            MarkQuizTypeAsUsed(slotCount, currentQuizType);

            DebugHelper.Log($"[QuizManager] Iniciando quiz tipo {currentQuizType} para slot {slotCount} (selecionado com rotação)");
            DebugHelper.Log($"[QuizManager] Enviando RPC_StartQuiz para todos os jogadores...");

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

        /// <summary>
        /// Força o fechamento do quiz sem processar resultado.
        /// Usado quando o tempo do turno expira.
        /// </summary>
        public void ForceCloseQuiz()
        {
            if (!isQuizActive) return;

            DebugHelper.Log("[QuizManager] ForceCloseQuiz - Fechando quiz forçadamente (timeout do turno)");

            isQuizActive = false;
            remainingTime = 0;

            // Esconder UI do quiz
            if (quizUI != null)
            {
                quizUI.ForceHideQuiz();
            }

            // Resetar estado
            currentCard = null;
            currentQuizType = QuizType.None;
        }

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

        #region Quiz Rotation System

        /// <summary>
        /// Obtém o próximo tipo de quiz disponível para uma carta, usando o sistema de rotação.
        /// Exclui tipos já usados até que todos sejam usados, então reseta.
        /// </summary>
        private QuizType GetNextAvailableQuizType(ThemeCard card, int slotCount)
        {
            if (card?.quizData == null) return QuizType.None;

            // Obter todos os tipos de quiz disponíveis para esta carta
            var allAvailableTypes = card.quizData.GetAllAvailableQuizTypes();

            if (allAvailableTypes.Count == 0)
            {
                DebugHelper.Log($"[QuizManager] Carta {slotCount} não tem quizzes disponíveis");
                return QuizType.None;
            }

            // Obter lista de tipos já usados para esta carta
            if (!usedQuizTypesByCard.TryGetValue(slotCount, out var usedTypes))
            {
                usedTypes = new List<QuizType>();
                usedQuizTypesByCard[slotCount] = usedTypes;
            }

            // Filtrar tipos não usados
            var unusedTypes = new List<QuizType>();
            foreach (var type in allAvailableTypes)
            {
                if (!usedTypes.Contains(type))
                {
                    unusedTypes.Add(type);
                }
            }

            DebugHelper.Log($"[QuizManager] Carta {slotCount}: total={allAvailableTypes.Count}, usados={usedTypes.Count}, disponíveis={unusedTypes.Count}");

            // Se todos foram usados, resetar a lista
            if (unusedTypes.Count == 0)
            {
                DebugHelper.Log($"[QuizManager] Todos os quizzes da carta {slotCount} foram usados, resetando rotação");
                usedTypes.Clear();
                unusedTypes.AddRange(allAvailableTypes);
            }

            // Selecionar aleatoriamente entre os não usados
            if (unusedTypes.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, unusedTypes.Count);
                var selectedType = unusedTypes[randomIndex];
                DebugHelper.Log($"[QuizManager] Quiz selecionado para carta {slotCount}: {selectedType}");
                return selectedType;
            }

            return QuizType.None;
        }

        /// <summary>
        /// Marca um tipo de quiz como usado para uma carta específica
        /// </summary>
        private void MarkQuizTypeAsUsed(int slotCount, QuizType quizType)
        {
            if (!usedQuizTypesByCard.TryGetValue(slotCount, out var usedTypes))
            {
                usedTypes = new List<QuizType>();
                usedQuizTypesByCard[slotCount] = usedTypes;
            }

            if (!usedTypes.Contains(quizType))
            {
                usedTypes.Add(quizType);
                DebugHelper.Log($"[QuizManager] Quiz {quizType} marcado como usado para carta {slotCount}. Total usados: {usedTypes.Count}");
            }
        }

        /// <summary>
        /// Reseta os quizzes usados para uma carta específica (útil quando a carta volta ao deck)
        /// </summary>
        public void ResetUsedQuizTypes(int slotCount)
        {
            if (usedQuizTypesByCard.ContainsKey(slotCount))
            {
                usedQuizTypesByCard[slotCount].Clear();
                DebugHelper.Log($"[QuizManager] Quizzes usados resetados para carta {slotCount}");
            }
        }

        /// <summary>
        /// Reseta todos os quizzes usados de todas as cartas
        /// </summary>
        public void ResetAllUsedQuizTypes()
        {
            usedQuizTypesByCard.Clear();
            DebugHelper.Log("[QuizManager] Todos os quizzes usados foram resetados");
        }

        #endregion

        #region RPCs

        [PunRPC]
        public void RPC_StartQuiz(int slotCount, int quizType)
        {
            DebugHelper.Log($"[QuizManager] RPC_StartQuiz RECEBIDO - slotCount={slotCount}, quizType={quizType}");

            currentSlotCount = slotCount;
            currentQuizType = (QuizType)quizType;
            isQuizActive = true;
            remainingTime = quizTimeLimit;

            // Buscar dados da carta no RandomMaterial
            var randomMaterial = FindFirstObjectByType<RandomMaterial>();
            DebugHelper.Log($"[QuizManager] randomMaterial={randomMaterial != null}");

            if (randomMaterial != null)
            {
                var selectedCards = randomMaterial.GetSelectedCards();
                DebugHelper.Log($"[QuizManager] selectedCards={selectedCards != null}, count={selectedCards?.Count ?? 0}");

                if (selectedCards != null)
                {
                    // Encontrar a carta pelo slotCount
                    var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
                    DebugHelper.Log($"[QuizManager] eventCards count={eventCards.Length}");

                    foreach (var eventCard in eventCards)
                    {
                        if (eventCard.slotCount == slotCount)
                        {
                            currentCard = eventCard.GetThemeCard();
                            DebugHelper.Log($"[QuizManager] currentCard encontrado: {currentCard != null}, quizData={currentCard?.quizData != null}");
                            break;
                        }
                    }
                }
            }

            DebugHelper.Log($"[QuizManager] RPC_StartQuiz - Slot: {slotCount}, Tipo: {currentQuizType}, currentCard={currentCard != null}");

            // Notificar UI e listeners
            OnQuizStarted?.Invoke(currentCard, currentQuizType);

            // Mostrar UI do quiz
            DebugHelper.Log($"[QuizManager] quizUI={quizUI != null}");
            if (quizUI != null)
            {
                DebugHelper.Log("[QuizManager] Chamando quizUI.ShowQuiz()...");
                quizUI.ShowQuiz(currentCard, currentQuizType);
            }
            else
            {
                DebugHelper.Log("[QuizManager] ERRO: quizUI é null! Quiz não será exibido.");
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

            // Limpar dicionário de quizzes usados
            usedQuizTypesByCard.Clear();

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
            usedQuizTypesByCard?.Clear();
            _instance = null;
        }

        #endregion
    }
}
