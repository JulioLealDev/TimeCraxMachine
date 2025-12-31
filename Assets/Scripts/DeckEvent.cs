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
            photonView.RPC("ClickDraw", RpcTarget.All, 1);

            gameManager.BlockActions();
            gameManager.ActivateFinishButton(false);
            if (photonView.IsMine)
            {
                var timeline = FindFirstObjectByType<Timeline>();
                timeline.ActiveTimeline(false);

                EventRandom();
            }
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

            DebugHelper.Log("Voc� j� realizaou uma a��o neste turno");

            this.DelayedCall(1.5f, HideActionInfo);
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

    public void EventRandom()
    {
        foreach (var number in eventList)
        {
            DebugHelper.Log(number);
        }

        DebugHelper.Log("max range (index): " + (eventList.Count));
        int index = Random.Range(0, eventList.Count);
        DebugHelper.Log("result: " + index);

        DrawEventCard(index);

    }
    public void DrawEventCard(int index)
    {
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var eventCard in eventCards)
        {
            //DebugHelper.Log("slotcount: "+ eventCard.slotCount+" -- valor: " + eventList[index]);
            if (eventCard.slotCount == eventList[index])
            {
                eventCard.DrawEventCard();
            }

        }

    }
    public void RemoveIndex(int value)
    {

        DebugHelper.Log(" eventList.Count: " + eventList.Count);
        for (int i = 0; i < eventList.Count; i++)
        {
            DebugHelper.Log("eventList[i]: "+ eventList[i]+ " ---- valor :" + value);
            if (eventList[i] == value)
            {
                DebugHelper.Log("Removendo valor :" + value);
                eventList.RemoveAt(i);
            }
        }


        foreach (var number in eventList)
        {
            DebugHelper.Log("-- "+number);
        }
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
