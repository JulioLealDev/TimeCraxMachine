using UnityEngine;
using Photon.Pun;
using System;
using TimeCrax.Core;
using TimeCrax.Themes;

public class EventCard : MonoBehaviourPunCallbacks
{
    public CameraController cameraController;
    public int slotCount;
    public int slotYear;

    // Referência à carta do tema (novo sistema)
    private ThemeCard themeCard;

    // Start is called before the first frame update
    void Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
    }

    public void DrawEventCard()
    {
        photonView.RPC("DrawingEventCard", RpcTarget.All);
    }

    [PunRPC]
    public void DrawingEventCard()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
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
        return themeCard?.quizData?.HasQuiz ?? false;
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
