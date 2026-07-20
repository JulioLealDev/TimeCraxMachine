using UnityEngine;
using Photon.Pun;
using System;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Themes;

public class EventCard : MonoBehaviourPunCallbacks
{
    [Header("Referências")]
    [SerializeField] private CameraController cameraController;

    [Header("Dados da Carta")]
    public int slotCount;  // Público - usado em RPCs e acesso externo
    public int slotYear;   // Público - usado em RPCs e acesso externo
    public string era;     // Público - usado em RPCs e acesso externo

    // Referência à carta do tema (novo sistema)
    private ThemeCard themeCard;

    // Cache de componentes
    private Animator cachedAnimator;
    private MeshRenderer cachedRenderer;
    private EventCardFollower cachedFollower;
    private TextMeshPro cachedCardText;
    private GameManager cachedGameManager;

    public Animator CardAnimator => cachedAnimator;

    void Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
        cachedAnimator = GetComponent<Animator>();
        cachedRenderer = GetComponent<MeshRenderer>();
        cachedFollower = GetComponent<EventCardFollower>();
        cachedCardText = GetComponentInChildren<TextMeshPro>();
        cachedGameManager = FindFirstObjectByType<GameManager>();
    }

    /// <summary>
    /// Executa a compra localmente (usado quando já está dentro de um RPC sincronizado)
    /// </summary>
    public void DrawEventCardLocal()
    {
        DrawingEventCardInternal();
    }

    [PunRPC]
    public void DrawingEventCard(PhotonMessageInfo info)
    {
        if (info.Sender != photonView.Owner) return;
        DrawingEventCardInternal();
    }

    private void DrawingEventCardInternal()
    {
        // Desancorar do slot caso o Animator tenha ficado preso em to_slotN
        if (cachedFollower != null) cachedFollower.Detach();

        cachedRenderer.enabled = true;

        // Ativar MeshRenderer do CardText
        if (cachedCardText != null)
            cachedCardText.GetComponent<MeshRenderer>().enabled = true;

        gameObject.tag = "Drew";

        // Forçar o Animator de volta ao estado idle antes de disparar a animação de compra.
        // Sem isso, se o Animator ficou preso em draw_eventCard_to_slotN (por race condition
        // em HandlePersonsWrong/HandleMapWrong), o drawingEventCard=true não dispara nenhuma
        // transição válida e a carta aparece diretamente no slot sem zoom, congelando o jogo.
        cachedAnimator.SetBool("wrongSlot", false);
        cachedAnimator.SetInteger("slotClicked", 0);
        cachedAnimator.Play("draw_eventCard_idle", 0, 0);
        cachedAnimator.SetBool("drawingEventCard", true);

        // Desativar coliders não-slot assim que a carta é comprada
        if (cachedGameManager != null) cachedGameManager.SetNewTimelineNonSlotColliders(false);
    }

    public void ZoomTimeline()
    {
        cameraController.ZoomTimeline();
    }

    public void waitToDistance()
    {
        this.DelayedCall(3.3f, DistanceTimeline);
    }

    public void DistanceTimeline()
    {
        // Re-habilitar coliders não-slot ao iniciar zoom-out (antes de AwaitDistanceTimeline)
        if (cachedGameManager != null) cachedGameManager.SetNewTimelineNonSlotColliders(true);

        cameraController.DistanceTimeline();
    }

    /// <summary>
    /// Chamado via Animation Event no último frame de draw_eventCard_to_slot0X.
    /// Faz a carta seguir o slot onde foi fixada.
    /// </summary>
    public void OnFixedToSlot()
    {
        int slotNumber = cachedAnimator.GetInteger("slotClicked");
        if (slotNumber <= 0) return;

        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            if (slot.SlotNumber == slotNumber)
            {
                if (cachedFollower != null)
                    cachedFollower.AttachToSlot(slot.transform);
                break;
            }
        }
    }

    public void ResetStatusCard()
    {
        if (cachedFollower != null) cachedFollower.Detach();

        cachedRenderer.enabled = false;

        // Desativar MeshRenderer do CardText
        if (cachedCardText != null)
            cachedCardText.GetComponent<MeshRenderer>().enabled = false;

        cachedAnimator.SetBool("wrongSlot", false);
        cachedAnimator.SetBool("drawingEventCard", false);
        cachedAnimator.SetInteger("slotClicked", 0);
    }

    public void ActivateEndButton()
    {
        cachedGameManager.ActivateEnd();
    }

    #region Theme System

    /// <summary>
    /// Define a carta do tema associada a este EventCard
    /// </summary>
    public void SetThemeCard(ThemeCard card)
    {
        themeCard = card;

        // Definir era
        era = card.era;

        // Definir título no CardText
        if (cachedCardText != null)
            cachedCardText.text = card.title;
    }

    /// <summary>
    /// Retorna a carta do tema associada
    /// </summary>
    public ThemeCard GetThemeCard()
    {
        return themeCard;
    }

    #endregion
}
