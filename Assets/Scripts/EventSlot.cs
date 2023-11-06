using UnityEngine;
using Photon.Pun;

public class EventSlot : MonoBehaviourPunCallbacks

{
    public int slotNumber;
    public int randomNumber;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnMouseDown()
    {
        var eventCards = FindObjectsOfType<EventCard>();
        foreach (var card in eventCards)
        {
            if (card.CompareTag("Drew"))
            {
                SetUpSlots(false, "Undestructable");

                if (slotNumber == card.slotCount)
                {
                    //Debug.Log("É igual!");
                    photonView.RPC("ClickedRightSlot", RpcTarget.All, card.slotCount);
                }
                else
                {
                    //Debug.Log("Noé igual!");
                    var gameManager = FindObjectOfType<GameManager>();
                    gameManager.RandomComponentNumber();

                    photonView.RPC("ClickedWrongSlot", RpcTarget.All, card.slotCount);

                }
            }

            //photonView.RPC("ClickSlot", RpcTarget.All);
        }

            
    }
    [PunRPC]
    public void ClickedWrongSlot(int slotCount)
    {
        var cards = FindObjectsOfType<EventCard>();
        foreach (var card in cards)
        {
            //Debug.Log("cardslotcount: "+ card.slotCount+" -- slotcount:"+slotCount);
            if (card.slotCount == slotCount)
            {
                card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);
                //Debug.Log("cardname: "+card.name);
                card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
                card.tag = "Undestructable";
                card.waitToDistance();
            }
        }
    }

    [PunRPC]
    public void ClickedRightSlot(int slotCount)
    {
        gameObject.tag = "Disabled";

        var deckEvent = FindObjectOfType<DeckEvent>();
        deckEvent.RemoveIndex(slotCount);

        var cards = FindObjectsOfType<EventCard>();
        foreach (var card in cards)
        {
            if(card.slotCount == slotCount)
            {
                card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);
                card.tag = "Disabled";
                card.waitToDistance();
            }
        }

        CheckIfWin();

    }

    //[PunRPC]
    //public void ClickSlot()
    //{
    //    var eventCards = FindObjectsOfType<EventCard>();
    //    foreach (var card in eventCards)
    //    {
    //        if (card.CompareTag("Drew"))
    //        {
    //            SetUpSlots(false, "Undestructable");
    //            //Debug.Log("CardName: " + card.slotCount + " -- SlotName: " + slotNumber);
    //            card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);

    //            if (slotNumber == card.slotCount)
    //            {
    //                Debug.Log("É igual!");
    //                card.tag = "Disabled";
    //                gameObject.tag = "Disabled";
    //                var deckEvent = FindObjectOfType<DeckEvent>();
    //                deckEvent.RemoveIndex(card.slotCount);
    //                card.waitToDistance();
    //                CheckIfWin();
    //            }
    //            else
    //            {
    //                var gameManager = FindObjectOfType<GameManager>();
    //                gameManager.RandomComponentNumber();
    //                card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
    //                card.tag = "Undestructable";
    //                Debug.Log("Noé igual!");
    //                card.waitToDistance();
    //            }
    //        }
    //    }
    //}

    public void SetUpSlots(bool activateSlot, string tag)
    {
        //Debug.Log("SetUpSlots");

        var slots = FindObjectsOfType<EventSlot>();
        foreach (var slot in slots)
        {
            //Debug.Log("slot "+slot.slotNumber+" -- tag: "+slot.tag);
            if (!slot.CompareTag("Disabled"))
            {
                slot.tag = tag;
                slot.GetComponentInChildren<MeshCollider>().enabled = activateSlot;
            }

        }

    }

    public void CheckIfWin()
    {
        var slots = FindObjectsOfType<EventSlot>();
        int slotsFilled = 0;
        foreach (var slot in slots)
        {
            //Debug.Log("slot " + slot.slotNumber + " -- tag: " + slot.tag);
            if (slot.CompareTag("Disabled"))
            {
                slotsFilled++;
            }

        }
        if (slotsFilled == 7)
        {
            Invoke("Victory", 3f);
        }
    }

    public void Victory()
    {
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.DeactivateAll();

        GameObject victory = GameObject.FindGameObjectWithTag("Victory");
        victory.transform.GetChild(0).gameObject.SetActive(true);
        //Debug.Log("name ---> " + victory.name);
    }
}
