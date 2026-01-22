using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Quiz;

public class EventSlot : MonoBehaviourPunCallbacks
{
    [SerializeField] private int slotNumber;
    [SerializeField] private int randomNumber;
    [SerializeField] private SoundEffects soundEffects;

    private GameManager gameManager;
    private BackgroundMusic backgroundMusic;
    private Victory victory;
    private QuizManager quizManager;

    // Estado do quiz pendente
    private int pendingQuizSlotCount = -1;
    private EventCard pendingQuizCard;

    // Proteção contra clique duplo
    private static bool isProcessingClick = false;

    public int SlotNumber => slotNumber;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        victory = FindFirstObjectByType<Victory>();
        backgroundMusic = FindFirstObjectByType<BackgroundMusic>();
        quizManager = QuizManager.Instance;

        // Inscrever no evento de quiz completado
        if (quizManager != null)
        {
            quizManager.OnQuizCompleted += OnQuizCompleted;
        }
    }

    private void OnDestroy()
    {
        if (quizManager != null)
        {
            quizManager.OnQuizCompleted -= OnQuizCompleted;
        }
    }

    public void OnMouseDown()
    {
        // Bloquear clique durante animações de câmera
        if (CameraController.IsAnimating) return;

        // Proteção contra clique duplo
        if (isProcessingClick) return;

        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        foreach (var card in eventCards)
        {
            if (card != null && card.CompareTag("Drew"))
            {
                isProcessingClick = true;
                // Enviar requisição ao MasterClient para processar o clique no slot
                photonView.RPC("RequestSlotClick", RpcTarget.MasterClient, card.slotCount, slotNumber);
                break; // Só processa uma carta
            }
        }
    }

    /// <summary>
    /// Reseta a proteção contra clique duplo (chamado após ação completar)
    /// </summary>
    public static void ResetClickProtection()
    {
        isProcessingClick = false;
    }

    /// <summary>
    /// RPC enviado ao MasterClient para processar clique no slot
    /// </summary>
    [PunRPC]
    public void RequestSlotClick(int cardSlotCount, int clickedSlotNumber)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            bool isCorrectSlot = clickedSlotNumber == cardSlotCount;
            DebugHelper.Log($"[EventSlot] MasterClient processando clique: cardSlot={cardSlotCount}, clickedSlot={clickedSlotNumber}, correct={isCorrectSlot}");

            // Sincronizar para todos
            photonView.RPC("ExecuteSlotClick", RpcTarget.All, cardSlotCount, clickedSlotNumber, isCorrectSlot);
        }
    }

    /// <summary>
    /// RPC executado em todos para processar clique no slot
    /// </summary>
    [PunRPC]
    public void ExecuteSlotClick(int cardSlotCount, int clickedSlotNumber, bool isCorrectSlot)
    {
        DebugHelper.Log($"[EventSlot] ExecuteSlotClick: cardSlot={cardSlotCount}, clickedSlot={clickedSlotNumber}, correct={isCorrectSlot}");

        // Som de clique no slot
        soundEffects.PlayClickSlotSound();

        // Desativar slots
        SetUpSlots(false, "Undestructable");

        if (isCorrectSlot)
        {
            // Slot correto - som de acerto após delay
            this.DelayedCall(3.3f, PlayRightSound);

            // Processar slot correto
            ProcessRightSlot(cardSlotCount, clickedSlotNumber);
        }
        else
        {
            // Slot errado - som de erro após delay
            this.DelayedCall(3.3f, PlayWrongSound);

            // Animar carta para slot errado
            var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
            foreach (var card in cards)
            {
                if (card.slotCount == cardSlotCount)
                {
                    card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", clickedSlotNumber);
                    card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
                    card.tag = "Undestructable";
                    card.waitToDistance();
                }
            }

            // Agendar malfunction apenas no MasterClient
            if (PhotonNetwork.IsMasterClient)
            {
                this.DelayedCall(5f, RandomComponent);
            }

            // Resetar proteção contra clique duplo após animação
            this.DelayedCall(5.5f, ResetClickProtection);
        }
    }

    // Tempo para aguardar a animação da carta antes de mostrar o quiz
    private const float CARD_ANIMATION_DELAY = 3.5f;

    /// <summary>
    /// Processa slot correto (pode ter quiz)
    /// </summary>
    private void ProcessRightSlot(int cardSlotCount, int clickedSlotNumber)
    {
        DebugHelper.Log($"[EventSlot] ProcessRightSlot chamado - cardSlotCount={cardSlotCount}, clickedSlotNumber={clickedSlotNumber}");

        var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        EventCard targetCard = null;

        foreach (var card in cards)
        {
            if (card.slotCount == cardSlotCount)
            {
                targetCard = card;
                break;
            }
        }

        if (targetCard == null)
        {
            DebugHelper.Log($"[EventSlot] targetCard é NULL para cardSlotCount={cardSlotCount}");
            return;
        }

        DebugHelper.Log($"[EventSlot] targetCard encontrado: {targetCard.name}, quizManager={quizManager != null}");

        // Animar a carta para o slot
        targetCard.gameObject.GetComponent<Animator>().SetInteger("slotClicked", clickedSlotNumber);

        // Verificar se a carta tem quiz
        bool hasQuiz = targetCard.HasQuiz();
        DebugHelper.Log($"[EventSlot] hasQuiz={hasQuiz}");

        if (hasQuiz && quizManager != null)
        {
            DebugHelper.Log("[EventSlot] Carta tem quiz! Aguardando animação...");

            // Guardar estado pendente para quando o quiz terminar
            pendingQuizSlotCount = cardSlotCount;
            pendingQuizCard = targetCard;
            pendingQuizClickedSlotNumber = clickedSlotNumber;

            // Aguardar a animação da carta terminar antes de iniciar o quiz
            // Apenas MasterClient inicia o quiz (vai sincronizar via RPC)
            if (PhotonNetwork.IsMasterClient)
            {
                this.DelayedCall(CARD_ANIMATION_DELAY, () =>
                {
                    DebugHelper.Log("[EventSlot] Animação terminou, iniciando quiz...");
                    var themeCard = targetCard.GetThemeCard();
                    quizManager.StartQuiz(themeCard, cardSlotCount);
                });
            }
        }
        else
        {
            DebugHelper.Log($"[EventSlot] Sem quiz ou quizManager null - finalizando slot diretamente");
            // Sem quiz - fluxo original
            FinalizeCorrectSlotLocal(cardSlotCount, targetCard, clickedSlotNumber);
        }
    }

    /// <summary>
    /// Finaliza slot correto localmente (já sincronizado)
    /// </summary>
    private void FinalizeCorrectSlotLocal(int slotCount, EventCard card, int clickedSlotNumber)
    {
        // Encontrar o slot que foi clicado para desativá-lo
        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            if (slot.slotNumber == clickedSlotNumber)
            {
                slot.gameObject.tag = "Disabled";
                break;
            }
        }

        var deckEvent = FindFirstObjectByType<DeckEvent>();
        // Apenas MasterClient remove do deck (ele vai sincronizar via RPC)
        if (PhotonNetwork.IsMasterClient)
        {
            deckEvent.RemoveIndex(slotCount);
        }

        card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", clickedSlotNumber);
        card.tag = "Disabled";
        card.waitToDistance();

        // Resetar proteção contra clique duplo
        ResetClickProtection();

        CheckIfWin();
    }

    public void PlayRightSound()
    {
        soundEffects.PlayRightSlotSound();
    }

    public void PlayWrongSound()
    {
        soundEffects.PlayWrongSlotSound();
    }

    public void RandomComponent()
    {
        if (gameManager != null)
        {
            gameManager.RandomComponentNumber();
        }
    }

    // Armazena o slotNumber do slot que foi clicado para quiz
    private int pendingQuizClickedSlotNumber = -1;

    /// <summary>
    /// Callback quando o quiz é completado
    /// </summary>
    private void OnQuizCompleted(bool correct)
    {
        if (pendingQuizSlotCount < 0 || pendingQuizCard == null) return;

        if (correct)
        {
            DebugHelper.Log($"[EventSlot] Quiz correto! Finalizando slot {pendingQuizSlotCount}");
            FinalizeCorrectSlotLocal(pendingQuizSlotCount, pendingQuizCard, pendingQuizClickedSlotNumber);
        }
        else
        {
            DebugHelper.Log($"[EventSlot] Quiz errado! Carta volta ao deck");
            // Quiz falhou - carta volta ao deck
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("QuizFailed", RpcTarget.All, pendingQuizSlotCount);
            }
        }

        // Limpar estado pendente
        pendingQuizSlotCount = -1;
        pendingQuizCard = null;
        pendingQuizClickedSlotNumber = -1;
    }

    [PunRPC]
    public void QuizFailed(int slotCount)
    {
        DebugHelper.Log($"[EventSlot] RPC QuizFailed - slotCount: {slotCount}");

        // Encontrar a carta
        var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in cards)
        {
            if (card.slotCount == slotCount)
            {
                // Animar erro
                card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
                card.tag = "Undestructable";
                card.waitToDistance();

                // Resetar estado da carta após animação para poder ser comprada novamente
                this.DelayedCall(3.5f, () =>
                {
                    card.ResetStatusCard();
                });
                break;
            }
        }

        // Adicionar carta de volta ao deck
        var deckEvent = FindFirstObjectByType<DeckEvent>();
        if (deckEvent != null)
        {
            deckEvent.AddCardBack(slotCount);
        }

        // Tocar som de erro
        soundEffects.PlayWrongSlotSound();

        // Sortear componente para malfunction após câmera voltar (3.3s animação + 1.5s zoom out + margem)
        if (PhotonNetwork.IsMasterClient)
        {
            this.DelayedCall(5f, RandomComponent);
        }

        // Resetar proteção contra clique duplo após animação
        this.DelayedCall(5.5f, ResetClickProtection);
    }

    /// <summary>
    /// Verifica se é o turno do jogador local
    /// </summary>
    private bool IsMyTurn()
    {
        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player != null && player.photonView != null && player.photonView.IsMine && player.GetYourTurn())
            {
                return true;
            }
        }
        return false;
    }

    public void SetUpSlots(bool activateSlot, string tag)
    {
        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            if (!slot.CompareTag("Disabled"))
            {
                slot.tag = tag;
                slot.GetComponentInChildren<MeshCollider>().enabled = activateSlot;
            }
        }
    }

    public void CheckIfWin()
    {
        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        int slotsFilled = 0;
        foreach (var slot in slots)
        {
            if (slot != null && slot.CompareTag("Disabled"))
            {
                slotsFilled++;
            }
        }
        if (slotsFilled == 7 && gameManager != null)
        {
            gameManager.DeactivateAll();
            gameManager.ResetAllComponents();
            gameManager.ResetAllPlatenames();
            this.DelayedCall(5.5f, Victory);
        }
    }

    public void Victory()
    {
        if (victory != null)
        {
            victory.transform.GetChild(0).gameObject.SetActive(true);
        }
        if (gameManager != null)
        {
            gameManager.hud.SetActive(false);
        }
        if (backgroundMusic != null)
        {
            backgroundMusic.PlayVictorySound();
        }
    }
}
