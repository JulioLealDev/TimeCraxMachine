
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameConnection : MonoBehaviourPunCallbacks
{

    List<RoomInfo> Rooms = new List<RoomInfo>();

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
        sufix++;
        string name = "Players"+sufix.ToString();
        PhotonNetwork.LocalPlayer.NickName = name;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {

        if (PhotonNetwork.InLobby == false)
        {
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
        Debug.Log("Entrou no Lobby");
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("nickname");
        lobbyBackgroundScreen.SetActive(true);
        lobbyScreen.SetActive(true);
        roomList.GetComponent<RoomList>().GetRoomsList(Rooms);
        //ListRooms();

    }

    public void CreateRoom()
    {
        Debug.Log("Entrou no Create Room");
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("nickname");
        lobbyBackgroundScreen.SetActive(true);
        createRoom.SetActive(true);
    }

    public void CreatedRoom(string nameRoom, int maxPlayers, string difficulty, string theme, string password)
    {
        Debug.Log("Entrou no Sala Criada");
        RoomOptions options = new RoomOptions { MaxPlayers = (byte)maxPlayers, EmptyRoomTtl = 0, PlayerTtl = 0 };
        options.CustomRoomPropertiesForLobby = new string[3] { "dif", "the", "pass" };
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add("dif", difficulty);
        options.CustomRoomProperties.Add("the", theme);
        options.CustomRoomProperties.Add("pass", password);
        PhotonNetwork.CreateRoom( nameRoom, options, null);
    }
    
    public void ReturnigToMenu()
    {
        Debug.Log("Entrou no Return to menu");
        var menu = FindObjectOfType<Menu>();
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
        Invoke("ReturnigToMenu", 4);

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
        chatLog.text += "\n" + otherPlayer.NickName + " saiu na sala";
        ListPlayersInRoom();
        CheckIfIsMaster();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Saiu da Sala");
        ListPlayersInRoom();
        chatLog.text = null;
    }

    public void CheckIfIsMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            buttonStart.interactable = true;
        }
        else
        {
            buttonStart.interactable = false;
        }
    }

    public void JoinRoomInList(string roomName)
    {
        Debug.Log("Entrando na sala: " + roomName);
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Entrou na sala");

        createRoom.SetActive(false);
        lobbyScreen.SetActive(false);
        roomScreen.SetActive(true); 

        //if (!lobbyScreen.gameObject.activeInHierarchy)
        //{
        //    Debug.Log("Entrou no if");
        //    createRoom.SetActive(false);
        //    lobby.SetActive(true);
        //}

        //CheckIfIsMaster();

        roomNameTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
        maxPlayersTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        themeTitle.GetComponent<TextMeshProUGUI>().text = "Theme: " + PhotonNetwork.CurrentRoom.CustomProperties["the"];
        difficultyTitle.GetComponent<TextMeshProUGUI>().text = "Difficulty: " + PhotonNetwork.CurrentRoom.CustomProperties["dif"];
        passwordTitle.GetComponent<TextMeshProUGUI>().text = "Password: " + PhotonNetwork.CurrentRoom.CustomProperties["pass"];
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

    public bool CheckRoomName(string nameRoom)
    {

        bool alreadyExist = false;
        for (int i = 0; i < Rooms.Count; i++)
        {
            if (Rooms[i].Name.ToUpper() == nameRoom.ToUpper())
            {
                alreadyExist = true;
            }

        }

        return alreadyExist;
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Atualizando salas");
        Debug.Log("Numero de salas criadas: "+roomList.Count);

        Rooms.Clear();

        for (int i = 0; i < roomList.Count; i++)
        {
            Debug.Log(" - " + roomList[i].Name);
            if (roomList[i].PlayerCount == 0 || roomList[i].PlayerCount == roomList[i].MaxPlayers) 
            {
                Debug.Log("Cheia ou vazia");
                roomList[i].RemovedFromList = true;
            }
            else
            {
                Debug.Log("adicionando lasa na lista");
                roomList[i].RemovedFromList = false;
                Rooms.Add(roomList[i]);
            }

        }



        //for (int i = 0; i < roomList.Count; i++)
        //{

            //    Rooms.Add(roomList[i]);
            //    roomListScroll.text += " " + roomList[i].Name + "\n";
            //    Debug.Log("Sala " + roomList[i].Name + " adicionada a lista ");
            //}

    }

    //public void ListRooms()
    //{
    //    roomListScroll.text = null;

    //    if (PhotonNetwork.CountOfRooms > 0)
    //    {

    //        foreach (var room in Rooms)
    //        {
    //            roomListScroll.text += " " + room.Name + "\n";
    //            Debug.Log("Sala" + room.Name + " adicionada a lista ");
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("Não há Salas criadas!");
    //    }

    //}

}
