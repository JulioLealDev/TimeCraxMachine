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
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        foreach (var card in eventCards)
        {
            if (card.CompareTag("Drew"))
            {
                // Enviar requisição ao MasterClient para processar o clique no slot
                photonView.RPC("RequestSlotClick", RpcTarget.MasterClient, card.slotCount, slotNumber);
                break; // Só processa uma carta
            }
        }
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
        }
    }

    /// <summary>
    /// Processa slot correto (pode ter quiz)
    /// </summary>
    private void ProcessRightSlot(int cardSlotCount, int clickedSlotNumber)
    {
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

        if (targetCard == null) return;

        // Verificar se a carta tem quiz
        if (targetCard.HasQuiz() && quizManager != null)
        {
            // Guardar estado pendente para quando o quiz terminar
            pendingQuizSlotCount = cardSlotCount;
            pendingQuizCard = targetCard;
            pendingQuizClickedSlotNumber = clickedSlotNumber;

            // Animar a carta para o slot (mas ainda não confirma)
            targetCard.gameObject.GetComponent<Animator>().SetInteger("slotClicked", clickedSlotNumber);

            // Iniciar quiz apenas no MasterClient
            if (PhotonNetwork.IsMasterClient)
            {
                var themeCard = targetCard.GetThemeCard();
                quizManager.StartQuiz(themeCard, cardSlotCount);
            }
        }
        else
        {
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
        gameManager.RandomComponentNumber();
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

        // Reativar slots para nova tentativa
        this.DelayedCall(3.5f, () =>
        {
            SetUpSlots(true, "Selectable");
        });

        // Tocar som de erro
        soundEffects.PlayWrongSlotSound();
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
            if (slot.CompareTag("Disabled"))
            {
                slotsFilled++;
            }
        }
        if (slotsFilled == 7)
        {
            gameManager.DeactivateAll();
            gameManager.ResetAllComponents();
            gameManager.ResetAllPlatenames();
            this.DelayedCall(5.5f, Victory);
        }
    }

    public void Victory()
    {
        victory.transform.GetChild(0).gameObject.SetActive(true);
        gameManager.hud.SetActive(false);
        backgroundMusic.PlayVictorySound();
    }
}
