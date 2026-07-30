using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Managers;

public class EventSlot : MonoBehaviourPunCallbacks
{
    [SerializeField] private int slotNumber;
    [SerializeField] private int randomNumber;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private EndMatch endMatchScreen;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BackgroundMusic backgroundMusic;

    public int SlotNumber => slotNumber;

    private EventSlot[] cachedSlots;
    private DeckEvent cachedDeckEvent;
    [System.NonSerialized] public MeshCollider cachedMeshCollider;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        endMatchScreen = FindFirstObjectByType<EndMatch>();
        backgroundMusic = FindFirstObjectByType<BackgroundMusic>();
        cachedSlots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        cachedDeckEvent = FindFirstObjectByType<DeckEvent>();
        cachedMeshCollider = GetComponentInChildren<MeshCollider>();
    }

    private void OnDestroy()
    {
    }

    public void OnMouseDown()
    {
        Debug.Log("[EventSlot] OnMouseDown");
        if (InputBlocker.IsBlocked) return;
        // Bloquear clique durante animações de câmera
        if (CameraController.IsAnimating) return;

        if (GameManager.IsClickProcessing(typeof(EventSlot))) return;

        GameStateManager.TransitionTo(GamePhase.IM_CheckingSlot);

        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        foreach (var card in eventCards)
        {
            if (card != null && card.CompareTag("Drew"))
            {
                if (card.GetThemeCard() == null)
                {
                    Debug.LogWarning($"[EventSlot] EventCard slotCount={card.slotCount} sem ThemeCard — clique ignorado.");
                    break;
                }
                GameManager.TryBeginClick(typeof(EventSlot));
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
        GameManager.ResetClick(typeof(EventSlot));

        if (!GameManager.IsInTurnTransition && !GameManager.IsMalfunctionPending)
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
        Debug.Log($"[EventSlot] RequestSlotClick recebido no MasterClient — card={cardSlotCount}, slot={clickedSlotNumber}, isMaster={PhotonNetwork.IsMasterClient}");
        if (PhotonNetwork.IsMasterClient)
        {
            bool isCorrectSlot = clickedSlotNumber == cardSlotCount;
            Debug.Log($"[EventSlot] isCorrectSlot={isCorrectSlot} → enviando ExecuteSlotClick para todos");
            photonView.RPC("ExecuteSlotClick", RpcTarget.All, cardSlotCount, clickedSlotNumber, isCorrectSlot);
        }
    }

    /// <summary>
    /// RPC executado em todos para processar clique no slot
    /// </summary>
    [PunRPC]
    public void ExecuteSlotClick(int cardSlotCount, int clickedSlotNumber, bool isCorrectSlot)
    {
        Debug.Log($"[EventSlot] ExecuteSlotClick — card={cardSlotCount}, slot={clickedSlotNumber}, correto={isCorrectSlot}");

        // Som de clique no slot
        soundEffects.PlayClickSlotSound();

        // Desativar slots
        SetUpSlots(false, "Undestructable");

        if (isCorrectSlot)
        {
            GameManager.CurrentCheckingSlotCount = cardSlotCount;

            var p = PlayerManager.Instance?.GetCurrentTurnPlayer();
            if (p != null) MatchStats.AddSlotCorrect(p.actorNumber, p.nickname);

            this.DelayedCall(3.3f, PlayRightSound);
            ProcessRightSlot(cardSlotCount, clickedSlotNumber);
        }
        else
        {
            // Verificar se jogador tem SecondChance ativa
            if (BonusCardManager.Instance != null && BonusCardManager.Instance.HasSecondChanceActive)
            {

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

            var p = PlayerManager.Instance?.GetCurrentTurnPlayer();
            if (p != null) MatchStats.AddSlotError(p.actorNumber, p.nickname);

            InputBlocker.Block();

            // Slot errado - som de erro após delay
            this.DelayedCall(3.3f, PlayWrongSound);

            // Animar carta para slot errado
            var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
            foreach (var card in cards)
            {
                if (card.slotCount == cardSlotCount)
                {
                    card.CardAnimator.SetInteger("slotClicked", clickedSlotNumber);
                    card.CardAnimator.SetBool("wrongSlot", true);
                    card.tag = "Undestructable";
                    card.waitToDistance();
                }
            }

            // Capturar flag de malfunction antes que OnPlayerError() altere a temperatura
            if (ThermometerManager.Instance != null && ThermometerManager.Instance.WillNextErrorCauseMalfunction())
                GameManager.IsMalfunctionPending = true;

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
        Debug.Log($"[EventSlot] ProcessRightSlot — card={cardSlotCount}, slot={clickedSlotNumber}");

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
            Debug.LogWarning($"[EventSlot] ProcessRightSlot — EventCard com slotCount={cardSlotCount} não encontrada!");
            return;
        }

        Debug.Log($"[EventSlot] EventCard encontrada: {targetCard.name}, themeCard={targetCard.GetThemeCard()?.title}");

        targetCard.CardAnimator.SetInteger("slotClicked", clickedSlotNumber);
        FinalizeCorrectSlotLocal(cardSlotCount, targetCard, clickedSlotNumber);

        if (PhotonNetwork.IsMasterClient && gameManager != null)
        {
            Debug.Log($"[EventSlot] MasterClient agendando ActivateRandomMapObject em {CARD_ANIMATION_DELAY}s");
            this.DelayedCall(CARD_ANIMATION_DELAY, () =>
            {
                GameManager.CurrentCheckingSlotCount = -1;
                if (GameManager.IsInTurnTransition) return;
                Debug.Log($"[EventSlot] Chamando ActivateRandomMapObject para slotCount={cardSlotCount}");
                gameManager.ActivateRandomMapObject(cardSlotCount);
            });
        }
        else
        {
            Debug.Log($"[EventSlot] ProcessRightSlot — não é MasterClient ou gameManager é null (isMaster={PhotonNetwork.IsMasterClient}, gm={gameManager != null})");
        }
    }

    /// <summary>
    /// Finaliza slot correto localmente (já sincronizado)
    /// </summary>
    private void FinalizeCorrectSlotLocal(int slotCount, EventCard card, int clickedSlotNumber)
    {
        Debug.Log($"[EventSlot] FinalizeCorrectSlotLocal — slotCount={slotCount}, slot={clickedSlotNumber}");

        foreach (var slot in cachedSlots)
        {
            if (slot != null && slot.slotNumber == clickedSlotNumber)
            {
                slot.gameObject.tag = "Disabled";
                break;
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"[EventSlot] MasterClient removendo slotCount={slotCount} do deck");
            cachedDeckEvent.RemoveIndex(slotCount);
        }

        card.CardAnimator.SetInteger("slotClicked", clickedSlotNumber);
        card.tag = "Disabled";

        ResetClickProtection();
        Debug.Log("[EventSlot] ResetClickProtection chamado — InputBlocker desbloqueado");
    }

    public void PlayRightSound()
    {
        soundEffects.PlayRightSlotSound();
    }

    public void PlayWrongSound()
    {
        soundEffects.PlayWrongSlotSound();
    }

    public void SetUpSlots(bool activateSlot, string tag)
    {
        foreach (var slot in cachedSlots)
        {
            if (slot != null && !slot.CompareTag("Disabled"))
            {
                slot.tag = tag;
                slot.cachedMeshCollider.enabled = activateSlot;
            }
        }
    }

    public void CheckIfWin()
    {
        int slotsFilled = 0;
        foreach (var slot in cachedSlots)
        {
            if (slot != null && slot.CompareTag("Disabled"))
            {
                slotsFilled++;
            }
        }
        if (slotsFilled == 6 && gameManager != null)
        {
            MatchStats.StopTimer();
            GameStateManager.TransitionTo(GamePhase.Victory);
            gameManager.DeactivateAll();
            gameManager.ResetAllComponents();
            gameManager.ResetAllPlatenames();
            this.DelayedCall(2.5f, ShowVictoryScreen);
        }
    }

    public void ShowVictoryScreen()
    {
        if (endMatchScreen != null)
        {
            endMatchScreen.transform.GetChild(0).gameObject.SetActive(true);
            endMatchScreen.UpdateTitle();
            gameManager.ResetAllSlotLinks();
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
