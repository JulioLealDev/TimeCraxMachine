using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DeckEvent : MonoBehaviourPunCallbacks
{
    public DeckRepair deckRepair;
    public GameManager gameManager;
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
            Debug.Log("Você já realizaou uma ação neste turno");
        }
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
