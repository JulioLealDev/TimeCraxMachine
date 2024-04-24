using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
    
public class DeckEvent : MonoBehaviourPunCallbacks
{
    //public DeckRepair deckRepair;
    public GameManager gameManager;
    public Canvas gameInfo;
    private List<int> eventList = new List<int>();
    private int[] numbers = { 1, 2, 3, 4, 5, 6, 7 };
    public SoundEffects soundEffects;

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
                var timeline = FindObjectOfType<Timeline>();
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

            Debug.Log("Você já realizaou uma ação neste turno");

            Invoke("HideActionInfo", 1.5f);
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
        //Debug.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ActionInfoBackground"  )
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("DisableGameInfo", 0.5f);
    }

    public void DisableGameInfo()
    {
        //Debug.Log("DisableGameInfo()");
        gameInfo.gameObject.SetActive(false);
    }

    public void EventRandom()
    {
        foreach (var number in eventList)
        {
            Debug.Log(number);
        }

        Debug.Log("max range (index): " + (eventList.Count));
        int index = Random.Range(0, eventList.Count);
        Debug.Log("result: " + index);

        DrawEventCard(index);

    }
    public void DrawEventCard(int index)
    {
        var eventCards = FindObjectsOfType<EventCard>();
        foreach (var eventCard in eventCards)
        {
            //Debug.Log("slotcount: "+ eventCard.slotCount+" -- valor: " + eventList[index]);
            if (eventCard.slotCount == eventList[index])
            {
                eventCard.DrawEventCard();
            }

        }

    }
    public void RemoveIndex(int value)
    {

        Debug.Log(" eventList.Count: " + eventList.Count);
        for (int i = 0; i < eventList.Count; i++)
        {
            Debug.Log("eventList[i]: "+ eventList[i]+ " ---- valor :" + value);
            if (eventList[i] == value)
            {
                Debug.Log("Removendo valor :" + value);
                eventList.RemoveAt(i);
            }
        }


        foreach (var number in eventList)
        {
            Debug.Log("-- "+number);
        }
    }

    public void ResetAllEventCards()
    {
        eventList.Clear();
        eventList.AddRange(numbers);

        var eventCards = FindObjectsOfType<EventCard>();

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
