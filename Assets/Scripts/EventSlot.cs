using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Managers;

public class EventSlot : MonoBehaviourPunCallbacks
{
    [SerializeField] private int slotNumber;
    [SerializeField] private int randomNumber;
    [SerializeField] private SoundEffects soundEffects;

    private GameManager gameManager;
    private BackgroundMusic backgroundMusic;
    private Victory victory;

    // Proteção contra clique duplo
    private static bool isProcessingClick = false;

    public int SlotNumber => slotNumber;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        victory = FindFirstObjectByType<Victory>();
        backgroundMusic = FindFirstObjectByType<BackgroundMusic>();
    }

    private void OnDestroy()
    {
    }

    public void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
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

        // Reabilitar cursor se NÃO estiver em transição de turno
        if (!GameManager.IsInTurnTransition)
        {
            InputBlocker.Unblock();
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
            // Verificar se jogador tem SecondChance ativa
            if (BonusCardManager.Instance != null && BonusCardManager.Instance.HasSecondChanceActive)
            {
                DebugHelper.Log("[EventSlot] Segunda chance ativa! Permitindo nova tentativa");

                // Consumir a segunda chance
                BonusCardManager.Instance.ConsumeSecondChance();

                // Tocar som de erro mas permitir nova tentativa
                this.DelayedCall(0.5f, PlayWrongSound);

                // Reativar slots para nova tentativa
                this.DelayedCall(1f, () =>
                {
                    SetUpSlots(true, "Selectable");
                    ResetClickProtection();
                });

                return;
            }

            InputBlocker.Block();

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
                    //card.tag = "Undestructable";
                    card.waitToDistance();
                }
            }

            // Agendar aumento de temperatura apenas no MasterClient
            if (PhotonNetwork.IsMasterClient && ThermometerManager.Instance != null)
            {
                float thermometerProcessingTime = ThermometerManager.Instance.GetErrorProcessingTime();
                this.DelayedCall(5f, () => ThermometerManager.Instance.OnPlayerError());

                // Resetar proteção contra clique duplo após animação do termômetro
                this.DelayedCall(5f + thermometerProcessingTime + 0.5f, ResetClickProtection);
            }
            else
            {
                // Fallback se ThermometerManager não existir
                this.DelayedCall(5.5f, ResetClickProtection);
            }
        }
    }

    private const float CARD_ANIMATION_DELAY = 3.5f;

    /// <summary>
    /// Processa slot correto
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

        DebugHelper.Log($"[EventSlot] targetCard encontrado: {targetCard.name}");

        targetCard.gameObject.GetComponent<Animator>().SetInteger("slotClicked", clickedSlotNumber);
        FinalizeCorrectSlotLocal(cardSlotCount, targetCard, clickedSlotNumber);

        if (PhotonNetwork.IsMasterClient && gameManager != null)
        {
            this.DelayedCall(CARD_ANIMATION_DELAY, () =>
            {
                gameManager.ActivateRandomMapObject(cardSlotCount);
            });
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
        if (slotsFilled == 6 && gameManager != null)
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
            gameManager.SetNewTimelineColliders(false);
            gameManager.Hud.SetActive(false);
        }
        if (backgroundMusic != null)
        {
            backgroundMusic.PlayVictorySound();
        }
    }
}
