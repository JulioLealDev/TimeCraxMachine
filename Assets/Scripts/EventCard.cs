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
        gameObject.GetComponent<MeshRenderer>().enabled = true;

        // Ativar MeshRenderer do CardText
        var cardText = GetComponentInChildren<TextMeshPro>();
        if (cardText != null)
        {
            cardText.GetComponent<MeshRenderer>().enabled = true;
        }

        gameObject.tag = "Drew";
        gameObject.GetComponent<Animator>().SetBool("drawingEventCard", true);
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
        cameraController.DistanceTimeline();
    }

    public void ResetStatusCard()
    {
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
        DebugHelper.Log("ActivateEndButton");
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

    /// <summary>
    /// Verifica se esta carta tem quiz associado
    /// </summary>
    public bool HasQuiz()
    {
        bool hasThemeCard = themeCard != null;
        bool hasQuizData = themeCard?.quizData != null;
        bool hasQuiz = themeCard?.quizData?.HasQuiz ?? false;

        DebugHelper.Log($"[EventCard.HasQuiz] slotCount={slotCount}, hasThemeCard={hasThemeCard}, hasQuizData={hasQuizData}, hasQuiz={hasQuiz}");

        if (hasQuizData)
        {
            var qd = themeCard.quizData;
            DebugHelper.Log($"[EventCard.HasQuiz] imageQuiz={qd.imageQuiz != null}, textQuiz={qd.textQuiz != null}, trueFalseQuiz={qd.trueFalseQuiz != null}, correlationQuiz={qd.correlationQuiz != null}");
        }

        return hasQuiz;
    }

    /// <summary>
    /// Retorna o tipo de quiz disponível para esta carta
    /// </summary>
    public QuizType GetQuizType()
    {
        return themeCard?.quizData?.GetAvailableQuizType() ?? QuizType.None;
    }

    #endregion
}
