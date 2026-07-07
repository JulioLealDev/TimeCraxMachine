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
    public void DrawBonusCard()
    {
        numberBonusCards++;

        DebugHelper.Log("------ mais: "+numberBonusCardsText);

        var findObject = GameObject.Find(numberBonusCardsText);
        if (findObject == null)
        {
            DebugHelper.Log($"[PlayerScript] DrawBonusCard: GameObject '{numberBonusCardsText}' não encontrado");
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            DebugHelper.Log($"[PlayerScript] DrawBonusCard: TextMeshProUGUI não encontrado em '{numberBonusCardsText}'");
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
                var textComponent = findObject.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = numberBonusCards.ToString();
                }
            }

            DebugHelper.Log($"[PlayerScript] RemoveBonusCard: {numberBonusCards} cartas restantes");
        }
    }

    public void GiveBonusCard(PlayerScript otherPlayer)
    {
        otherPlayer.numberBonusCards++;

        DebugHelper.Log("------ menos: " + numberBonusCardsText);

        var findObject = GameObject.Find(numberBonusCardsText);
        if (findObject == null)
        {
            DebugHelper.Log($"[PlayerScript] GiveBonusCard: GameObject '{numberBonusCardsText}' não encontrado");
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            DebugHelper.Log($"[PlayerScript] GiveBonusCard: TextMeshProUGUI não encontrado em '{numberBonusCardsText}'");
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
        numberBonusCards -= cards;

        DebugHelper.Log("------ repair: " + numberBonusCardsText);

        var findObject = GameObject.Find(numberBonusCardsText);
        if (findObject == null)
        {
            DebugHelper.Log($"[PlayerScript] DescreaseAndDestroyCards: GameObject '{numberBonusCardsText}' não encontrado");
            DestroyBonusCards(cards);
            return;
        }

        var textComponent = findObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            DebugHelper.Log($"[PlayerScript] DescreaseAndDestroyCards: TextMeshProUGUI não encontrado em '{numberBonusCardsText}'");
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
