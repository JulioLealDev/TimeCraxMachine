using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
    
public class DeckEvent : MonoBehaviourPunCallbacks
{
    public DeckRepair deckRepair;
    public GameManager gameManager;
    public Canvas gameInfo;
    private List<int> eventList = new List<int>();

    void Start()
    {
        int[] numbers = { 1, 2, 3, 4, 5, 6, 7 };
        eventList.AddRange(numbers);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {

        if (gameObject.CompareTag("Selectable"))
        {
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
        int index = Random.Range(0, eventList.Count - 1);
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
        for(int i = 0; i < eventList.Count - 1; i++)
        {
            if (eventList[i] == value)
            {
                eventList.RemoveAt(i);
            }
        }

    }

}
