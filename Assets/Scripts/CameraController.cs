using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class CameraController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject suitTop;
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private bool fullScreen = true;

    // Indica se a câmera está em animação (bloqueia interações)
    public static bool IsAnimating { get; private set; }

    // Cached components
    private Timeline timeline;
    private EventSlot slot;
    private Animator suitTopAnimator;
    private Menu menuCache;
    private GameManager gameManagerCache;

    private void Awake()
    {
        SessionData.GameStarted = false;
        gameConnection.EnterServerAndLobby();
        timeline = FindFirstObjectByType<Timeline>();
        slot = FindFirstObjectByType<EventSlot>();
        menuCache = FindFirstObjectByType<Menu>();
        gameManagerCache = FindFirstObjectByType<GameManager>();

        if (suitTop != null)
        {
            suitTopAnimator = suitTop.GetComponent<Animator>();
        }
    }

    void Start()
    {
        int targetHeight = Screen.width * 9 / 16;
        Screen.SetResolution(Screen.width, targetHeight, fullScreen);
        animator.SetBool("enterMenu", true);
    }

    void AwaitZoomAnimation()
    {
        Transform menuOptions = suitTop.transform.Find("MenuOptions");
        if (menuOptions == null) return;
        foreach (Transform child in menuOptions.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("Selectable"))
            {
                var col = child.GetComponent<MeshCollider>();
                if (col != null) col.enabled = true;
            }
        }
    }

    void AwaitDistanceCamera()
    {
        suitTopAnimator.enabled = true;
        suitTopAnimator.SetBool("openSuit", true);
    }

    public void ZoomTimeline()
    {
        IsAnimating = true;
        animator.SetBool("distanceZoom", false);
        animator.SetBool("zoomTimeline", true);
    }

    public void DistanceTimeline()
    {
        IsAnimating = true;
        animator.SetBool("zoomTimeline", false);
        animator.SetBool("distanceZoom", true);
        this.DelayedCall(1.5f, ActivateEndButton);
        // Segurança: garantir que IsAnimating seja resetado mesmo se o animation event falhar
        this.DelayedCall(3f, () => { if (IsAnimating) { DebugHelper.Log("[CameraController] SAFETY: forçando IsAnimating=false"); IsAnimating = false; } });
    }

    public void ActivateEndButton()
    {
        gameManagerCache.ActivateEnd();
    }

    void AwaitZoomTimeline()
    {
        IsAnimating = false;
        if (IsMyTurn())
        {
            if (CheckIfCardWasDrew())
            {
                if (slot != null) slot.SetUpSlots(true, "Selectable");
                if (timeline != null) timeline.ActiveTimeline(false);
            }
            else
            {
                if (timeline != null) timeline.ActiveTimeline(true);
            }
        }
    }

    void AwaitDistanceTimeline()
    {
        DebugHelper.Log("[CameraController] AwaitDistanceTimeline disparado");
        IsAnimating = false;
        // Reativar coliders não-slot ao retornar ao zoom normal
        if (gameManagerCache != null) gameManagerCache.SetNewTimelineNonSlotColliders(true);

        if (IsMyTurn())
        {
            if (timeline != null) timeline.ActiveTimeline(true);
            if (CheckIfCardWasDrew() && slot != null)
            {
                slot.SetUpSlots(false, "Undestructable");
            }
        }
    }

    /// <summary>
    /// Força o reset da câmera para o estado inicial.
    /// Usado quando o tempo do turno expira.
    /// </summary>
    public void ForceResetToInitialState()
    {
        DebugHelper.Log("[CameraController] ForceResetToInitialState");

        // Resetar flags de animação
        IsAnimating = false;

        // Garantir que a câmera está no estado de distância
        animator.SetBool("zoomTimeline", false);
        animator.SetBool("distanceZoom", false);

        // Desativar slots
        if (slot != null)
        {
            slot.SetUpSlots(false, "Undestructable");
        }

        // Desativar timeline
        if (timeline != null)
        {
            timeline.ActiveTimeline(false);
        }
    }

    /// <summary>
    /// Verifica se a câmera está em modo zoom timeline
    /// </summary>
    public bool IsInZoomMode()
    {
        return animator.GetBool("zoomTimeline");
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

    bool CheckIfCardWasDrew()
    {
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in eventCards)
        {
            if (card.CompareTag("Drew"))
            {
                return true;
            }
        }
        return false;
    }
}
