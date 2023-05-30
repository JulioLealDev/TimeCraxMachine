using UnityEngine;
using Photon.Pun;

public class EventSlot : MonoBehaviourPunCallbacks

{
    public int slotNumber;

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
        photonView.RPC("ClickSlot", RpcTarget.All);
    }

    [PunRPC]
    public void ClickSlot()
    {
        var eventCards = FindObjectsOfType<EventCard>();
        foreach (var card in eventCards)
        {
            if (card.CompareTag("Drew"))
            {
                SetUpSlots(false, "Undestructable");
                Debug.Log("CardName: " + card.slotCount + " -- SlotName: " + slotNumber);
                card.gameObject.GetComponent<Animator>().SetInteger("slotClicked", slotNumber);

                if (slotNumber == card.slotCount)
                {
                    Debug.Log("É igual!");
                    card.tag = "Disabled";
                    gameObject.tag = "Disabled";
                    var deckEvent = FindObjectOfType<DeckEvent>();
                    deckEvent.RemoveIndex(card.slotCount);
                    card.waitToDistance();
                }
                else
                {
                    var gameManager = FindObjectOfType<GameManager>();
                    gameManager.RandomComponentNumber();
                    card.gameObject.GetComponent<Animator>().SetBool("wrongSlot", true);
                    card.tag = "Undestructable";
                    Debug.Log("Noé igual!");
                    card.waitToDistance();
                }
            }
        }
    }

    public void SetUpSlots(bool activateSlot, string tag)
    {
        Debug.Log("SetUpSlots");

        var slots = FindObjectsOfType<EventSlot>();
        foreach (var slot in slots)
        {
            Debug.Log("slot "+slot.slotNumber+" -- tag: "+slot.tag);
            if (!slot.CompareTag("Disabled"))
            {
                slot.tag = tag;
                slot.GetComponentInChildren<MeshCollider>().enabled = activateSlot;
            }

        }

    }
}
