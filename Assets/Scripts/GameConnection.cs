
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using TimeCrax.Core;

public class GameConnection : MonoBehaviourPunCallbacks
{

    List<RoomInfo> rooms = new List<RoomInfo>();
    List<RoomInfo> closedRooms = new List<RoomInfo>();

    private int sufix = 0;
    //public TMPro.TextMeshProUGUI roomListScroll;
    public GameObject lobbyBackgroundScreen;
    public GameObject lobbyScreen;
    public GameObject roomScreen;
    public GameObject createRoom;
    public GameObject fullGameScreen;
    public Button buttonStart;
    public TMPro.TextMeshProUGUI chatLog;
    public TMPro.TextMeshProUGUI players;
    public GameObject roomNameTitle;
    public GameObject difficultyTitle;
    public GameObject maxPlayersTitle;
    public GameObject themeTitle;
    public GameObject passwordTitle;
    public GameObject roomList;
    public InputField nameDisplay;


    public void EnterServerAndLobby() 
    { 
        PhotonNetwork.ConnectUsingSettings();
    }

    public void ConnectingInServerAndLobby()
    {
        //DebugHelper.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        sufix++;
        string name = "Players"+sufix.ToString();
        PhotonNetwork.LocalPlayer.NickName = name;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {

        if (PhotonNetwork.InLobby == false)
        {
            DebugHelper.Log("Entrando no Lobby");
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        DebugHelper.Log("[GameConnection] Entrou no Lobby");

        // Se há sala pendente para criar, criar agora
        if (pendingRoomData != null)
        {
            DebugHelper.Log("[GameConnection] Criando sala pendente...");
            CreateRoomInternal(
                pendingRoomData.nameRoom,
                pendingRoomData.maxPlayers,
                pendingRoomData.difficulty,
                pendingRoomData.theme,
                pendingRoomData.password,
                pendingRoomData.themeId
            );
            pendingRoomData = null;
        }

        // Se há sala pendente para entrar, entrar agora
        if (!string.IsNullOrEmpty(pendingJoinRoomName))
        {
            DebugHelper.Log($"[GameConnection] Entrando na sala pendente: {pendingJoinRoomName}");
            PhotonNetwork.JoinRoom(pendingJoinRoomName);
            pendingJoinRoomName = null;
        }
    }
    public void Start()
    {
        //PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("nickname");
        //RoomOptions room = new RoomOptions { MaxPlayers = (byte)PlayerPrefs.GetInt("numberOfPlayers"), EmptyRoomTtl = 0 };
        //PhotonNetwork.JoinOrCreateRoom("TimeCrax", room, null);

    }

    public void Lobby()
    {
        DebugHelper.Log("Entrou no LobbyScreen");
        PhotonNetwork.LocalPlayer.NickName = SessionData.Nickname;
        lobbyBackgroundScreen.SetActive(true);
        lobbyScreen.SetActive(true);
        roomList.GetComponent<RoomList>().GetRoomsList(rooms);
        //ListRooms();

    }

    public void CreateRoom()
    {
        DebugHelper.Log("Entrou no Create Room");
        PhotonNetwork.LocalPlayer.NickName = SessionData.Nickname;
        lobbyBackgroundScreen.SetActive(true);
        createRoom.SetActive(true);
    }

    public void CreatedRoom(string nameRoom, int maxPlayers, string difficulty, string theme, string password, string themeId = "")
    {
        //DebugHelper.Log("password: " + password + " --- isnullorwhite: " + string.IsNullOrWhiteSpace(password));
        DebugHelper.Log("Entrou na Sala Criada");

        // Verificar se está conectado ao Master Server e pronto para operações
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
        {
            DebugHelper.Log("[GameConnection] Não está pronto para criar sala. Reconectando...");

            // Armazenar dados para criar sala após reconexão
            pendingRoomData = new PendingRoomData
            {
                nameRoom = nameRoom,
                maxPlayers = maxPlayers,
                difficulty = difficulty,
                theme = theme,
                password = password,
                themeId = themeId
            };

            // Reconectar se necessário
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            else if (PhotonNetwork.IsConnected && !PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            return;
        }

        CreateRoomInternal(nameRoom, maxPlayers, difficulty, theme, password, themeId);
    }

    private void CreateRoomInternal(string nameRoom, int maxPlayers, string difficulty, string theme, string password, string themeId)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = (byte)maxPlayers, EmptyRoomTtl = 0, PlayerTtl = 0 };
        options.CustomRoomPropertiesForLobby = new string[4] { "dif", "the", "pass", "themeId" };
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add("dif", difficulty);
        options.CustomRoomProperties.Add("the", theme);
        options.CustomRoomProperties.Add("pass", password);
        options.CustomRoomProperties.Add("themeId", themeId);
        PhotonNetwork.CreateRoom(nameRoom, options, null);
        DebugHelper.Log($"[GameConnection] Sala criada com tema: {theme} (ID: {themeId})");
    }

    // Dados pendentes para criar sala após reconexão
    private PendingRoomData pendingRoomData;

    private class PendingRoomData
    {
        public string nameRoom;
        public int maxPlayers;
        public string difficulty;
        public string theme;
        public string password;
        public string themeId;
    }
    
    public void ReturnigToMenu()
    {
        DebugHelper.Log("Entrou no Return to menu");
        var menu = FindFirstObjectByType<Menu>();
        menu.EnableMenu();
        fullGameScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);
        nameDisplay.gameObject.SetActive(true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        chatLog.text = null;
        lobbyScreen.SetActive(false);
        fullGameScreen.SetActive(true);
        this.DelayedCall(4f, ReturnigToMenu);

        if (returnCode == ErrorCode.GameDoesNotExist)
        {

        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        chatLog.text += "\n" + newPlayer.NickName + " entrou na sala";
        ListPlayersInRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DebugHelper.Log("Client saiu da sala");
        chatLog.text += "\n" + otherPlayer.NickName + " saiu na sala";
        ListPlayersInRoom();
        //CheckIfIsMaster();
        //DisconectAndReconect();
    }

    public override void OnLeftRoom()
    {
        //Rooms.Clear();
        DebugHelper.Log("Master saiu da Sala");
        ListPlayersInRoom();
        chatLog.text = null;
        //DisconectAndReconect();
    }

    public void DisconectAndReconect()
    {
        DebugHelper.Log("Disconecting and Reconecting");
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CheckIfIsMaster()
    {
        DebugHelper.Log($"[GameConnection] CheckIfIsMaster - IsMasterClient: {PhotonNetwork.IsMasterClient}");

        // Tentar encontrar o botão se a referência não estiver configurada
        if (buttonStart == null && roomScreen != null)
        {
            var buttons = roomScreen.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name == "StartGameButton" || btn.gameObject.name == "Start" || btn.gameObject.name == "ButtonStart" || btn.gameObject.name == "StartButton")
                {
                    buttonStart = btn;
                    DebugHelper.Log($"[GameConnection] Botão Start encontrado dinamicamente: {btn.gameObject.name}");
                    break;
                }
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            if (buttonStart != null)
            {
                buttonStart.interactable = true;
                DebugHelper.Log("[GameConnection] Botão Start HABILITADO (MasterClient)");
            }
            else
            {
                DebugHelper.Log("[GameConnection] ERRO: buttonStart é null!");
            }
        }
        else
        {
            if (buttonStart != null)
            {
                buttonStart.interactable = false;
                DebugHelper.Log("[GameConnection] Botão Start DESABILITADO (não é MasterClient)");
            }
            else
            {
                DebugHelper.Log("[GameConnection] ERRO: buttonStart é null!");
            }
        }
    }

    public void JoinRoomInList(string roomName)
    {
        DebugHelper.Log("Entrando na sala: " + roomName);

        // Verificar se está conectado ao Master Server e pronto para operações
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
        {
            DebugHelper.Log("[GameConnection] Não está pronto para entrar na sala. Reconectando...");

            // Armazenar nome da sala para entrar após reconexão
            pendingJoinRoomName = roomName;

            // Reconectar se necessário
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            else if (PhotonNetwork.IsConnected && !PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    // Nome da sala pendente para entrar após reconexão
    private string pendingJoinRoomName;
    public override void OnJoinedRoom()
    {
        DebugHelper.Log("Entrou na sala");

        createRoom.SetActive(false);
        lobbyScreen.SetActive(false);
        roomScreen.SetActive(true);

        // Primeiro desabilita o botão para todos
        if (buttonStart != null)
        {
            buttonStart.interactable = false;
        }

        // Depois verifica se é MasterClient para habilitar
        CheckIfIsMaster();

        roomNameTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
        maxPlayersTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        themeTitle.GetComponent<TextMeshProUGUI>().text = "Theme: " + PhotonNetwork.CurrentRoom.CustomProperties["the"];
        difficultyTitle.GetComponent<TextMeshProUGUI>().text = "" + PhotonNetwork.CurrentRoom.CustomProperties["dif"];
        passwordTitle.GetComponent<TextMeshProUGUI>().text = "" + PhotonNetwork.CurrentRoom.CustomProperties["pass"];
        ListPlayersInRoom();

        chatLog.text += "Você entrou na sala";

        //maxPlayers.text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public void ListPlayersInRoom()
    {
        players.text = null;

        if (PhotonNetwork.CurrentRoom != null)
        {
            //maxPlayers.text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
            maxPlayersTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
            foreach (int key in PhotonNetwork.CurrentRoom.Players.Keys)
            {
                players.text += " " + PhotonNetwork.CurrentRoom.Players[key].NickName + "\n";
            }
        }
        else
        {

        }
        
    }

    public bool CheckPassword(string nameRoom, string password)
    {
        DebugHelper.Log("Entrou no CheckPassword");

        for (int i = 0; i < rooms.Count; i++)
        {
            DebugHelper.Log("RoomName: "+ rooms[i].Name.ToUpper()+" ----- nameRoom: "+ nameRoom.ToUpper());
            if (rooms[i].Name.ToUpper() == nameRoom.ToUpper())
            {
                string passwordRoom = rooms[i].CustomProperties["pass"].ToString();

                DebugHelper.Log("Comparando senhas: ---- senha01: " + passwordRoom.ToUpper() + " senha02: " + password.ToUpper());
                if ( passwordRoom.ToUpper() == password.ToUpper())
                {
                    return true;
                }
                else
                {
                    //passwordCorrect = false;
                    DebugHelper.Log("Password Errado 01");
                }
            }

        }

        return false;
    }

    public bool CheckRoomName(string nameRoom)
    {

        bool alreadyExist = false;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].Name.ToUpper() == nameRoom.ToUpper())
            {
                alreadyExist = true;
            }

        }

        for (int i = 0; i < closedRooms.Count; i++)
        {
            if (closedRooms[i].Name.ToUpper() == nameRoom.ToUpper())
            {
                alreadyExist = true;
            }

        }

        return alreadyExist;
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        DebugHelper.Log("Atualizando salas");
        DebugHelper.Log("Numero de salas criadas: "+roomList.Count);

        rooms.Clear();
        closedRooms.Clear();
        //Rooms = Rooms.Distinct().ToList();

        for (int i = 0; i < roomList.Count; i++)
        {
            DebugHelper.Log(" - " + roomList[i].Name);
            if (roomList[i].PlayerCount == 0 || !roomList[i].IsOpen) 
            {
                DebugHelper.Log("Sala vazia ou fechada");
                roomList[i].RemovedFromList = true;
                closedRooms.Add(roomList[i]);
            }
            else
            {
                DebugHelper.Log("adicionando sala aberta na lista");
                roomList[i].RemovedFromList = false;
                rooms.Add(roomList[i]);
            }

        }
        DebugHelper.Log("Salas ABERTAS armazenadas na lista flexivel:");
        DebugHelper.Log("Tamanho da lista: "+rooms.Count);
        for (int i = 0; i < rooms.Count; i++)
        {
            DebugHelper.Log("Sala: " + rooms[i].Name+" no index: "+ i);
        }

    }

}
