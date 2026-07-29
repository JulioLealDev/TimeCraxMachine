
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using TimeCrax.Core;
using TimeCrax.Managers;

public class GameConnection : MonoBehaviourPunCallbacks
{
    [SerializeField] private MenuManager menuManager;

    List<RoomInfo> rooms = new List<RoomInfo>();
    List<RoomInfo> closedRooms = new List<RoomInfo>();

    private int sufix = 0;

    // Flag para prevenir cliques múltiplos durante operações de rede
    private bool isProcessingRoomOperation = false;
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

    public override void OnJoinedLobby()
    {

        // Se há sala pendente para criar, criar agora
        if (pendingRoomData != null)
        {
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
            PhotonNetwork.JoinRoom(pendingJoinRoomName);
            pendingJoinRoomName = null;
        }
    }

    public bool Lobby()
    {
        if (lobbyBackgroundScreen == null || lobbyScreen == null || roomList == null)
        {
            return false;
        }

        PhotonNetwork.LocalPlayer.NickName = SessionData.Nickname;
        lobbyBackgroundScreen.SetActive(true);
        lobbyScreen.SetActive(true);
        roomList.GetComponent<RoomList>().GetRoomsList(rooms);

        var lobbyOptions = FindFirstObjectByType<LobbyOptions>();
        if (lobbyOptions != null)
            lobbyOptions.ActivateButtons(true);

        return true;
    }

    public bool CreateRoom()
    {
        if (lobbyBackgroundScreen == null || createRoom == null)
        {
            return false;
        }

        PhotonNetwork.LocalPlayer.NickName = SessionData.Nickname;
        lobbyBackgroundScreen.SetActive(true);
        createRoom.SetActive(true);
        return true;
    }

    public void CreatedRoom(string nameRoom, int maxPlayers, string difficulty, string theme, string password, string themeId = "")
    {

        // Prevenir cliques múltiplos
        if (isProcessingRoomOperation)
        {
            return;
        }
        isProcessingRoomOperation = true;

        // Verificar se está conectado ao Master Server e pronto para operações
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
        {

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
            else if (PhotonNetwork.IsConnected && !PhotonNetwork.InLobby
                     && PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.JoiningLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            // Se estado é JoiningLobby, OnJoinedLobby vai processar pendingRoomData
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
        var suitTop = FindFirstObjectByType<SuitTop>();
        menuManager.EnablingMenuOptions();
        fullGameScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);
        nameDisplay.gameObject.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {

        // Resetar flag de operação
        isProcessingRoomOperation = false;
        pendingRoomData = null;

        // Voltar para a tela de criação de sala
        if (createRoom != null)
        {
            createRoom.SetActive(true);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {

        // Resetar flag de operação
        isProcessingRoomOperation = false;

        if (chatLog != null)
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

        // Atualizar chat e lista localmente
        if (chatLog != null)
        {
            chatLog.text += "\n" + newPlayer.NickName + " joined the room";
        }
        ListPlayersInRoom();

        // Se sou MasterClient, sincronizar a mensagem de chat para TODOS (incluindo quem acabou de entrar)
        if (PhotonNetwork.IsMasterClient && photonView != null)
        {
            photonView.RPC("SyncPlayerEnteredMessage", RpcTarget.All, newPlayer.NickName);
        }
    }

    /// <summary>
    /// RPC para sincronizar mensagem de entrada de jogador em todos os clientes
    /// </summary>
    [PunRPC]
    public void SyncPlayerEnteredMessage(string playerName)
    {

        // Atualizar lista de jogadores em todos os clientes
        ListPlayersInRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        if (chatLog != null)
        {
            chatLog.text += "\n" + otherPlayer.NickName + " left the room";
        }
        ListPlayersInRoom();

        // Se sou MasterClient, sincronizar para todos
        if (PhotonNetwork.IsMasterClient && photonView != null)
        {
            photonView.RPC("SyncPlayerLeftMessage", RpcTarget.All, otherPlayer.NickName);
        }
    }

    /// <summary>
    /// RPC para sincronizar mensagem de saída de jogador em todos os clientes
    /// </summary>
    [PunRPC]
    public void SyncPlayerLeftMessage(string playerName)
    {

        // Atualizar lista de jogadores em todos os clientes
        ListPlayersInRoom();
    }

    public override void OnLeftRoom()
    {

        // Resetar flag de operação
        isProcessingRoomOperation = false;

        ListPlayersInRoom();

        if (chatLog != null)
            chatLog.text = null;
    }

    public void DisconectAndReconect()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CheckIfIsMaster()
    {

        // Tentar encontrar o botão se a referência não estiver configurada
        if (buttonStart == null && roomScreen != null)
        {
            var buttons = roomScreen.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name == "StartGameButton" || btn.gameObject.name == "Start" || btn.gameObject.name == "ButtonStart" || btn.gameObject.name == "StartButton")
                {
                    buttonStart = btn;
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
            }
            else
            {
            }
        }
        else
        {
            if (buttonStart != null)
            {
                buttonStart.interactable = false;
            }
            else
            {
            }
        }
    }

    public void JoinRoomInList(string roomName)
    {
        // Prevenir cliques múltiplos
        if (isProcessingRoomOperation)
        {
            return;
        }

        isProcessingRoomOperation = true;

        // Verificar se está conectado ao Master Server e pronto para operações
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
        {

            // Armazenar nome da sala para entrar após reconexão
            pendingJoinRoomName = roomName;

            // Reconectar se necessário
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            else if (PhotonNetwork.IsConnected && !PhotonNetwork.InLobby
                     && PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.JoiningLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            // Se estado é JoiningLobby, OnJoinedLobby vai processar pendingJoinRoomName
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// Verifica se uma operação de sala está em andamento
    /// </summary>
    public bool IsProcessingRoomOperation()
    {
        return isProcessingRoomOperation;
    }

    /// <summary>
    /// Reseta a flag de operação (usado quando a operação termina ou é cancelada)
    /// </summary>
    public void ResetRoomOperation()
    {
        isProcessingRoomOperation = false;
    }

    /// <summary>
    /// Limpa operações pendentes (usado ao cancelar entrada em sala)
    /// </summary>
    public void ClearPendingOperations()
    {
        pendingJoinRoomName = null;
        pendingRoomData = null;
    }

    // Nome da sala pendente para entrar após reconexão
    private string pendingJoinRoomName;
    public override void OnJoinedRoom()
    {

        // Resetar flag de operação - entrada na sala foi bem sucedida
        isProcessingRoomOperation = false;

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

        // Atualizar informações da sala
        if (roomNameTitle != null)
            roomNameTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;

        if (maxPlayersTitle != null)
            maxPlayersTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (themeTitle != null)
            themeTitle.GetComponent<TextMeshProUGUI>().text = "Theme: " + PhotonNetwork.CurrentRoom.CustomProperties["the"];

        if (difficultyTitle != null)
            difficultyTitle.GetComponent<TextMeshProUGUI>().text = "" + PhotonNetwork.CurrentRoom.CustomProperties["dif"];

        if (passwordTitle != null)
            passwordTitle.GetComponent<TextMeshProUGUI>().text = "" + PhotonNetwork.CurrentRoom.CustomProperties["pass"];

        // Atualizar lista de jogadores
        ListPlayersInRoom();

        // Atualizar chat
        if (chatLog != null)
        {
            // Limpar chat ao entrar e mostrar todos os jogadores na sala
            chatLog.text = "You joined the room";


        }

    }

    public void ListPlayersInRoom()
    {

        if (players != null)
        {
            players.text = null;
        }
        else
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom != null)
        {

            if (maxPlayersTitle != null)
            {
                maxPlayersTitle.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Players.Count + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
            }

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

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].Name.ToUpper() == nameRoom.ToUpper())
            {
                string passwordRoom = rooms[i].CustomProperties["pass"].ToString();

                if ( passwordRoom.ToUpper() == password.ToUpper())
                {
                    return true;
                }
                else
                {
                    //passwordCorrect = false;
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

        rooms.Clear();
        closedRooms.Clear();
        //Rooms = Rooms.Distinct().ToList();

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount == 0 || !roomList[i].IsOpen) 
            {
                roomList[i].RemovedFromList = true;
                closedRooms.Add(roomList[i]);
            }
            else
            {
                roomList[i].RemovedFromList = false;
                rooms.Add(roomList[i]);
            }

        }
        for (int i = 0; i < rooms.Count; i++)
        {
        }

    }

}
