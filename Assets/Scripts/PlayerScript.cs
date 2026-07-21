using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Managers;

public class PlayerScript : MonoBehaviourPunCallbacks
{

    public int numberBonusCards;
    public string nickname;
    public int index;
    public int plateNameIndex;
    public bool yourTurn = false;
    public string numberBonusCardsText;
    public int actorNumber;


    void Start()
    {
        numberBonusCards = 0;
        InitializePlayerIndex();
        numberBonusCardsText = GameObjectNames.GetNumberBonusCards(index + 1);
    }

    public void UpdateIndex()
    {
        var orderedPlayerList = PlayerManager.GetOrderedPlayerList();

        for (int i = 0; i < orderedPlayerList.Length; i++)
        {
            if (orderedPlayerList[i].ActorNumber == photonView.OwnerActorNr)
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
            if (orderedPlayerList[i].ActorNumber == photonView.OwnerActorNr)
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
    }
    public void DrawBonusCard()
    {
        numberBonusCards++;


        var findObject = GameObject.Find(numberBonusCardsText);
        if (findObject == null)
        {
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshPro>();
        if (textComponent == null)
        {
            return;
        }

        int numberOfCards = int.Parse(textComponent.text);
        numberOfCards++;
        textComponent.text = numberOfCards.ToString();
    }

    public int GetNumberOfBonusCards()
    {
        return numberBonusCards;
    }

    /// <summary>
    /// Remove uma carta bonus do contador (chamado quando carta é consumida)
    /// </summary>
    public void RemoveBonusCard()
    {
        photonView.RPC("RPC_RemoveBonusCard", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_RemoveBonusCard()
    {
        if (numberBonusCards > 0)
        {
            numberBonusCards--;

            var findObject = GameObject.Find(numberBonusCardsText);
            if (findObject != null)
            {
                var textComponent = findObject.GetComponent<TextMeshPro>();
                if (textComponent != null)
                {
                    textComponent.text = numberBonusCards.ToString();
                }
            }

        }
    }

    public void GiveBonusCard(PlayerScript otherPlayer)
    {
        otherPlayer.numberBonusCards++;


        var findObject = GameObject.Find(numberBonusCardsText);
        if (findObject == null)
        {
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshPro>();
        if (textComponent == null)
        {
            return;
        }

        int numberOfCards = int.Parse(textComponent.text);
        numberOfCards--;
        textComponent.text = numberOfCards.ToString();
    }

    public void RepairComponent(int cards)
    {
        photonView.RPC("DescreaseAndDestroyCards", RpcTarget.All, cards);
    }

    [PunRPC]
    public void DescreaseAndDestroyCards(int cards)
    {
        numberBonusCards -= cards;


        var findObject = GameObject.Find(numberBonusCardsText);
        if (findObject == null)
        {
            DestroyBonusCards(cards);
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshPro>();
        if (textComponent == null)
        {
            DestroyBonusCards(cards);
            return;
        }

        int numberOfCards = int.Parse(textComponent.text);
        numberOfCards -= cards;
        textComponent.text = numberOfCards.ToString();

        DestroyBonusCards(cards);
    }

    public bool GetYourTurn()
    {
        return yourTurn;
    }

    public void SetYourTurn(bool isYourTurn)
    {
        yourTurn = isYourTurn;
    }

    public void DestroyBonusCards(int cardNumber)
    {
        var allCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
        List<BonusCard> cardList = new List<BonusCard>();

        foreach (var card in allCards)
        {
            if(card.photonView.OwnerActorNr == photonView.OwnerActorNr)
            {
                cardList.Add(card);
            }
        }
        var orderedlist = cardList.OrderByDescending(x => x.photonView.ViewID).ToList();

        for (var i = 0; i < cardNumber; i++)
        {
            orderedlist[i].GetComponent<Animator>().enabled = true;
            orderedlist[i].GetComponent<Animator>().SetBool("destroyCard", true);
        }

    }
}
