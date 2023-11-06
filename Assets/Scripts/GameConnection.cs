
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameConnection : MonoBehaviourPunCallbacks
{
    private int sufix = 0;
    public GameObject lobbyScreen;
    public GameObject fullGameScreen;
    public Button buttonStart;
    public TMPro.TextMeshProUGUI chatLog;
    public TMPro.TextMeshProUGUI maxPlayers;
    public TMPro.TextMeshProUGUI players;

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
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("nickname");
        RoomOptions room = new RoomOptions { MaxPlayers = (byte)PlayerPrefs.GetInt("numberOfPlayers"), EmptyRoomTtl = 0 };
        PhotonNetwork.JoinOrCreateRoom("TimeCrax", room, null);

    }
    public void ReturnigToMenu()
    {
        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
        fullGameScreen.SetActive(false);
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
    public override void OnJoinedRoom()
    {
        if(!lobbyScreen.gameObject.activeInHierarchy)
        {
            lobbyScreen.SetActive(true);
        }
        CheckIfIsMaster();
        chatLog.text += "Você entrou na sala";
        ListPlayersInRoom();
        maxPlayers.text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public void ListPlayersInRoom()
    {
        players.text = null;

        if (PhotonNetwork.CurrentRoom != null)
        {
            maxPlayers.text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
            foreach (int key in PhotonNetwork.CurrentRoom.Players.Keys)
            {
                players.text += " " + PhotonNetwork.CurrentRoom.Players[key].NickName + "\n";
            }
        }
        else
        {

        }
        
    }

}
