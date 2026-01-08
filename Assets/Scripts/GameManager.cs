using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using System.Collections;
using TimeCrax.Core;
using TimeCrax.Themes;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int randomId;
    private MachineComponent[] timeCraxComponents;
    private PlayerScript[] players;
    public GameObject gameInfo;
    public GameObject enviroment;
    public DeckEvent deckEvent;
    public GameObject deckRepair;
    public GameObject timeline;
    public CameraController gameCamera;
    public GameObject inputName;
    public GameObject suitTop;
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
                    DebugHelper.Log("while maxplayerNumber: " + maxPlayerNumber);
                    foreach (var player in players)
                    {
                        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                        {
                            DebugHelper.Log("playerNickname: " + player.nickname + " ---  PN_Nickname: "+ PhotonNetwork.PlayerList[i].NickName);
                            if (player.nickname != PhotonNetwork.PlayerList[i].NickName)
                            {
                                DebugHelper.Log("maxplayerNumber --: " + maxPlayerNumber);
                                maxPlayerNumber--;
                            }
                        }

                        if(maxPlayerNumber == 0)
                        {
                            DebugHelper.Log("ENTROU maxplayerNumber: " + maxPlayerNumber);
                            plateNameIndex = player.plateNameIndex;
                            DebugHelper.Log("plateNameIndex: " + plateNameIndex);

                            break;
                        }
                        else
                        {
                            DebugHelper.Log("RESETANDO maxplayerNumber: " + maxPlayerNumber);
                            maxPlayerNumber = PhotonNetwork.PlayerList.Length;
                        }

                    }

                }

                DebugHelper.Log("numberPlayers: "+PhotonNetwork.PlayerList.Length+" --  initialNumber: "+initialPlayersNumber);
                photonView.RPC("RemovePlayersPlatenames", RpcTarget.All, plateNameIndex);
                //photonView.RPC("UpdatePlayersIndex", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void UpdatePlayersIndex()
    {

        DebugHelper.Log("Resetando Index");

        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            DebugHelper.Log("player name: "+player.nickname);
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
                DebugHelper.Log("Removendo " + orderedPlayers[i].nickname);
                orderedPlayers[i] = null;
            }
        }

        index++;

        DebugHelper.Log("Removendo platename");

        string plateName = "plateName0" + index;
        var plate = GameObject.Find(plateName);
        DebugHelper.Log("plate name: " + plate?.name);

        plate.GetComponent<MeshRenderer>().enabled = false;
        plate.GetComponent<MeshCollider>().enabled = false;

        string repairSymbolName = "repairCardSymbol0" + index;
        var repairSymbol = GameObject.Find(repairSymbolName);
        DebugHelper.Log("repairSymbol name: " + repairSymbol?.name);

        repairSymbol.GetComponent<SpriteRenderer>().enabled = false;

        string namePlateText = "namePlayer0" + index;
        var namePlate = GameObject.Find(namePlateText);
        DebugHelper.Log("namePlate name: " + namePlate?.name);

        namePlate.GetComponent<TMP_Text>().text = " ";
        namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);

        string numberRepairCardText = "numberRepairCards0" + index;
        var numberRepairCard = GameObject.Find(numberRepairCardText);
        DebugHelper.Log("repairCardSymbol name: " + numberRepairCard?.name);

        numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";
    }

    void Start()
    {
        //DebugHelper.Log("Start()");

        //timeCraxComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);

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

        DebugHelper.Log("-------------  THEME: " + theme);

        int index = 0;
        gameIsOn = true;
        //gameOver.gameIsOver = false;

        //Lista de todos os componentes com o Script Component
        timeCraxComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);

        Transform[] components = enviroment.GetComponentsInChildren<Transform>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].CompareTag("Component"))
            {
                //Lista de todos os componentes com a tag Componet (Ou seja, possui Animator)
                DebugHelper.Log("Ativando animator do componente " + components[i].name);
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

        DebugHelper.Log("Starting new game");

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
            // Verificar se há tema da API selecionado
            if (ThemeManager.Instance != null && ThemeManager.Instance.HasSelectedTheme)
            {
                // Novo sistema: usar tema da API
                var selectedTheme = ThemeManager.Instance.SelectedTheme;
                DebugHelper.Log($"[GameManager] Usando tema da API: {selectedTheme.name}");

                randomMaterial.InitializeForTheme(selectedTheme);
                randomMaterial.GetRandomMaterialFromTheme();
            }
            else
            {
                // Sistema legado: usar temas pré-definidos
                DebugHelper.Log($"[GameManager] Usando tema legado: {theme}");
                randomMaterial.GetRandomMaterial(theme);
            }

            this.DelayedCall(6f, StartGame);
        }
    }

    public void StartGame()
    {

        DebugHelper.Log("StartGame()");
        photonView.RPC("ShowHUD", RpcTarget.All);
        //ShowHUD();
    }

    [PunRPC]
    public void ShowHUD()
    {

        hud.SetActive(true);
        var outline = FindFirstObjectByType<OutlineAction>();
        outline.MakeObjectsSelectable();

        var components = hud.GetComponentsInChildren<Transform>();

        string plateName = "plateName0";
        string namePlayer = "namePlayer0";
        string repairCardSymbol = "repairCardSymbol0";
        string numberRepairCards = "numberRepairCards0";

        DebugHelper.Log("player list lenght: " + PhotonNetwork.PlayerList.Length);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int name = i + 1;
            for (int x = 0; x < components.Length; x++)
            {
                //DebugHelper.Log("component name:" + components[x].name+" - name: "+ plateName+name.ToString());
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
        this.DelayedCall(2f, FirstTurn);
    }
    public void FirstTurn()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        DebugHelper.Log("players lenght: " + players.Length);

        orderedPlayers = new PlayerScript[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            DebugHelper.Log("player name: " + players[i].nickname);
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

        //DebugHelper.Log("FirstTurn()");
        //photonView.RPC("Turn", RpcTarget.All);
        Turn();
    }

    public bool CheckTimeAndIndex(PlayerScript[] orderedPlayers)
    {
        for (int i = 0; i < orderedPlayers.Length; i++)
        {
            DebugHelper.Log("ORDERED -- player name: " + orderedPlayers[i]?.nickname);
            if (orderedPlayers[i]?.index == time)
            {
                DebugHelper.Log("Entrou -- return true");
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
            DebugHelper.Log("ordered pos " + i + "  -  name: " + orderedPlayers[i]?.nickname);
        }

        DebugHelper.Log("Entrou no turn - time: " + time);

        int numberOfPlayers = 4;

        //if(playerLeftGame)
        //{
        //    numberOfPlayers--;
        //}

        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        bool checkTime = false;

        while (!checkTime)
        {
            checkTime = CheckTimeAndIndex(orderedPlayers);
        }


        DebugHelper.Log("5 -- Turn()");
        DebugHelper.Log("time: " + time + " < numPlayers: " + numberOfPlayers);
        if (time < numberOfPlayers)
        {
            //Button[] components = gameHUD.GetComponentsInChildren<Button>();
            //int indexPlayer = time + 1;

            foreach (var player in players)
            {
                //DebugHelper.Log("time: " + time);
                //if(time == 0)
                //{
                //    DebugHelper.Log("Resetando Index");
                //    player.UpdateIndex();
                //}

                if (player.index == time)
                {
                    //DebugHelper.Log("agora � o turno de : " + player.nickname);
                    player.SetYourTurn(true);

                    ChangeRepairCardsView(player);

                    //ChangePlateNameMaterial(player.plateNameIndex);
                    photonView.RPC("ChangePlateNameMaterial", RpcTarget.All, player.plateNameIndex);

                    //foreach (Button component in components)
                    //{
                    //    //DebugHelper.Log("name: " + component.name + " - time+1:" + indexPlayer);
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
                    //DebugHelper.Log("n�o � turno de : " + player.nickname);
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
            DebugHelper.Log("Caiu no else");
            time = 0;
            round++;
            Turn();
        }

    }

    [PunRPC]
    public void ChangePlateNameMaterial(int plateNameIndex)
    {
        string plateNameText = "plateName0" + (plateNameIndex + 1);

        var plateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);

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
        //DebugHelper.Log("ShowRoundInfo()");
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
        this.DelayedCall(1.5f, HideRoundInfo);
    }

    public void HideRoundInfo()
    {
        //DebugHelper.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        //DebugHelper.Log("DisableGameInfo()");
        gameInfo.gameObject.SetActive(false);
        StartTurn();
    }

    public void StartTurn()
    {
        //Button[] components = gameHUD.GetComponentsInChildren<Button>();

        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        DebugHelper.Log("tamanho da lista: " + PhotonNetwork.PlayerList.Length);
        DebugHelper.Log("Time: " + time);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            DebugHelper.Log("player na vez: " + orderedPlayers[time]?.nickname + " -- player: "+ PhotonNetwork.PlayerList[i].NickName);
            if (orderedPlayers[time].actorNumber == PhotonNetwork.PlayerList[i].ActorNumber)
            {

                DebugHelper.Log("Jogador: " + orderedPlayers[time]?.nickname + " est� na vez  -- recebendo photon views");
                deckEvent.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                deckRepair.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                timeline.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);
                endButton.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[i]);

                var plateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);

                foreach (GiveCards plateName in plateNames)
                {
                    DebugHelper.Log("transferindo platename: "+plateName.name);
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
            DebugHelper.Log("jogador " + player.name + " index: " + player.index + " -----  time: " + time);
            if (player.index == time)
            {

                if (player.GetNumberOfRepairsCards() == 5)
                {
                    //DebugHelper.Log("tem 5 cartas");
                    deckRepair.tag = "Disabled";
                }
                else
                {
                    //DebugHelper.Log("nao tem 5 cartas");
                    deckRepair.tag = "Selectable";
                }

                foreach (var timeCraxComponent in timeCraxComponents)
                {
                    if (timeCraxComponent.malfunctions == 1)
                    {
                        timeCraxComponent.tag = "Selectable";
                    }
                }

                DebugHelper.Log("Ativando MeshCollider dos objetos");
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
                //DebugHelper.Log("4 -- ");
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
        //DebugHelper.Log("7 --");
        randomId = UnityEngine.Random.Range(1, componentList.Count + 1);
        DebugHelper.Log("result: " + randomId);

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
            //DebugHelper.Log("Random: " + index);
            //DebugHelper.Log("Ativando outline do component: " + timeCraxComponents[index].name);
            timeCraxComponents[index].GetComponent<OutlineComponent>().enabled = true;
            soundEffects.PlayRouletteSound();
            yield return new WaitForSeconds(interval);

            //DebugHelper.Log("Destivando outline do component: " + timeCraxComponents[index].name);
            timeCraxComponents[index].GetComponent<OutlineComponent>().enabled = false;

            cond++;
            interval -= 0.015f;
        }

        //DebugHelper.Log("Random: " + (randomId - 1));

        //DebugHelper.Log("Ativando outline do component: " + timeCraxComponents[(randomId - 1)].name);
        timeCraxComponents[(randomId - 1)].GetComponent<OutlineComponent>().enabled = true;
        soundEffects.PlayRouletteSound();
        yield return new WaitForSeconds(interval);

        //DebugHelper.Log("Destivando outline do component: " + timeCraxComponents[(randomId - 1)].name);
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

        DebugHelper.Log("orderedPlayers.Length: " + orderedPlayers.Length);
        for(int i = orderedPlayers.Length - 1; i >= 0; i--)
        {
            DebugHelper.Log("-- "+orderedPlayers[i]);
            if(orderedPlayers[i] != null)
            {
                DebugHelper.Log("ultimo player �: " + orderedPlayers[i].nickname + " com index: " + orderedPlayers[i].index);
                lastPlayerIndex = orderedPlayers[i].index;
                break;
            }
        }

        //if (time == PhotonNetwork.PlayerList.Length - 1)
        DebugHelper.Log("Ultimo do Round? -- Time: " + time + " - index: " + lastPlayerIndex);
        if (time == lastPlayerIndex)
        {
            DebugHelper.Log("Random Malfunction");
            RandomComponentNumber();
            waiting = 4;
            
        }
        //else
        //{
        //    DebugHelper.Log("N�o � o ultimo da rodada");
        //    Invoke("WaitForFinishTurn", waiting);
        //}

        this.DelayedCall(waiting, WaitForFinishTurn);

    }

    public void WaitForFinishTurn() 
    {

        DebugHelper.Log(" -------------------->>>>>  game is over?: "+ gameOver.gameIsOver);
        if (!gameOver.gameIsOver)
        {
            DebugHelper.Log("Game is ON");
            photonView.RPC("FinishTurn", RpcTarget.All);
        }

    }

    public void SetUpComponents()
    {
        foreach (var player in players)
        {
            DebugHelper.Log("6 -- Player --: " + player.nickname);
            foreach (var component in timeCraxComponents)
            {
                if (component.malfunctions == 1)
                {
                    //DebugHelper.Log("component " + randomId + "mesh: " + component.GetComponent<MeshCollider>().enabled);
                    //if (component.photonView.IsMine)
                    if (player.GetYourTurn())
                    {
                        //DebugHelper.Log("Player: " + player.nickname + " - comp: " + component.name + " ativado");
                        component.GetComponent<MeshCollider>().enabled = true;
                    }
                    else
                    {
                        //DebugHelper.Log("Player: " + player.nickname + " - comp: " + component.name + " desativado");
                        component.GetComponent<MeshCollider>().enabled = false;
                    }
                    //DebugHelper.Log("component " + randomId + "mesh depois: " + component.GetComponent<MeshCollider>().enabled);
                }
            }
        }

    }

    [PunRPC]
    public void FinishTurn()
    {
        DebugHelper.Log("4 -- Finish turn, time ++");
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
        var repairCards = FindObjectsByType<RepairCard>(FindObjectsSortMode.None);

        //DebugHelper.Log("Player: " + player.nickname);

        foreach (var card in repairCards)
        {
            //DebugHelper.Log("carta: " + card.photonView.ViewID + " -- player: " + player.nickname);
            //DebugHelper.Log(" -- owner: " + card.photonView.OwnerActorNr + " -- " + player.photonView.OwnerActorNr);
            if (card.photonView.OwnerActorNr == player.photonView.OwnerActorNr)
            {
                //DebugHelper.Log("set true");
                card.GetComponent<Animator>().SetBool("sending", false);
                card.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                //DebugHelper.Log("set false");
                card.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void CheckQuitGamePlayer()
    {

        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            DebugHelper.Log("2 -- LocalPlayer ActrNumber: " + PhotonNetwork.LocalPlayer?.ActorNumber + " --- Photon ActrNumber: " + player?.photonView.ControllerActorNr);
            if (PhotonNetwork.LocalPlayer?.ActorNumber == player?.photonView.ControllerActorNr && !gameOver.gameIsOver)
            {

                //photonView.RPC("RemovePlateName", RpcTarget.All, player.plateNameIndex);

                DebugHelper.Log("Chamando ShowLeftPlayer");
                photonView.RPC("ShowLeftPlayerInfo", RpcTarget.Others, player.nickname);


                if (player.GetYourTurn())
                {
                    DebugHelper.Log("numero de players: " + PhotonNetwork.PlayerList.Length);
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
        DebugHelper.Log("Entrou no ShowLeftPlayer");

        gameInfo.gameObject.SetActive(true);
        playerLeftBackground.GetComponentInChildren<TMP_Text>().text = nickname + " left the game";
        playerLeftBackground.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);

        this.DelayedCall(1.5f, HideLeftPlayerInfo);
    }

    public void HideLeftPlayerInfo()
    {
        DebugHelper.Log("Entrou no HideLeftPlayer");
        playerLeftBackground.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
        this.DelayedCall(0.5f, DisableOnlyGameInfo);
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
        //    DebugHelper.Log("7 -- Estou no turno");
        //    Invoke("SetUpBackToMenu", 0.5f);
        //}
        //else
        //{
        //    SetUpBackToMenu();
        //}

        CheckQuitGamePlayer();

        this.DelayedCall(0.7f, SetUpBackToMenu);

    }

    //[PunRPC]
    //public void RemovePlateName(int index)
    //{
    //    DebugHelper.Log("3 -- Removendo platenames");

    //    string plateName = "plateName0" + (index + 1);
    //    var plate = GameObject.Find(plateName);
    //    DebugHelper.Log("plate name: " + plate.name);

    //    plate.GetComponent<MeshRenderer>().enabled = false;
    //    plate.GetComponent<MeshCollider>().enabled = false;

    //    string repairSymbolName = "repairCardSymbol0" + (index + 1);
    //    var repairSymbol = GameObject.Find(repairSymbolName);
    //    DebugHelper.Log("repairSymbol name: "+repairSymbol.name);

    //    repairSymbol.GetComponent<SpriteRenderer>().enabled = false;

    //    string namePlateText = "namePlayer0" + (index + 1);
    //    var namePlate = GameObject.Find(namePlateText);
    //    DebugHelper.Log("namePlate name: " + namePlate.name);

    //    namePlate.GetComponent<TMP_Text>().text = " ";
    //    namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);

    //    string numberRepairCardText = "numberRepairCards0" + (index + 1);
    //    var numberRepairCard = GameObject.Find(numberRepairCardText);
    //    DebugHelper.Log("repairCardSymbol name: " + numberRepairCard.name);

    //    numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";

    //}

    public void SetUpBackToMenu()
    {
        DebugHelper.Log("8 -- SeUPBackToMenu");

        backgroundMusic.PlayMenuSound();

        gameIsOn = false;
        gameOver.gameIsOver = false;

        //DeactivateAll();
        //ResetAllComponents();
        //ResetAllPlatenames();

        deckEvent.ResetAllEventCards();

        var gameConnection = FindFirstObjectByType<GameConnection>();
        gameConnection.OnLeftRoom();
        gameConnection.DisconectAndReconect();

        suitTop.GetComponent<Animator>().enabled = true;
        suitTop.GetComponent<Animator>().SetBool("openSuit", false);
    }

    public void ResetAllPlatenames()
    {
        DebugHelper.Log("11 -- Resetando platenames");
        for (int i = 0; i < 4; i++)
        {
            string plateName = "plateName0" + (i + 1);
            var plate = GameObject.Find(plateName);
            DebugHelper.Log("plate name: "+plate.name);


            plate.GetComponent<MeshRenderer>().enabled = false;
            plate.GetComponent<MeshCollider>().enabled = false;

            string repairSymbolName = "repairCardSymbol0" + (i + 1);
            var repairSymbol = GameObject.Find(repairSymbolName);
            DebugHelper.Log("repairSymbol name: "+repairSymbol.name); 


            repairSymbol.GetComponent<SpriteRenderer>().enabled = false;

            string namePlateText = "namePlayer0" + (i + 1);
            var namePlate = GameObject.Find(namePlateText);
            DebugHelper.Log("namePlate name: " + namePlate.name);

            namePlate.GetComponent<TMP_Text>().text = " ";
            namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);

            string numberRepairCardText = "numberRepairCards0" + (i + 1);
            var numberRepairCard = GameObject.Find(numberRepairCardText);
            DebugHelper.Log("repairCardSymbol name: " + numberRepairCard.name);


            numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";

        }
    }
    public void ResetAllComponents()
    {
        DebugHelper.Log("10 -- ResetAllComponents");

        foreach (var component in timeCraxComponents)
        {
            component.malfunctions = 0;
        }

        foreach (var component in componentsWithAnimator)
        {

            DebugHelper.Log("opcName: " + component.name);
            component.GetComponent<Animator>().SetBool("malfunction", false);
            component.GetComponent<Animator>().enabled = false;
            component.tag = "Component";

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
        //        DebugHelper.Log("opcName: "+opc.gameObject.name);
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
                DebugHelper.Log("player " + player.nickname + " est� na vez");
                endButton.GetComponent<MeshCollider>().enabled = true;
            }
            else
            {
                DebugHelper.Log("player " + player.nickname + " N�O est� na vez");
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

        var suitComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);
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
        DebugHelper.Log("9 -- Desativando platenames");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            string plateName = "plateName0" + (i + 1);
            var findObject = GameObject.Find(plateName);


            findObject.GetComponent<MeshCollider>().enabled = false;
        }

        DebugHelper.Log("Desativando components");
        var suitComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);
        foreach (var suitComponent in suitComponents)
        {
            if (suitComponent.malfunctions > 0)
            {
                DebugHelper.Log(suitComponent.name + " with malfunction > 0 becoming false");
                suitComponent.GetComponent<MeshCollider>().enabled = false;
            }
        }

        DebugHelper.Log("Desativando decks");
        deckEvent.GetComponent<MeshCollider>().enabled = false;
        deckRepair.GetComponent<MeshCollider>().enabled = false;
        timeline.GetComponent<MeshCollider>().enabled = false;
        endButton.GetComponent<MeshCollider>().enabled = false;
        quitButton.GetComponent<MeshCollider>().enabled = false;
        
    }

    public void GiveCard(int numberPlayer)
    {
        //string button = EventSystem.current.currentSelectedGameObject.name;
        //DebugHelper.Log("Nome: " + button);
        //int buttonName = int.Parse(EventSystem.current.currentSelectedGameObject.name);

        photonView.RPC("GiveRepairCard", RpcTarget.All, numberPlayer);

    }

    [PunRPC]
    public void GiveRepairCard(int numberPlayer)
    {

        PlayerScript playerSending = null;
        PlayerScript playerReceiving = null;

        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
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

            var repairCards = FindObjectsByType<RepairCard>(FindObjectsSortMode.None);
            List<RepairCard> orderedList = new List<RepairCard>();
            List<RepairCard> playerCards = new List<RepairCard>();

            foreach (var repairCard in repairCards)
            {
                if (repairCard.photonView.OwnerActorNr == playerSending.photonView.OwnerActorNr)
                {
                    DebugHelper.Log(" - " + repairCard.photonView.ViewID);
                    playerCards.Add(repairCard);
                }
            }

            orderedList = playerCards.OrderByDescending(x => x.index).ToList();
            RepairCard lastCard = orderedList[0];

            //DebugHelper.Log("Carta que est� sendo passada: " + lastCard.photonView.ViewID);

            //DebugHelper.Log("player recebendo o owner: " + PhotonNetwork.PlayerList[playerReceiving.index].NickName);
            lastCard.photonView.TransferOwnership(PhotonNetwork.PlayerList[playerReceiving.index]);

            //DebugHelper.Log("Recebendo carta: " + playerReceiving.nickname);
            playerReceiving.numberRepairCards++;

            string numberRepairCardsReceiver = "numberRepairCards0" + numberPlayer;
            var findReceiverNumberCards = GameObject.Find(numberRepairCardsReceiver);
            DebugHelper.Log("receiver: " + findReceiverNumberCards.name);

            //int numberOfCardsReceiver = int.Parse(findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text);
            //numberOfCardsReceiver++;

            findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text = playerReceiving.numberRepairCards.ToString();

            //DebugHelper.Log("Dando carta: " + playerSending.nickname);
            playerSending.numberRepairCards--;

            string numberRepairCardsSender = "numberRepairCards0" + (time + 1);
            var findSenderNumberCards = GameObject.Find(numberRepairCardsSender); 
            DebugHelper.Log("sender: " + findSenderNumberCards.name);
            DebugHelper.Log("time + 1: " + (time + 1));

            //int numberOfCardsSender = int.Parse(findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text);
            //DebugHelper.Log("antes -- number of cards sender: " + numberOfCardsSender);
            //numberOfCardsSender--;
            //DebugHelper.Log("depois -- number of cards sender: " + numberOfCardsSender);

            findSenderNumberCards.GetComponent<TextMeshProUGUI>().text = playerSending.numberRepairCards.ToString();

            //DebugHelper.Log("ativando animator");
            lastCard.GetComponent<Animator>().enabled = true;
           // DebugHelper.Log("ativando anima��o sending");
            lastCard.GetComponent<Animator>().SetBool("sending", true);
        }
        else
        {
            //DebugHelper.Log("Voc� n�o possui cartas!");
        }

    }

}
