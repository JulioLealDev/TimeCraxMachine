using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class DeckEvent : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Canvas gameInfo;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private TimelineColliderArea timelineColliderArea;
    private List<int> eventList = new List<int>();
    private int[] numbers = { 1, 2, 3, 4, 5, 6 };

    void Start()
    {
        eventList.Clear();
        eventList.AddRange(numbers);
    }

    public void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (CameraController.IsAnimating) return;
        if (!GameManager.TryBeginClick(this)) return;

        if (gameObject.CompareTag("Selectable"))
        {
            GameStateManager.TransitionTo(GamePhase.IM_DrewEventCard);

            gameObject.tag = "Undestructable";
            timelineColliderArea.SetUpTimelineCollider(false);
            
            photonView.RPC("RequestDrawEventCard", RpcTarget.MasterClient);
        }
        else
        {
            photonView.RPC("PlayTagSound", RpcTarget.All);

            gameInfo.gameObject.SetActive(true);
            foreach (Transform info in gameInfo.GetComponentsInChildren<Transform>())
            {
                if (info.gameObject.name == "ActionInfoBackground")
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
            }

            this.DelayedCall(1.5f, HideActionInfo);
        }
    }

    /// <summary>
    /// RPC enviado ao MasterClient para processar a compra de carta
    /// </summary>
    [PunRPC]
    public void RequestDrawEventCard()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int index = Random.Range(0, eventList.Count);
            int slotCount = eventList[index];
            photonView.RPC("ExecuteDrawEventCard", RpcTarget.All, slotCount);
        }
    }

    /// <summary>
    /// RPC executado em todos os clientes para comprar a carta
    /// </summary>
    [PunRPC]
    public void ExecuteDrawEventCard(int slotCount)
    {
        soundEffects.PlayDrawCardSound();

        gameManager.BlockActions();
        gameManager.ActivateFinishButton(false);

        var timeline = FindFirstObjectByType<Timeline>();
        if (timeline != null)
            timeline.ActiveTimeline(false);

        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var eventCard in eventCards)
        {
            if (eventCard.slotCount == slotCount)
            {
                eventCard.ExecuteDraw();
                break;
            }
        }

        GameManager.ResetClick(this);
    }

    [PunRPC]
    public void PlayTagSound()
    {
        soundEffects.TagSound();
    }

    public void HideActionInfo()
    {
        foreach (Transform info in gameInfo.GetComponentsInChildren<Transform>())
        {
            if (info.gameObject.name == "ActionInfoBackground")
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
        GameManager.ResetClick(this);
    }

    public void ResetClickProtection()
    {
        GameManager.ResetClick(this);
    }

    /// <summary>
    /// Remove carta do deck - chamado via RPC para sincronizar
    /// </summary>
    public void RemoveIndex(int value)
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("RemoveIndexRPC", RpcTarget.All, value);
    }

    [PunRPC]
    public void RemoveIndexRPC(int value)
    {
        for (int i = 0; i < eventList.Count; i++)
        {
            if (eventList[i] == value)
            {
                eventList.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// Adiciona uma carta de volta ao deck
    /// </summary>
    public void AddCardBack(int slotCount)
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("AddCardBackRPC", RpcTarget.All, slotCount);
    }

    [PunRPC]
    public void AddCardBackRPC(int slotCount)
    {
        if (!eventList.Contains(slotCount))
            eventList.Add(slotCount);
    }

    /// <summary>
    /// Retorna quantidade de cartas restantes no deck
    /// </summary>
    public int GetRemainingCards()
    {
        return eventList.Count;
    }

    public void ResetAllEventCards()
    {
        eventList.Clear();
        eventList.AddRange(numbers);

        foreach (var eventCard in FindObjectsByType<EventCard>(FindObjectsSortMode.None))
        {
            if (eventCard.GetComponent<MeshRenderer>().enabled)
            {
                eventCard.GetComponent<MeshRenderer>().enabled = false;
                eventCard.GetComponent<Animator>().SetBool("wrongSlot", true);
            }
        }
    }
    public void TurnDeckEventSelectable(bool selectable)
    {
        if (selectable)
        {
            gameObject.tag = "Selectable";
        }
        else
        {
            gameObject.tag = "Undestructable";
        }
    }
}
