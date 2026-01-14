using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class DeckEvent : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Canvas gameInfo;
    [SerializeField] private SoundEffects soundEffects;

    private List<int> eventList = new List<int>();
    private int[] numbers = { 1, 2, 3, 4, 5, 6, 7 };

    void Start()
    {
        eventList.Clear();
        eventList.AddRange(numbers);
    }

    public void OnMouseDown()
    {
        if (gameObject.CompareTag("Selectable"))
        {
            // Enviar requisição ao MasterClient para processar a compra
            photonView.RPC("RequestDrawEventCard", RpcTarget.MasterClient);
        }
        else
        {
            photonView.RPC("ClickDraw", RpcTarget.All, 2);

            Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
            gameInfo.gameObject.SetActive(true);

            foreach (var info in infos)
            {
                if (info.gameObject.name == "ActionInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }

            DebugHelper.Log("Você já realizou uma ação neste turno");

            this.DelayedCall(1.5f, HideActionInfo);
        }
    }

    /// <summary>
    /// RPC enviado ao MasterClient para processar a compra de carta
    /// </summary>
    [PunRPC]
    public void RequestDrawEventCard()
    {
        // Apenas MasterClient processa e sincroniza para todos
        if (PhotonNetwork.IsMasterClient)
        {
            int index = Random.Range(0, eventList.Count);
            int slotCount = eventList[index];
            DebugHelper.Log($"[DeckEvent] MasterClient gerou slotCount: {slotCount}");

            // Sincronizar som, bloqueio e carta para todos
            photonView.RPC("ExecuteDrawEventCard", RpcTarget.All, slotCount);
        }
    }

    /// <summary>
    /// RPC executado em todos os clientes para comprar a carta
    /// </summary>
    [PunRPC]
    public void ExecuteDrawEventCard(int slotCount)
    {
        DebugHelper.Log($"[DeckEvent] ExecuteDrawEventCard: slotCount={slotCount}");

        // Tocar som
        soundEffects.PlayDrawCardSound();

        // Bloquear ações
        gameManager.BlockActions();
        gameManager.ActivateFinishButton(false);

        var timeline = FindFirstObjectByType<Timeline>();
        timeline.ActiveTimeline(false);

        // Comprar a carta (local, pois já estamos dentro de um RPC sincronizado)
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var eventCard in eventCards)
        {
            if (eventCard.slotCount == slotCount)
            {
                eventCard.DrawEventCardLocal();
                break;
            }
        }
    }

    [PunRPC]
    public void ClickDraw(int idSound)
    {
        if(idSound == 1)
        {
            soundEffects.PlayDrawCardSound();
        }
        else if(idSound == 2)
        {
            soundEffects.TagSound();
        }
    }
    public void HideActionInfo()
    {
        //DebugHelper.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ActionInfoBackground"  )
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        //DebugHelper.Log("DisableGameInfo()");
        gameInfo.gameObject.SetActive(false);
    }


    /// <summary>
    /// Remove carta do deck - chamado via RPC para sincronizar
    /// </summary>
    public void RemoveIndex(int value)
    {
        // Apenas MasterClient inicia a remoção e sincroniza
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RemoveIndexRPC", RpcTarget.All, value);
        }
    }

    [PunRPC]
    public void RemoveIndexRPC(int value)
    {
        DebugHelper.Log($"[DeckEvent] RemoveIndexRPC: removendo {value}");
        DebugHelper.Log(" eventList.Count: " + eventList.Count);

        for (int i = 0; i < eventList.Count; i++)
        {
            DebugHelper.Log("eventList[i]: " + eventList[i] + " ---- valor :" + value);
            if (eventList[i] == value)
            {
                DebugHelper.Log("Removendo valor :" + value);
                eventList.RemoveAt(i);
                break; // Importante: sair após remover para evitar index out of range
            }
        }

        foreach (var number in eventList)
        {
            DebugHelper.Log("-- " + number);
        }
    }

    /// <summary>
    /// Adiciona uma carta de volta ao deck (usado quando quiz falha)
    /// </summary>
    public void AddCardBack(int slotCount)
    {
        // Apenas MasterClient inicia a adição e sincroniza
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("AddCardBackRPC", RpcTarget.All, slotCount);
        }
    }

    [PunRPC]
    public void AddCardBackRPC(int slotCount)
    {
        if (!eventList.Contains(slotCount))
        {
            eventList.Add(slotCount);
            DebugHelper.Log($"[DeckEvent] Carta {slotCount} adicionada de volta ao deck");
        }
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

        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        foreach (var eventCard in eventCards)
        {
            if(eventCard.GetComponent<MeshRenderer>().enabled)
            {
                eventCard.GetComponent<MeshRenderer>().enabled = false;
                eventCard.GetComponent<Animator>().SetBool("wrongSlot", true);
            }

        }

        //foreach (var eventCard in eventCards)
        //{
        //    eventCard.GetComponent<Animator>().SetBool("drawingEventCard", false);
        //    eventCard.GetComponent<Animator>().SetBool("wrongSlot", false);
        //    eventCard.GetComponent<Animator>().SetInteger("slotClicked", 0);
        //}


    }

}
