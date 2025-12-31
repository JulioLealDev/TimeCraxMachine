
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

    public void CreatedRoom(string nameRoom, int maxPlayers, string difficulty, string theme, string password)
    {
        //DebugHelper.Log("password: " + password + " --- isnullorwhite: " + string.IsNullOrWhiteSpace(password));
        DebugHelper.Log("Entrou na Sala Criada");
        RoomOptions options = new RoomOptions { MaxPlayers = (byte)maxPlayers, EmptyRoomTtl = 0, PlayerTtl = 0 };
        options.CustomRoomPropertiesForLobby = new string[3] { "dif", "the", "pass" };
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add("dif", difficulty);
        options.CustomRoomProperties.Add("the", theme);
        options.CustomRoomProperties.Add("pass", password);
        PhotonNetwork.CreateRoom( nameRoom, options, null);
        //DebugHelper.Log("options.pass: " + options.CustomRoomProperties["pass"] + " --- isnullorwhite: " + string.IsNullOrWhiteSpace(options.CustomRoomProperties["pass"].ToString()));
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
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            buttonStart.interactable = true;
        }
        else
        {
            buttonStart.interactable = false;
        }
    }

    public void JoinRoomInList(string roomName)
    {
        DebugHelper.Log("Entrando na sala: " + roomName);
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnJoinedRoom()
    {
        DebugHelper.Log("Entrou na sala");

        createRoom.SetActive(false);
        lobbyScreen.SetActive(false);
        roomScreen.SetActive(true); 

        //if (!lobbyScreen.gameObject.activeInHierarchy)
        //{
        //    DebugHelper.Log("Entrou no if");
        //    createRoom.SetActive(false);
        //    lobby.SetActive(true);
        //}

        CheckIfIsMaster();

        roomNameTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
        maxPlayersTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        themeTitle.GetComponent<TextMeshProUGUI>().text = "Theme: " + PhotonNetwork.CurrentRoom.CustomProperties["the"];
        difficultyTitle.GetComponent<TextMeshProUGUI>().text = "Difficulty: " + PhotonNetwork.CurrentRoom.CustomProperties["dif"];
        passwordTitle.GetComponent<TextMeshProUGUI>().text = "Password: " + PhotonNetwork.CurrentRoom.CustomProperties["pass"];
        ListPlayersInRoom();

        chatLog.text += "Voc� entrou na sala";

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
