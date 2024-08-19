using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int randomId;
    private Component[] timeCraxComponents;
    private PlayerScript[] players;
    public GameObject gameInfo;
    public GameObject enviroment;
    public DeckEvent deckEvent;
    public GameObject deckRepair;
    public GameObject timeline;
    public Camera gameCamera;
    public GameObject inputName;
    public GameObject suitTop;
    public GameObject gameHUD;
    public GameObject hud;
    public FinishTurn endButton;
    public GameObject quitButton;
    public GameOver gameOver;
    public Victory victory;
    private int[] playersList;
    private int initialPlayersNumber;
    private int round;
    private int roundCompare;
    private int time;
    private bool gameIsOn = false;
    private List<int> componentList = new List<int>();
    Transform[] componentsWithAnimator = new Transform[20];
    public SoundEffects soundEffects;
    public BackgroundMusic backgroundMusic;
    private PlayerScript[] orderedPlayers;
    public Material plateNameMaterial;
    public Material plateNameMaterial2;
    public GameObject playerLeftBackground;
    public RandomMaterial randomMaterial;



    private void Awake()
    {
        //inputName.SetActive(false);
        //PhotonNetwork.Instantiate("Player", new Vector3(7.224f, 1.01f, 0.83f), Quaternion.identity);
        //playersList = new int[PhotonNetwork.PlayerList.Length];
    }

    void Update()
    {
        if (gameIsOn && PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerList.Length != initialPlayersNumber)
            {
                initialPlayersNumber = PhotonNetwork.PlayerList.Length;

                int plateNameIndex = 0;

                int maxPlayerNumber = PhotonNetwork.PlayerList.Length;
                while (maxPlayerNumber > 0)
                {
                    Debug.Log("while maxplayerNumber: " + maxPlayerNumber);
                    foreach (var player in players)
                    {
                        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                        {
                            Debug.Log("playerNickname: " + player.nickname + " ---  PN_Nickname: "+ PhotonNetwork.PlayerList[i].NickName);
                            if (player.nickname != PhotonNetwork.PlayerList[i].NickName)
                            {
                                Debug.Log("maxplayerNumber --: " + maxPlayerNumber);
                                maxPlayerNumber--;
                            }
                        }

                        if(maxPlayerNumber == 0)
                        {
                            Debug.Log("ENTROU maxplayerNumber: " + maxPlayerNumber);
                            plateNameIndex = player.plateNameIndex;
                            Debug.Log("plateNameIndex: " + plateNameIndex);

                            break;
                        }
                        else
                        {
                            Debug.Log("RESETANDO maxplayerNumber: " + maxPlayerNumber);
                            maxPlayerNumber = PhotonNetwork.PlayerList.Length;
                        }

                    }

                }

                Debug.Log("numberPlayers: "+PhotonNetwork.PlayerList.Length+" --  initialNumber: "+initialPlayersNumber);
                photonView.RPC("RemovePlayersPlatenames", RpcTarget.All, plateNameIndex);
                //photonView.RPC("UpdatePlayersIndex", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void UpdatePlayersIndex()
    {

        Debug.Log("Resetando Index");

        players = FindObjectsOfType<PlayerScript>();

        foreach (var player in players)
        {
            Debug.Log("player name: "+player.nickname);
            player.UpdateIndex();
        }
    }


    [PunRPC]
    public void RemovePlayersPlatenames(int index)
    {

        for(int i = 0; i < orderedPlayers.Length; i++)
        {
            if (orderedPlayers[i]?.plateNameIndex == index)
            {
                Debug.Log("Removendo " + orderedPlayers[i].nickname);
                orderedPlayers[i] = null;
            }
        }

        index++;

        Debug.Log("Removendo platename");

        string plateName = "plateName0" + index;
        var plate = GameObject.Find(plateName);
        Debug.Log("plate name: " + plate.name);

        plate.GetComponent<MeshRenderer>().enabled = false;
        plate.GetComponent<MeshCollider>().enabled = false;

        string repairSymbolName = "repairCardSymbol0" + index;
        var repairSymbol = GameObject.Find(repairSymbolName);
        Debug.Log("repairSymbol name: " + repairSymbol.name);

        repairSymbol.GetComponent<SpriteRenderer>().enabled = false;

        string namePlateText = "namePlayer0" + index;
        var namePlate = GameObject.Find(namePlateText);
        Debug.Log("namePlate name: " + namePlate.name);

        namePlate.GetComponent<TMP_Text>().text = " ";
        namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);

        string numberRepairCardText = "numberRepairCards0" + index;
        var numberRepairCard = GameObject.Find(numberRepairCardText);
        Debug.Log("repairCardSymbol name: " + numberRepairCard.name);

        numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";
    }

    void Start()
    {
        //Debug.Log("Start()");

        //timeCraxComponents = FindObjectsOfType<Component>();

        //gameCamera.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);

        //int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        //componentList.AddRange(numbers);

        //for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        //{
        //    playersList[i] = PhotonNetwork.PlayerList[i].ActorNumber;

        //}
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    Invoke("StartGame", 6f);
        //}
    }

    //[PunRPC]
    public void GetRandomEventCards(string theme)
    {
        randomMaterial.GetRandomMaterial(theme);

    }

    public void StartNewGame()
    {
        string theme = PhotonNetwork.CurrentRoom.CustomProperties["the"].ToString();

        Debug.Log("-------------  THEME: " + theme);

        int index = 0;
        gameIsOn = true;
        //gameOver.gameIsOver = false;

        //Lista de todos os componentes com o Script Component
        timeCraxComponents = FindObjectsOfType<Component>();

        Transform[] components = enviroment.GetComponentsInChildren<Transform>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].CompareTag("Component"))
            {
                //Lista de todos os componentes com a tag Componet (Ou seja, possui Animator)
                components[i].GetComponent<Animator>().enabled = true;
                componentsWithAnimator[index] = components[i];
                index++;
            }
        }

        round = 1;
        roundCompare = 1;
        time = 0;

        //inputName.SetActive(false);
        PhotonNetwork.Instantiate("Player", new Vector3(7.224f, 1.01f, 0.83f), Quaternion.identity);
        playersList = new int[PhotonNetwork.PlayerList.Length];
        initialPlayersNumber = PhotonNetwork.PlayerList.Length;

        Debug.Log("Starting new game");

        gameCamera.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);

        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        componentList.Clear();
        componentList.AddRange(numbers);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playersList[i] = PhotonNetwork.PlayerList[i].ActorNumber;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            //photonView.RPC("GetRandomEventCards", RpcTarget.All, theme);
            randomMaterial.GetRandomMaterial(theme);
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

        Debug.Log("player list lenght: " + PhotonNetwork.PlayerList.Length);

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
        players = FindObjectsOfType<PlayerScript>();
        Debug.Log("players lenght: " + players.Length);

        orderedPlayers = new PlayerScript[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log("player name: " + players[i].nickname);
            if (players[i].index == 0)
            {
                orderedPlayers[0] = players[i];
            }
            else if (players[i].index == 1)
            {
                orderedPlayers[1] = players[i];
            }
            else if (players[i].index == 2)
            {
                orderedPlayers[2] = players[i];
            }
            else if (players[i].index == 3)
            {
                orderedPlayers[3] = players[i];
            }
        }

        //Debug.Log("FirstTurn()");
        //photonView.RPC("Turn", RpcTarget.All);
        Turn();
    }

    public bool CheckTimeAndIndex(PlayerScript[] orderedPlayers)
    {
        for (int i = 0; i < orderedPlayers.Length; i++)
        {
            Debug.Log("ORDERED -- player name: " + orderedPlayers[i]?.nickname);
            if (orderedPlayers[i]?.index == time)
            {
                Debug.Log("Entrou -- return true");
                return true;
            }
        }

        time ++;

        if(time == 4)
        {
            return true;
        }

        return false;
    }

    //[PunRPC]
    public void Turn()
    {

        for (int i = 0; i < orderedPlayers.Length; i++)
        {
            Debug.Log("ordered pos " + i + "  -  name: " + orderedPlayers[i]?.nickname);
        }

        Debug.Log("Entrou no turn - time: " + time);

        int numberOfPlayers = 4;

        //if(playerLeftGame)
        //{
        //    numberOfPlayers--;
        //}

        players = FindObjectsOfType<PlayerScript>();

        bool checkTime = false;

        while (!checkTime)
        {
            checkTime = CheckTimeAndIndex(orderedPlayers);
        }


        Debug.Log("5 -- Turn()");
        Debug.Log("time: " + time + " < numPlayers: " + numberOfPlayers);
        if (time < numberOfPlayers)
        {
            //Button[] components = gameHUD.GetComponentsInChildren<Button>();
            //int indexPlayer = time + 1;

            foreach (var player in players)
            {
                //Debug.Log("time: " + time);
                //if(time == 0)
                //{
                //    Debug.Log("Resetando Index");
                //    player.UpdateIndex();
                //}

                if (player.index == time)
                {
                    //Debug.Log("agora é o turno de : " + player.nickname);
                    player.SetYourTurn(true);

                    ChangeRepairCardsView(player);

                    //ChangePlateNameMaterial(player.plateNameIndex);
                    photonView.RPC("ChangePlateNameMaterial", RpcTarget.All, player.plateNameIndex);

                    //foreach (Button component in components)
                    //{
                    //    //Debug.Log("name: " + component.name + " - time+1:" + indexPlayer);
                    //    if (component.name == indexPlayer.ToString())
                    //    {
                    //        component.interactable = false;
                    //    }
                    //    else
                    //    {
                    //        component.interactable = true;
                    //    }

                    //}
                }
                else
                {
                    //Debug.Log("não é turno de : " + player.nickname);
                    player.SetYourTurn(false);

                    //foreach (Button component in components)
                    //{
                    //    if (!(component.name == "QuitGame"))
                    //    {
                    //        component.interactable = false;
                    //    }

                    //}
                }
            }

            ShowRoundInfo();

        }
        else
        {
            Debug.Log("Caiu no else");
            time = 0;
            round++;
            Turn();
        }

    }

    [PunRPC]
    public void ChangePlateNameMaterial(int plateNameIndex)
    {
        string plateNameText = "plateName0" + (plateNameIndex + 1);

        var plateNames = FindObjectsOfType<GiveCards>();

        foreach (GiveCards plateName in plateNames)
        {
            if(plateName.name == plateNameText)
            {
                plateName.GetComponent<MeshRenderer>().material = plateNameMaterial2;
            }
            else
            {
                plateName.GetComponent<MeshRenderer>().material = plateNameMaterial;
            }
        }
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
                    info.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = orderedPlayers[time].nickname +"'s Turn";
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
                    info.GetComponentInChildren<TextMeshProUGUI>().text = orderedPlayers[time].nickname + "'s Turn";
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

        Debug.Log("tamanho da lista: " + PhotonNetwork.PlayerList.Length);
        Debug.Log("Time: " + time);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log("player na vez: " + orderedPlayers[time]?.nickname + " -- player: "+ PhotonNetwork.PlayerList[i].NickName);
            if (orderedPlayers[time].actorNumber == PhotonNetwork.PlayerList[i].ActorNumber)
            {

                Debug.Log("Jogador: " + orderedPlayers[time]?.nickname + " está na vez  -- recebendo photon views");
                deckEvent.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                deckRepair.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                timeline.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                endButton.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);

                var plateNames = FindObjectsOfType<GiveCards>();

                foreach (GiveCards plateName in plateNames)
                {
                    Debug.Log("transferindo platename: "+plateName.name);
                    plateName.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                }

                foreach (var component in timeCraxComponents)
                {
                    component.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                }
            }
        }

        //for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        //{
        //    string plateName = "plateName0" + (i + 1);
        //    var findObject = GameObject.Find(plateName);

        //    findObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        //}

        //foreach (var component in timeCraxComponents)
        //{
        //    component.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
        //}

        foreach (var player in players)
        {
            //player.UpdateIndex();
            Debug.Log("jogador " + player.name + " index: " + player.index + " -----  time: " + time);
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

                Debug.Log("Ativando MeshCollider dos objetos");
                endButton.GetComponent<MeshCollider>().enabled = true;
                quitButton.GetComponent<MeshCollider>().enabled = true;

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
        randomId = UnityEngine.Random.Range(1, componentList.Count + 1);
        Debug.Log("result: " + randomId);

        //photonView.RPC("RandomAnimator", RpcTarget.All);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        photonView.RPC("ComponentRandom", RpcTarget.All, randomId);

    }

    [PunRPC]
    public void ComponentRandom(int id)
    {
        randomId = id;

        StartCoroutine(Roulettecomponent());

    }

    [PunRPC]
    private IEnumerator Roulettecomponent()
    {
        int randomIndex = 0;
        int cond = 0;
        float interval = 0.3f;

        while(cond < 15)
        {
            int index = UnityEngine.Random.Range(0, 15);
            while(index == randomIndex)
            {
                index = UnityEngine.Random.Range(0, 15);
            }
            randomIndex = index;
            //Debug.Log("Random: " + index);
            //Debug.Log("Ativando outline do component: " + timeCraxComponents[index].name);
            timeCraxComponents[index].GetComponent<OutlineComponent>().enabled = true;
            soundEffects.PlayRouletteSound();
            yield return new WaitForSeconds(interval);

            //Debug.Log("Destivando outline do component: " + timeCraxComponents[index].name);
            timeCraxComponents[index].GetComponent<OutlineComponent>().enabled = false;

            cond++;
            interval -= 0.015f;
        }

        //Debug.Log("Random: " + (randomId - 1));

        //Debug.Log("Ativando outline do component: " + timeCraxComponents[(randomId - 1)].name);
        timeCraxComponents[(randomId - 1)].GetComponent<OutlineComponent>().enabled = true;
        soundEffects.PlayRouletteSound();
        yield return new WaitForSeconds(interval);

        //Debug.Log("Destivando outline do component: " + timeCraxComponents[(randomId - 1)].name);
        timeCraxComponents[(randomId - 1)].GetComponent<OutlineComponent>().enabled = false;

        AddMalfunctionInComponent();
    }

    public void AddMalfunctionInComponent()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var component in timeCraxComponents)
        {
            if (component.componentId == randomId)
            {
                component.AddMalfunction();
            }
        }
    }

    public void EndTurn()
    {
        float waiting;
        waiting = 0;

        int lastPlayerIndex = 3;

        Debug.Log("orderedPlayers.Length: " + orderedPlayers.Length);
        for(int i = orderedPlayers.Length - 1; i >= 0; i--)
        {
            Debug.Log("-- "+orderedPlayers[i]);
            if(orderedPlayers[i] != null)
            {
                Debug.Log("ultimo player é: " + orderedPlayers[i].nickname + " com index: " + orderedPlayers[i].index);
                lastPlayerIndex = orderedPlayers[i].index;
                break;
            }
        }
        //if (time == PhotonNetwork.PlayerList.Length - 1)
        Debug.Log("Ultimo do Round? -- Time: " + time + " - index: " + lastPlayerIndex);
        if (time == lastPlayerIndex)
        {
            Debug.Log("Random Malfunction");
            RandomComponentNumber();
            waiting = 4;
        }

        Invoke("WaitForFinishTurn", waiting);

    }

    public void WaitForFinishTurn() 
    {
        //gameOver = GameObject.FindGameObjectWithTag("GameOver");
        //victory = GameObject.FindGameObjectWithTag("Victory");

        Debug.Log("child name: "+ gameOver.transform.GetChild(0).gameObject.name);
        if (!gameOver.transform.GetChild(0).gameObject.activeInHierarchy || !victory.transform.GetChild(0).gameObject.activeInHierarchy)
        {
            Debug.Log("Gameover or Victory is NOT active");
            photonView.RPC("FinishTurn", RpcTarget.All);
        }

    }

    public void SetUpComponents()
    {
        foreach (var player in players)
        {
            Debug.Log("6 -- Player --: " + player.nickname);
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
        Debug.Log("4 -- Finish turn, time ++");
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

    public void CheckQuitGamePlayer()
    {

        players = FindObjectsOfType<PlayerScript>();

        foreach (var player in players)
        {
            Debug.Log("2 -- LocalPlayer ActrNumber: " + PhotonNetwork.LocalPlayer?.ActorNumber + " --- Photon ActrNumber: " + player?.photonView.ControllerActorNr);
            if (PhotonNetwork.LocalPlayer?.ActorNumber == player?.photonView.ControllerActorNr)
            {

                //photonView.RPC("RemovePlateName", RpcTarget.All, player.plateNameIndex);

                Debug.Log("Chamando ShowLeftPlayer");
                photonView.RPC("ShowLeftPlayerInfo", RpcTarget.Others, player.nickname);


                if (player.GetYourTurn())
                {
                    Debug.Log("numero de players: " + PhotonNetwork.PlayerList.Length);
                    if(PhotonNetwork.PlayerList.Length != 1)
                    {
                        //return true;
                        photonView.RPC("FinishTurn", RpcTarget.All);
                    }
                    //else
                    //{
                        //photonView.RPC("FinishTurn", RpcTarget.All);
                        //return true;
                    //}

                }
            }
        }

        //return false;
    }

    [PunRPC]
    public void ShowLeftPlayerInfo(string nickname)
    {
        Debug.Log("Entrou no ShowLeftPlayer");

        gameInfo.gameObject.SetActive(true);
        playerLeftBackground.GetComponentInChildren<TMP_Text>().text = nickname + " left the game";
        playerLeftBackground.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);

        Invoke("HideLeftPlayerInfo", 1.5f);
    }

    public void HideLeftPlayerInfo()
    {
        Debug.Log("Entrou no HideLeftPlayer");
        playerLeftBackground.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
        Invoke("DisableOnlyGameInfo", 0.5f);
    }

    public void DisableOnlyGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
    }

    public void BackToMenu()
    {

        //bool isTurn = CheckQuitGamePlayer();

        //if (isTurn)
        //{
        //    Debug.Log("7 -- Estou no turno");
        //    Invoke("SetUpBackToMenu", 0.5f);
        //}
        //else
        //{
        //    SetUpBackToMenu();
        //}

        CheckQuitGamePlayer();

        Invoke("SetUpBackToMenu", 0.7f);

    }

    //[PunRPC]
    //public void RemovePlateName(int index)
    //{
    //    Debug.Log("3 -- Removendo platenames");

    //    string plateName = "plateName0" + (index + 1);
    //    var plate = GameObject.Find(plateName);
    //    Debug.Log("plate name: " + plate.name);

    //    plate.GetComponent<MeshRenderer>().enabled = false;
    //    plate.GetComponent<MeshCollider>().enabled = false;

    //    string repairSymbolName = "repairCardSymbol0" + (index + 1);
    //    var repairSymbol = GameObject.Find(repairSymbolName);
    //    Debug.Log("repairSymbol name: "+repairSymbol.name);

    //    repairSymbol.GetComponent<SpriteRenderer>().enabled = false;

    //    string namePlateText = "namePlayer0" + (index + 1);
    //    var namePlate = GameObject.Find(namePlateText);
    //    Debug.Log("namePlate name: " + namePlate.name);

    //    namePlate.GetComponent<TMP_Text>().text = " ";
    //    namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);

    //    string numberRepairCardText = "numberRepairCards0" + (index + 1);
    //    var numberRepairCard = GameObject.Find(numberRepairCardText);
    //    Debug.Log("repairCardSymbol name: " + numberRepairCard.name);

    //    numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";

    //}

    public void SetUpBackToMenu()
    {
        Debug.Log("8 -- SeUPBackToMenu");

        backgroundMusic.PlayMenuSound();

        gameIsOn = false;

        DeactivateAll();
        ResetAllComponents();
        ResetAllPlatenames();

        deckEvent.ResetAllEventCards();

        var gameConnection = FindObjectOfType<GameConnection>();
        gameConnection.OnLeftRoom();
        gameConnection.DisconectAndReconect();

        suitTop.GetComponent<Animator>().enabled = true;
        suitTop.GetComponent<Animator>().SetBool("openSuit", false);
    }

    public void ResetAllPlatenames()
    {
        Debug.Log("11 -- Resetando platenames");
        for (int i = 0; i < 4; i++)
        {
            string plateName = "plateName0" + (i + 1);
            var plate = GameObject.Find(plateName);
            Debug.Log("plate name: "+plate.name);


            plate.GetComponent<MeshRenderer>().enabled = false;
            plate.GetComponent<MeshCollider>().enabled = false;

            string repairSymbolName = "repairCardSymbol0" + (i + 1);
            var repairSymbol = GameObject.Find(repairSymbolName);
            Debug.Log("repairSymbol name: "+repairSymbol.name); 


            repairSymbol.GetComponent<SpriteRenderer>().enabled = false;

            string namePlateText = "namePlayer0" + (i + 1);
            var namePlate = GameObject.Find(namePlateText);
            Debug.Log("namePlate name: " + namePlate.name);

            namePlate.GetComponent<TMP_Text>().text = " ";
            namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);

            string numberRepairCardText = "numberRepairCards0" + (i + 1);
            var numberRepairCard = GameObject.Find(numberRepairCardText);
            Debug.Log("repairCardSymbol name: " + numberRepairCard.name);


            numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";

        }
    }
    public void ResetAllComponents()
    {
        Debug.Log("10 -- ResetAllComponents");

        foreach (var component in timeCraxComponents)
        {
            component.malfunctions = 0;
        }

        foreach (var component in componentsWithAnimator)
        {

            Debug.Log("opcName: " + component.name);
            component.GetComponent<Animator>().SetBool("malfunction", false);
            component.GetComponent<Animator>().enabled = false;

            ParticleSystem[] effects = component.GetComponentsInChildren<ParticleSystem>(true);

            foreach (var effect in effects)
            {
                effect.gameObject.SetActive(false);
            }

        }

        //Transform[] opcoes = enviroment.GetComponentsInChildren<Transform>();
        //foreach (var opc in opcoes)
        //{
        //    if (opc.GetComponent<Animator>())
        //    {
        //        Debug.Log("opcName: "+opc.gameObject.name);
        //        opc.GetComponent<Animator>().SetBool("malfunction", false);

        //        ParticleSystem effect = opc.GetComponentInChildren<ParticleSystem>(true);
        //        effect.gameObject.SetActive(false);
        //    }
        //}
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
        Debug.Log("9 -- Desativando platenames");
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
            if (suitComponent.malfunctions > 0)
            {
                suitComponent.GetComponent<MeshCollider>().enabled = false;
            }
        }

        Debug.Log("Desativando decks");
        deckEvent.GetComponent<MeshCollider>().enabled = false;
        deckRepair.GetComponent<MeshCollider>().enabled = false;
        timeline.GetComponent<MeshCollider>().enabled = false;
        endButton.GetComponent<MeshCollider>().enabled = false;
        quitButton.GetComponent<MeshCollider>().enabled = false;
        
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
