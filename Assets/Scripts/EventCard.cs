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

    // Start is called before the first frame update
    void Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
    }

    /// <summary>
    /// Chama RPC para comprar carta (usado quando precisa sincronizar)
    /// </summary>
    public void DrawEventCard()
    {
        photonView.RPC("DrawingEventCard", RpcTarget.All);
    }

    /// <summary>
    /// Executa a compra localmente (usado quando já está dentro de um RPC sincronizado)
    /// </summary>
    public void DrawEventCardLocal()
    {
        DrawingEventCardInternal();
    }

    [PunRPC]
    public void DrawingEventCard()
    {
        DrawingEventCardInternal();
    }

    private void DrawingEventCardInternal()
    {
        // Desancorar do slot caso o Animator tenha ficado preso em to_slotN
        var follower = GetComponent<EventCardFollower>();
        if (follower != null) follower.Detach();

        gameObject.GetComponent<MeshRenderer>().enabled = true;

        // Ativar MeshRenderer do CardText
        var cardText = GetComponentInChildren<TextMeshPro>();
        if (cardText != null)
        {
            cardText.GetComponent<MeshRenderer>().enabled = true;
        }

        gameObject.tag = "Drew";

        // Forçar o Animator de volta ao estado idle antes de disparar a animação de compra.
        // Sem isso, se o Animator ficou preso em draw_eventCard_to_slotN (por race condition
        // em HandlePersonsWrong/HandleMapWrong), o drawingEventCard=true não dispara nenhuma
        // transição válida e a carta aparece diretamente no slot sem zoom, congelando o jogo.
        var animator = gameObject.GetComponent<Animator>();
        animator.SetBool("wrongSlot", false);
        animator.SetInteger("slotClicked", 0);
        animator.Play("draw_eventCard_idle", 0, 0);
        animator.SetBool("drawingEventCard", true);

        // Desativar coliders não-slot assim que a carta é comprada
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null) gm.SetNewTimelineNonSlotColliders(false);
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
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null) gm.SetNewTimelineNonSlotColliders(true);

        cameraController.DistanceTimeline();
    }

    /// <summary>
    /// Chamado via Animation Event no último frame de draw_eventCard_to_slot0X.
    /// Faz a carta seguir o slot onde foi fixada.
    /// </summary>
    public void OnFixedToSlot()
    {
        int slotNumber = GetComponent<Animator>().GetInteger("slotClicked");
        if (slotNumber <= 0) return;

        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            if (slot.SlotNumber == slotNumber)
            {
                var follower = GetComponent<EventCardFollower>();
                if (follower != null)
                    follower.AttachToSlot(slot.transform);
                break;
            }
        }
    }

    public void ResetStatusCard()
    {
        var follower = GetComponent<EventCardFollower>();
        if (follower != null) follower.Detach();

        gameObject.GetComponent<MeshRenderer>().enabled = false;

        // Desativar MeshRenderer do CardText
        var cardText = GetComponentInChildren<TextMeshPro>();
        if (cardText != null)
        {
            cardText.GetComponent<MeshRenderer>().enabled = false;
        }

        gameObject.GetComponent<Animator>().SetBool("wrongSlot", false);
        gameObject.GetComponent<Animator>().SetBool("drawingEventCard", false);
        gameObject.GetComponent<Animator>().SetInteger("slotClicked", 0);
    }

    public void ActivateEndButton()
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager.ActivateEnd();
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
        var cardText = GetComponentInChildren<TextMeshPro>();
        if (cardText != null)
        {
            cardText.text = card.title;
        }
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
