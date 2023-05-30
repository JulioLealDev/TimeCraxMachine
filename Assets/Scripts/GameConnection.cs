
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
            //RoomOptions room = new RoomOptions { MaxPlayers = (byte)PlayerPrefs.GetInt("numberOfPlayers")};
            //PhotonNetwork.CreateRoom("TimeCrax2", room, null);
            //chatLog.text += "\nCriando sala TimeCrax2!";
            //SetLobbyChildren("Log", "\nCriando sala TimeCrax2!");

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

    //public void SetLobbyChildren(string name = null, string value = "", bool interactive = false)
    //{
    //    lobbyScreen = GameObject.FindGameObjectWithTag("Lobby");
    //    if (lobbyScreen != null)
    //    {
    //        var components = lobbyScreen.GetComponentsInChildren<Transform>();
    //        foreach (var component in components)
    //        {
    //            if (component.name == "StartButton" && name == "StartButton")
    //            {
    //                Debug.Log("if do button, quem entrou foi ->" + component.name);
    //                component.GetComponentInChildren<Button>().interactable = interactive;
    //            }
    //            else
    //            {
    //                if (component.name == name)
    //                {
    //                    Debug.Log("if dos texts, quem entrou foi ->" + component.name);
    //                    Debug.Log("Antes do maldito if --> value: " + value + " name: " + name);
    //                    if (value != "")
    //                    {
    //                        Debug.Log("if do value dif de null, quem entrou foi ->" + component.name + " com value: " + value);
    //                        if (name == "MaxPlayers")
    //                        {
    //                            Debug.Log("if do maxplayers, quem entrou foi ->" + component.name);
    //                            component.GetComponentInChildren<TextMeshProUGUI>().text = value;
    //                        }
    //                        else
    //                        {
    //                            Debug.Log("if do log e players, quem entrou foi ->" + component.name);
    //                            component.GetComponentInChildren<TextMeshProUGUI>().text += value;
    //                        }

    //                    }
    //                    else if (value == "" && name == "Players" || value == "" && name == "Log")
    //                    {
    //                        Debug.Log("if do value = null, quem entrou foi ->" + component.name + " com value: " + value);
    //                        component.GetComponentInChildren<TextMeshProUGUI>().text = null;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("LobbyScreen não encontrado");
    //    }
    //}
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
