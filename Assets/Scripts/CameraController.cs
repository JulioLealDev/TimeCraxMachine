using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Managers;
public class CameraController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private Animator suitTopAnimator;
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private TimelineColliderArea timelineColliderArea;

    public static bool IsAnimating { get; private set; }

    private Timeline timeline;
    private EventSlot slot;
    private GameManager gameManagerCache;

    /// <summary>
    /// Cacheia referências de cena. Chamado pelo GameManager no Awake.
    /// </summary>
    public void Initialize()
    {
        timeline         = FindFirstObjectByType<Timeline>();
        slot             = FindFirstObjectByType<EventSlot>();
        gameManagerCache = FindFirstObjectByType<GameManager>();
    }

    /// <summary>
    /// Dispara a animação de entrada no menu. Chamado pelo GameManager no Start.
    /// </summary>
    public void EnterMenu()
    {
        cameraAnimator.SetBool("enterMenu", true);
    }

    /// <summary>
    /// Dispara a animação de distancia do menu. Chamado pelo QuitGame.
    /// </summary>
    public void DistanceFromMenu()
    {
        cameraAnimator.SetBool("enterMenu", false);
        cameraAnimator.SetBool("quitGame", true);
    }

    /// <summary>
    /// Animation Event — disparado ao fim do zoom de entrada no menu.
    /// Delega para o MenuManager a habilitação dos colliders das opções do menu.
    /// </summary>
    void EnablingMenuOptions()
    {
        menuManager?.EnablingMenuOptions();
    }

    /// <summary>
    /// Animation Event — disparado ao fim do recuo da câmera da suit.
    /// Ativa o animator do SuitTop e inicia a animação de abertura da suit.
    /// </summary>
    void StartingMatch()
    {
        suitTopAnimator.enabled = true;
        suitTopAnimator.SetBool("openSuit", true);
    }

    /// <summary>
    /// Animation Event — disparado ao voltar da partida para o menu.
    /// </summary>
    public void ExitingMatch()
    {
        cameraAnimator.SetBool("enterMatch", false);
    }

    /// <summary>
    /// Inicia o zoom da câmera na timeline. Chamado via Animation Event do EventCard.
    /// Seta IsAnimating = true para bloquear interações durante a transição.
    /// </summary>
    public void ZoomTimeline()
    {
        IsAnimating = true;
        Debug.Log("[CameraController] ZoomTimeline");
        cameraAnimator.SetBool("distanceZoom", false);
        cameraAnimator.SetBool("zoomTimeline", true);
    }

    /// <summary>
    /// Inicia o recuo da câmera da timeline. Chamado pelo GameManager após fim do PersonsFrame ou timeout.
    /// Seta IsAnimating = true e agenda a reativação do botão de fim de turno após 1.5s.
    /// </summary>
    public void DistanceTimeline()
    {
        IsAnimating = true;
        Debug.Log("[CameraController] DistanceTimeline");
        cameraAnimator.SetBool("zoomTimeline", false);
        cameraAnimator.SetBool("distanceZoom", true);
        this.DelayedCall(1.5f, ActivateEndButton);
    }

    /// <summary>
    /// Delega para o GameManager a reativação do botão de fim de turno.
    /// Chamado por DelayedCall dentro de DistanceTimeline.
    /// </summary>
    public void ActivateEndButton()
    {
        gameManagerCache.ActivateEnd();
    }

    /// <summary>
    /// Animation Event — disparado ao fim do zoom in na timeline.
    /// Libera IsAnimating e ativa os slots se carta foi comprada, ou reativa a timeline se não foi.
    /// </summary>
    void ZoomTimelineFinished()
    {
        IsAnimating = false;
        Debug.Log("[CameraController] ZoomTimelineFinished");
        if (gameManagerCache != null && gameManagerCache.IsMyTurn())
        {
            /*Debug.Log("[CameraController] ZoomTimelineFinished - CheckIfCardWasDrew: " +gameManagerCache.CheckIfCardWasDrew() );
            if (gameManagerCache.CheckIfCardWasDrew())*/
            if (GameStateManager.Is(GamePhase.IM_DrewEventCard))
            {
                if (slot != null) slot.SetUpSlots(true, "Selectable");
                if (timeline != null) timeline.ActiveTimeline(false);
            }
            else
            {
                Debug.Log($"[CameraController] ZoomTimelineFinished else-branch — IsMalfunctionPending={GameManager.IsMalfunctionPending}");
                if (!GameManager.IsMalfunctionPending)
                    if (timeline != null) timeline.ActiveTimeline(true);
            }
        }
    }

    /// <summary>
    /// Animation Event — disparado ao fim do zoom out da timeline.
    /// Libera IsAnimating, reativa colliders da timeline e desativa slots se carta ainda está em jogo.
    /// </summary>
    void DistanceTimelineFinished()
    {
        IsAnimating = false;
        Debug.Log("[CameraController] DistanceTimelineFinished");
        if (GameStateManager.Is(GamePhase.Victory) || GameStateManager.Is(GamePhase.GameOver)) return;
        if (gameManagerCache != null) gameManagerCache.SetNewTimelineNonSlotColliders(true);

        if (gameManagerCache != null && gameManagerCache.IsMyTurn())
        {
            Debug.Log($"[CameraController] DistanceTimelineFinished — IsMalfunctionPending={GameManager.IsMalfunctionPending}");
            if (!GameManager.IsMalfunctionPending)
            {
                if (timeline != null) timeline.ActiveTimeline(true);
                //if (gameManagerCache.CheckIfCardWasDrew() && slot != null)
                else
                {
                    slot.SetUpSlots(false, "Undestructable");
                    timelineColliderArea.SetUpTimelineCollider(true);
                }
            }
        }
        gameManagerCache?.ProcessPendingPlayerError();
        Debug.Log("[CameraController] GameState: " +GamePhase.IM_Turn );
        GameStateManager.TransitionTo(GamePhase.IM_Turn);
    }

    /// <summary>
    /// Reseta a câmera para o estado inicial sem animação.
    /// Usado quando o turno expira por timeout.
    /// </summary>
    public void ForceResetToInitialState()
    {
        IsAnimating = false;
        cameraAnimator.SetBool("zoomTimeline", false);
        cameraAnimator.SetBool("distanceZoom", false);

        if (slot != null) slot.SetUpSlots(false, "Undestructable");
        if (timeline != null) timeline.ActiveTimeline(false);
    }

    /// <summary>
    /// Retorna true se a câmera está atualmente com zoom na timeline.
    /// </summary>
    public bool IsInZoomMode()
    {
        return cameraAnimator.GetBool("zoomTimeline");
    }

    /// <summary>
    /// Chamado por GameManager.AddMalfunctionInComponent() ao fim da roleta.
    /// Limpa o flag de malfunction e reativa o collider da timeline se for o turno do jogador local.
    /// </summary>
    public void ActivateTimelineAfterMalfunction()
    {
        Debug.Log("[CameraController] ActivateTimelineAfterMalfunction — limpando IsMalfunctionPending");
        GameManager.IsMalfunctionPending = false;
        // Não reativar se o turno já está finalizando (End Turn clicado)
        if (GameManager.IsInTurnTransition) return;
        if (gameManagerCache != null && gameManagerCache.IsMyTurn())
        {
            if (timeline != null) timeline.ActiveTimeline(true);
            else
            {
                slot?.SetUpSlots(false, "Undestructable");
                timelineColliderArea?.SetUpTimelineCollider(true);
            }
        }
    }
}
