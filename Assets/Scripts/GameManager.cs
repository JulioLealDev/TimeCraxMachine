using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int randomId;
    private Component[] timeCraxComponents;
    private PlayerScript[] players;
    public GameObject gameInfo;
    public GameObject deckEvent;
    public GameObject deckRepair;
    public GameObject timeline;
    public Camera gameCamera;
    public GameObject inputName;
    public GameObject suitTop;
    public GameObject gameHUD;
    public GameObject hud;
    public GameObject endButton;
    private int[] playersList;
    private int round = 1;
    private int roundCompare = 1;
    private int time = 0;
    private List<int> componentList = new List<int>();

    private void Awake()
    {
        inputName.SetActive(false);
        PhotonNetwork.Instantiate("Player", new Vector3(7.224f, 1.01f, 0.83f), Quaternion.identity);
        playersList = new int[PhotonNetwork.PlayerList.Length];
    }
    void Start()
    {
        Debug.Log("Start()");

        timeCraxComponents = FindObjectsOfType<Component>();

        gameCamera.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);

        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        componentList.AddRange(numbers);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playersList[i] = PhotonNetwork.PlayerList[i].ActorNumber;

        }
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("StartGame", 6f);
        }

    }

    public void StartGame()
    {
        Debug.Log("StartGame()");
        photonView.RPC("ShowHUD", RpcTarget.All);
        //ShowHUD();
    }

    [PunRPC]
    public void ShowHUD()
    {

        hud.SetActive(true);
        var outline = FindObjectOfType<OutlineAction>();
        outline.MakeObjectsSelectable();

        var components = hud.GetComponentsInChildren<Transform>();

        string plateName = "plateName0";
        string namePlayer = "namePlayer0";
        string repairCardSymbol = "repairCardSymbol0";
        string numberRepairCards = "numberRepairCards0";

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int name = i + 1;
            for (int x = 0; x < components.Length; x++)
            {
                //Debug.Log("component name:" + components[x].name+" - name: "+ plateName+name.ToString());
                if (components[x].name == plateName + name.ToString())
                {
                    components[x].gameObject.GetComponent<MeshRenderer>().enabled = true;
                }
                else if(components[x].name == "FinishTurn" || components[x].name == "QuitGame")
                {
                    components[x].gameObject.GetComponent<CanvasGroup>().LeanAlpha(1f, 2f);
                }
                else if (components[x].name == namePlayer + name.ToString())
                {
                    TextMeshProUGUI textName = components[x].gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    textName.text = PhotonNetwork.PlayerList[i].NickName;
                    textName.GetComponent<CanvasGroup>().LeanAlpha(1f, 2f);
                }
                else if (components[x].name == repairCardSymbol + name.ToString())
                {
                    components[x].GetComponent<SpriteRenderer>().enabled = true;
                }
                else if (components[x].name == numberRepairCards + name.ToString())
                {
                    components[x].GetComponent<TextMeshProUGUI>().text = "0";
                }
            }
        }
        Invoke("FirstTurn", 2f);
    }
    public void FirstTurn()
    {
        //Debug.Log("FirstTurn()");
        //photonView.RPC("Turn", RpcTarget.All);
        Turn();
    }

    //[PunRPC]
    public void Turn()
    {
        //Debug.Log("Turn()");
        //Debug.Log("1 -- time: " + time + " < numPlayers: " + PhotonNetwork.PlayerList.Length);
        if (time < PhotonNetwork.PlayerList.Length)
        {
            players = FindObjectsOfType<PlayerScript>();
            Button[] components = gameHUD.GetComponentsInChildren<Button>();
            int indexPlayer = time + 1;

            foreach (var player in players)
            {
                //Debug.Log("time: " + time);

                if (player.index == time)
                {
                    //Debug.Log("agora é o turno de : " + player.nickname);
                    player.SetYourTurn(true);

                    ChangeRepairCardsView(player);

                    foreach (Button component in components)
                    {
                        //Debug.Log("name: " + component.name + " - time+1:" + indexPlayer);
                        if (component.name == indexPlayer.ToString())
                        {
                            component.interactable = false;
                        }
                        else
                        {
                            component.interactable = true;
                        }

                    }
                }
                else
                {
                    //Debug.Log("não é turno de : " + player.nickname);
                    player.SetYourTurn(false);

                    foreach (Button component in components)
                    {
                        if (!(component.name == "QuitGame"))
                        {
                            component.interactable = false;
                        }

                    }
                }
            }

            ShowRoundInfo();

        }
        else
        {
            //Debug.Log("Caiu no else");
            time = 0;
            round++;
            Turn();
        }

    }

    void Update()
    {

    }

    //[PunRPC]
    public void ShowRoundInfo()
    {
        //Debug.Log("ShowRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        gameInfo.gameObject.SetActive(true);
        if (round == roundCompare)
        {
            roundCompare++;

            foreach (var info in infos)
            {

                if (info.gameObject.name == "TurnInfo")
                {
                    info.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Is " + PhotonNetwork.PlayerList[time].NickName + " Turn";
                }
                if (info.gameObject.name == "RoundInfo")
                {
                    info.GetComponentInChildren<TextMeshProUGUI>().text = "Starting Round " + round;
                }
                if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }
        }
        else
        {
            foreach (var info in infos)
            {

                if (info.gameObject.name == "TurnInfo")
                {
                    info.GetComponentInChildren<TextMeshProUGUI>().text = "Is " + PhotonNetwork.PlayerList[time].NickName + " Turn";
                }
                if (info.gameObject.name == "TurnInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }

            }
        }
        Invoke("HideRoundInfo", 1.5f);
    }

    public void HideRoundInfo()
    {
        //Debug.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
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
        StartTurn();
    }

    public void StartTurn()
    {
        //Button[] components = gameHUD.GetComponentsInChildren<Button>();

        players = FindObjectsOfType<PlayerScript>();

        deckEvent.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        deckRepair.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        timeline.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        endButton.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);


        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            string plateName = "plateName0" + (i + 1);
            var findObject = GameObject.Find(plateName);

            findObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        }

            foreach (var component in timeCraxComponents)
        {
            component.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        }

        foreach (var player in players)
        {

            if (player.index == time)
            {

                if (player.GetNumberOfRepairsCards() == 5)
                {
                    //Debug.Log("tem 5 cartas");
                    deckRepair.tag = "Disabled";
                }
                else
                {
                    //Debug.Log("nao tem 5 cartas");
                    deckRepair.tag = "Selectable";
                }

                foreach (var timeCraxComponent in timeCraxComponents)
                {
                    if (timeCraxComponent.malfunctions == 1)
                    {
                        timeCraxComponent.tag = "Selectable";
                    }
                }

                endButton.GetComponent<MeshCollider>().enabled = true;

                timeline.GetComponent<MeshCollider>().enabled = true;
                deckEvent.GetComponent<MeshCollider>().enabled = true;
                deckRepair.GetComponent<MeshCollider>().enabled = true;
                deckEvent.tag = "Selectable";
                timeline.tag = "Selectable";


                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    string name = "plateName0" + (i + 1);
                    var plate = GameObject.Find(name);

                    plate.GetComponent<MeshCollider>().enabled = true;
                    plate.tag = "Selectable";
                }

                string plateName = "plateName0" + (time + 1);
                var findObject = GameObject.Find(plateName);

                findObject.GetComponent<MeshCollider>().enabled = false;

            }
            else
            {
                //Debug.Log("4 -- ");
                player.SetYourTurn(false);

                //foreach (Button component in components)
                //{
                //    if (!(component.name == "QuitGame"))
                //    {
                //        component.interactable = false;
                //    }

                //}
                endButton.GetComponent<MeshCollider>().enabled = false;

                timeline.GetComponent<MeshCollider>().enabled = false;
                deckEvent.GetComponent<MeshCollider>().enabled = false;
                deckRepair.GetComponent<MeshCollider>().enabled = false;

                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    string name = "plateName0" + (i + 1);
                    var plate = GameObject.Find(name);

                    plate.GetComponent<MeshCollider>().enabled = false;
                    //plate.tag = "Selectable";
                }
            }
        }
    }

    public void RandomComponentNumber()
    {
        //Debug.Log("7 --");
        randomId = UnityEngine.Random.Range(1, componentList.Count);
        //Debug.Log("result: " + randomId);
        photonView.RPC("ComponentRandom", RpcTarget.All, randomId);
        //ComponentRandom(randomId);

    }

    public void EndTurn()
    {
        //FinishTurn();
        //string buttonName = EventSystem.current.currentSelectedGameObject.name;

        //Debug.Log("apertou Finish: "+buttonName);
        //Debug.Log("Finish - time: " + time + " == " + (PhotonNetwork.PlayerList.Length - 1));
        if (time == PhotonNetwork.PlayerList.Length - 1)
        {
            Debug.Log("Random Malfunction");
            RandomComponentNumber();
        }

        photonView.RPC("FinishTurn", RpcTarget.All);

    }

    public void SetUpComponents()
    {
        foreach (var player in players)
        {
            //Debug.Log("-- Player --: " + player.nickname);
            foreach (var component in timeCraxComponents)
            {
                if (component.malfunctions == 1)
                {
                    //Debug.Log("component " + randomId + "mesh: " + component.GetComponent<MeshCollider>().enabled);
                    //if (component.photonView.IsMine)
                    if (player.GetYourTurn())
                    {
                        //Debug.Log("Player: " + player.nickname + " - comp: " + component.name + " ativado");
                        component.GetComponent<MeshCollider>().enabled = true;
                    }
                    else
                    {
                        //Debug.Log("Player: " + player.nickname + " - comp: " + component.name + " desativado");
                        component.GetComponent<MeshCollider>().enabled = false;
                    }
                    //Debug.Log("component " + randomId + "mesh depois: " + component.GetComponent<MeshCollider>().enabled);
                }
            }
        }

    }

    [PunRPC]
    public void FinishTurn()
    {
        //Debug.Log("Finish turn, time ++");
        time++;
        deckRepair.tag = "Disabled";
        deckEvent.tag = "Disabled";
        timeline.tag = "Disabled";


        //for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        //{
        //    string plateName = "plateName0" + (i + 1);
        //    var findObject = GameObject.Find(plateName);

        //   findObject.GetComponent<MeshCollider>().enabled = true;
        //}


        //photonView.RPC("Turn", RpcTarget.All);

        Turn();

        SetUpComponents();

        

    }

    public void ChangeRepairCardsView(PlayerScript player)
    {
        var repairCards = FindObjectsOfType<RepairCard>();

        //Debug.Log("Player: " + player.nickname);

        foreach (var card in repairCards)
        {
            //Debug.Log("carta: " + card.photonView.ViewID + " -- player: " + player.nickname);
            //Debug.Log(" -- owner: " + card.photonView.OwnerActorNr + " -- " + player.photonView.OwnerActorNr);
            if (card.photonView.OwnerActorNr == player.photonView.OwnerActorNr)
            {
                //Debug.Log("set true");
                card.GetComponent<Animator>().SetBool("sending", false);
                card.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                //Debug.Log("set false");
                card.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    [PunRPC]
    public void ComponentRandom(int id)
    {
        randomId = id;
        //Debug.Log("Random --");

        foreach (var component in timeCraxComponents)
        {
            if (component.componentId == randomId)
            {
                component.AddMalfunction();
            }

        }
    }

    public void ActivateEnd()
    {

        foreach (var player in players)
        {

            if (player.index == time)
            {
                Debug.Log("player " + player.nickname + " está na vez");
                endButton.GetComponent<MeshCollider>().enabled = true;
            }
            else
            {
                Debug.Log("player " + player.nickname + " NÃO está na vez");
                endButton.GetComponent<MeshCollider>().enabled = false;
            }
        }

    }

    public void ActivateFinishButton(bool activate)
    {

        endButton.GetComponent<MeshCollider>().enabled = activate;

    }


    public void BlockActions()
    {
        deckRepair.tag = "Disabled";
        deckEvent.tag = "Disabled";

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            string plateName = "plateName0" + (i + 1);
            var findObject = GameObject.Find(plateName);

            //findObject.GetComponent<MeshCollider>().enabled = false;
            findObject.tag = "Disabled";
        }

        var suitComponents = FindObjectsOfType<Component>();
        foreach (var suitComponent in suitComponents)
        {
            if (suitComponent.malfunctions == 1)
            {
                suitComponent.tag = "Disabled";
            }
        }

        //Button[] components = gameHUD.GetComponentsInChildren<Button>();
        //foreach (Button component in components)
        //{
        //    if (component.name != "QuitGame" && component.name != "FinishTurn")
        //    {
        //        component.interactable = false;
        //    }
        //}
    }

    public void DeactivateAll()
    {
        Debug.Log("Desativando platenames");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            string plateName = "plateName0" + (i + 1);
            var findObject = GameObject.Find(plateName);


            findObject.GetComponent<MeshCollider>().enabled = false;
        }

        Debug.Log("Desativando components");
        var suitComponents = FindObjectsOfType<Component>();
        foreach (var suitComponent in suitComponents)
        {
            if (suitComponent.malfunctions == 1)
            {
                suitComponent.GetComponent<MeshCollider>().enabled = false;
            }
        }

        Debug.Log("Desativando decks");
        deckEvent.GetComponent<MeshCollider>().enabled = false;
        deckRepair.GetComponent<MeshCollider>().enabled = false;
        timeline.GetComponent<MeshCollider>().enabled = false;
        endButton.GetComponent<MeshCollider>().enabled = false;
    }

    public void GiveCard(int numberPlayer)
    {
        //string button = EventSystem.current.currentSelectedGameObject.name;
        //Debug.Log("Nome: " + button);
        //int buttonName = int.Parse(EventSystem.current.currentSelectedGameObject.name);

        photonView.RPC("GiveRepairCard", RpcTarget.All, numberPlayer);

    }

    [PunRPC]
    public void GiveRepairCard(int numberPlayer)
    {

        PlayerScript playerSending = null;
        PlayerScript playerReceiving = null;

        var players = FindObjectsOfType<PlayerScript>();
        foreach (var player in players)
        {
            if (player.GetYourTurn())
            {
                playerSending = player;
            }
            else if (player.index == numberPlayer - 1)
            {
                playerReceiving = player;
            }
        }


        if (playerSending.GetNumberOfRepairsCards() > 0 && playerReceiving.GetNumberOfRepairsCards() < 5)
        {
            BlockActions();

            var repairCards = FindObjectsOfType<RepairCard>();
            List<RepairCard> orderedList = new List<RepairCard>();
            List<RepairCard> playerCards = new List<RepairCard>();

            foreach (var repairCard in repairCards)
            {
                if (repairCard.photonView.OwnerActorNr == playerSending.photonView.OwnerActorNr)
                {
                    Debug.Log(" - " + repairCard.photonView.ViewID);
                    playerCards.Add(repairCard);
                }
            }

            orderedList = playerCards.OrderByDescending(x => x.index).ToList();
            RepairCard lastCard = orderedList[0];

            //Debug.Log("Carta que está sendo passada: " + lastCard.photonView.ViewID);

            //Debug.Log("player recebendo o owner: " + PhotonNetwork.PlayerList[playerReceiving.index].NickName);
            lastCard.photonView.TransferOwnership(PhotonNetwork.PlayerList[playerReceiving.index]);

            //Debug.Log("Recebendo carta: " + playerReceiving.nickname);
            playerReceiving.numberRepairCards++;

            string numberRepairCardsReceiver = "numberRepairCards0" + numberPlayer;
            var findReceiverNumberCards = GameObject.Find(numberRepairCardsReceiver);
            Debug.Log("receiver: " + findReceiverNumberCards.name);

            //int numberOfCardsReceiver = int.Parse(findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text);
            //numberOfCardsReceiver++;

            findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text = playerReceiving.numberRepairCards.ToString();

            //Debug.Log("Dando carta: " + playerSending.nickname);
            playerSending.numberRepairCards--;

            string numberRepairCardsSender = "numberRepairCards0" + (time + 1);
            var findSenderNumberCards = GameObject.Find(numberRepairCardsSender); 
            Debug.Log("sender: " + findSenderNumberCards.name);
            Debug.Log("time + 1: " + (time + 1));

            //int numberOfCardsSender = int.Parse(findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text);
            //Debug.Log("antes -- number of cards sender: " + numberOfCardsSender);
            //numberOfCardsSender--;
            //Debug.Log("depois -- number of cards sender: " + numberOfCardsSender);

            findSenderNumberCards.GetComponent<TextMeshProUGUI>().text = playerSending.numberRepairCards.ToString();

            //Debug.Log("ativando animator");
            lastCard.GetComponent<Animator>().enabled = true;
           // Debug.Log("ativando animação sending");
            lastCard.GetComponent<Animator>().SetBool("sending", true);
        }
        else
        {
            //Debug.Log("Você não possui cartas!");
        }

    }

}
