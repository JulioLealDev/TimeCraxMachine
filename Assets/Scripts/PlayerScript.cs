using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Managers;

public class PlayerScript : MonoBehaviourPunCallbacks
{

    public int numberRepairCards;
    public string nickname;
    public int index;
    public int plateNameIndex;
    public bool yourTurn = false;
    public string numberRepairCardsText;
    public int actorNumber;


    void Start()
    {
        numberRepairCards = 0;
        InitializePlayerIndex();
        numberRepairCardsText = GameObjectNames.GetNumberRepairCards(index + 1);
    }

    public void UpdateIndex()
    {
        var orderedPlayerList = PlayerManager.GetOrderedPlayerList();

        for (int i = 0; i < orderedPlayerList.Length; i++)
        {
            if (orderedPlayerList[i].ActorNumber == photonView.ControllerActorNr)
            {
                index = i;
            }
        }
    }

    private void InitializePlayerIndex()
    {
        var orderedPlayerList = PlayerManager.GetOrderedPlayerList();

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
        if (findObject == null)
        {
            DebugHelper.Log($"[PlayerScript] DrawRepairCard: GameObject '{numberRepairCardsText}' não encontrado");
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            DebugHelper.Log($"[PlayerScript] DrawRepairCard: TextMeshProUGUI não encontrado em '{numberRepairCardsText}'");
            return;
        }

        int numberOfCards = int.Parse(textComponent.text);
        numberOfCards++;
        textComponent.text = numberOfCards.ToString();
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
        if (findObject == null)
        {
            DebugHelper.Log($"[PlayerScript] GiveRepairCard: GameObject '{numberRepairCardsText}' não encontrado");
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            DebugHelper.Log($"[PlayerScript] GiveRepairCard: TextMeshProUGUI não encontrado em '{numberRepairCardsText}'");
            return;
        }

        int numberOfCards = int.Parse(textComponent.text);
        numberOfCards--;
        textComponent.text = numberOfCards.ToString();
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
        if (findObject == null)
        {
            DebugHelper.Log($"[PlayerScript] DescreaseAndDestroyCards: GameObject '{numberRepairCardsText}' não encontrado");
            DestroyRepairCards(cards);
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            DebugHelper.Log($"[PlayerScript] DescreaseAndDestroyCards: TextMeshProUGUI não encontrado em '{numberRepairCardsText}'");
            DestroyRepairCards(cards);
            return;
        }

        int numberOfCards = int.Parse(textComponent.text);
        numberOfCards -= cards;
        textComponent.text = numberOfCards.ToString();

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
