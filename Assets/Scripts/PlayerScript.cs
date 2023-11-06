using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro;

public class PlayerScript : MonoBehaviourPunCallbacks
{

    public int numberRepairCards;
    public string nickname;
    public int index;
    public bool yourTurn = false;
    public string numberRepairCardsText;


    // Start is called before the first frame update
    void Start()
    {
        numberRepairCards = 0;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == photonView.ControllerActorNr)
            {
                nickname = PhotonNetwork.PlayerList[i].NickName;
                index = i;
            }
        }

        numberRepairCardsText = "numberRepairCards0" + (index + 1);
}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawEventCard()
    {
        Debug.Log("You draw one EventCard!");
    }
    public void DrawRepairCard()
    {
        numberRepairCards++;

        Debug.Log("------ mais: "+numberRepairCardsText);

        var findObject = GameObject.Find(numberRepairCardsText);
        Debug.Log("name: "+findObject.name);

        int numberOfCards = int.Parse(findObject.GetComponent<TextMeshProUGUI>().text);
        numberOfCards++;

        findObject.GetComponent<TextMeshProUGUI>().text = numberOfCards.ToString();
    }

    public int GetNumberOfRepairsCards()
    {
        return numberRepairCards;
    }

    public void GiveRepairCard(PlayerScript otherPlayer)
    {
        otherPlayer.numberRepairCards++;

        Debug.Log("------ menos: " + numberRepairCardsText);

        var findObject = GameObject.Find(numberRepairCardsText);
        Debug.Log("name: " + findObject.name);

        int numberOfCards = int.Parse(findObject.GetComponent<TextMeshProUGUI>().text);
        numberOfCards--;

        findObject.GetComponent<TextMeshProUGUI>().text = numberOfCards.ToString();
    }

    public void RepairComponent(int cards)
    {
        Debug.Log("cartas: " + cards);
        photonView.RPC("DescreaseAndDestroyCards", RpcTarget.All, cards);
    }

    [PunRPC]
    public void DescreaseAndDestroyCards(int cards)
    {
        numberRepairCards -= cards;

        Debug.Log("------ repair: " + numberRepairCardsText);

        var findObject = GameObject.Find(numberRepairCardsText);
        Debug.Log("name: " + findObject.name);

        int numberOfCards = int.Parse(findObject.GetComponent<TextMeshProUGUI>().text);
        numberOfCards -= cards;

        findObject.GetComponent<TextMeshProUGUI>().text = numberOfCards.ToString();

        DestroyRepairCards(cards);
    }

    public bool GetYourTurn()
    {
        return yourTurn;
    }

    public void SetYourTurn(bool isYourTurn)
    {
        yourTurn = isYourTurn;
    }

    public void DestroyRepairCards(int cardNumber)
    {
        var allCards = FindObjectsOfType<RepairCard>();
        List<RepairCard> cardList = new List<RepairCard>();

        foreach (var card in allCards)
        {
            if(card.photonView.OwnerActorNr == gameObject.GetPhotonView().OwnerActorNr)
            {
                cardList.Add(card);
            }
        }
        var orderedlist = cardList.OrderByDescending(x => x.photonView.ViewID).ToList();

        for (var i = 0; i < cardNumber; i++)
        {
            Debug.Log("carta -> " + orderedlist[i].photonView.ViewID);
            orderedlist[i].GetComponent<Animator>().enabled = true;
            orderedlist[i].GetComponent<Animator>().SetBool("destroyCard", true);
        }

    }
}
