using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro;
using TimeCrax.Core;

public class PlayerScript : MonoBehaviourPunCallbacks
{

    public int numberRepairCards;
    public string nickname;
    public int index;
    public int plateNameIndex;
    public bool yourTurn = false;
    public string numberRepairCardsText;
    public int actorNumber;


    // Start is called before the first frame update
    void Start()
    {
        numberRepairCards = 0;

        // Ordenar PlayerList por ActorNumber para garantir ordem consistente em todos os clientes
        var orderedPlayerList = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();

        for (int i = 0; i < orderedPlayerList.Length; i++)
        {
            if (orderedPlayerList[i].ActorNumber == photonView.ControllerActorNr)
            {
                nickname = orderedPlayerList[i].NickName;
                index = i;
                plateNameIndex = i;
                actorNumber = orderedPlayerList[i].ActorNumber;
            }
        }

        numberRepairCardsText = "numberRepairCards0" + (index + 1);
    }
    public void UpdateIndex()
    {
        // Ordenar PlayerList por ActorNumber para garantir ordem consistente em todos os clientes
        var orderedPlayerList = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();

        for (int i = 0; i < orderedPlayerList.Length; i++)
        {
            if (orderedPlayerList[i].ActorNumber == photonView.ControllerActorNr)
            {
                //nickname = orderedPlayerList[i].NickName;
                index = i;
            }
        }
    }

    public void DrawEventCard()
    {
        DebugHelper.Log("You draw one EventCard!");
    }
    public void DrawRepairCard()
    {
        numberRepairCards++;

        DebugHelper.Log("------ mais: "+numberRepairCardsText);

        var findObject = GameObject.Find(numberRepairCardsText);
        DebugHelper.Log("name: "+findObject.name);

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

        DebugHelper.Log("------ menos: " + numberRepairCardsText);

        var findObject = GameObject.Find(numberRepairCardsText);
        DebugHelper.Log("name: " + findObject.name);

        int numberOfCards = int.Parse(findObject.GetComponent<TextMeshProUGUI>().text);
        numberOfCards--;

        findObject.GetComponent<TextMeshProUGUI>().text = numberOfCards.ToString();
    }

    public void RepairComponent(int cards)
    {
        DebugHelper.Log("cartas: " + cards);
        photonView.RPC("DescreaseAndDestroyCards", RpcTarget.All, cards);
    }

    [PunRPC]
    public void DescreaseAndDestroyCards(int cards)
    {
        numberRepairCards -= cards;

        DebugHelper.Log("------ repair: " + numberRepairCardsText);

        var findObject = GameObject.Find(numberRepairCardsText);
        DebugHelper.Log("name: " + findObject.name);

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
        var allCards = FindObjectsByType<RepairCard>(FindObjectsSortMode.None);
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
            DebugHelper.Log("carta -> " + orderedlist[i].photonView.ViewID);
            orderedlist[i].GetComponent<Animator>().enabled = true;
            orderedlist[i].GetComponent<Animator>().SetBool("destroyCard", true);
        }

    }
}
