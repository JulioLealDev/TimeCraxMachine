using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using UnityEngine.UI;
using TimeCrax.Core;
using TimeCrax.Themes;

public class LobbyOptions : MonoBehaviourPunCallbacks
{
    public GameObject lobbyBackgroundScreen;
    //public GameObject lobbyBackgroundScreen02;
    public GameConnection gameConnection;
    public GameObject roomNameInput;
    public GameObject maxPlayersDropdown;
    public GameObject difficultyDropdown;
    public GameObject privateDropdown;
    public GameObject themeDropdown;
    public GameObject passwordInput;
    public GameObject passwordLabel;    
    public Canvas loading;
    public GameManager gameManager;
    public GameObject createRoom;
    public GameObject roomScreen;
    public GameObject lobbyScreen;
    public InputField nameDisplay;
    public GameObject roomListContent;
    public GameObject createRoomButton;
    public GameObject roomNameWarning;
    public GameObject passwordWarning;
    public GameObject alreadyExistNameWarning;
    public BackgroundMusic backgroundMusic;
    public SoundEffects soundEffects;
    //public GameObject passwordScreen;

    bool privateRoom = false;

    public void ClickStart()
    {
        soundEffects.PressHudButtonSound();
        PhotonNetwork.CurrentRoom.IsOpen = false;

        photonView.RPC("StartMatch", RpcTarget.All);

    }

    [PunRPC]
    public void StartMatch()
    {
        backgroundMusic.PlayGameSound();
        SessionData.GameStarted = true;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        roomScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);
        //gameManager.SetActive(true);
        gameManager.StartNewGame();
    }
    public void EnablePassword()
    {
        privateRoom = !privateRoom;
        if(!privateRoom)
        {
            DebugHelper.Log("Não privada");
            passwordInput.GetComponent<TMP_InputField>().text = " ";
            passwordLabel.GetComponent<TextMeshProUGUI>().color = Color.gray;
        }
        else
        {
            DebugHelper.Log("Privada");
            passwordLabel.GetComponent<TextMeshProUGUI>().color = Color.white;
        }
        passwordInput.GetComponent<TMP_InputField>().readOnly = !privateRoom;
    }

    public void CreateRoom()
    {
        Verifications();
    }

    public void AllVerifyed()
    {
        soundEffects.PressHudButtonSound();

        string maxPlayers = maxPlayersDropdown.GetComponent<TextMeshProUGUI>().text;
        string difficulty = difficultyDropdown.GetComponent<TextMeshProUGUI>().text;
        string password = passwordInput != null ? passwordInput.GetComponent<TMP_InputField>().text : "";

        // Verificar se há tema da API selecionado
        string theme;
        string themeId = "";

        if (ThemeManager.Instance != null && ThemeManager.Instance.HasSelectedTheme)
        {
            var selectedTheme = ThemeManager.Instance.SelectedTheme;
            theme = selectedTheme.name;
            themeId = selectedTheme.id;
            DebugHelper.Log($"[LobbyOptions] Usando tema da API: {theme} (ID: {themeId})");
        }
        else
        {
            // Tema legado do dropdown
            theme = themeDropdown != null ? themeDropdown.GetComponent<TextMeshProUGUI>().text : "World History";
            DebugHelper.Log($"[LobbyOptions] Usando tema legado: {theme}");
        }

        string roomName = roomNameInput.GetComponent<TMP_InputField>().text + " - " + theme;
        int max = Int32.Parse(maxPlayers.Substring(0, 1));

        gameConnection.CreatedRoom(roomName, max, difficulty, theme, password, themeId);
    }
    public void CancelCreateRoom()
    {
        soundEffects.PressHudButtonSound();

        DebugHelper.Log("CancelCreateRoom clicked");
        createRoom.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);

        var menu = FindFirstObjectByType<Menu>();
        menu.EnableMenu();

        if (nameDisplay != null)
            nameDisplay.gameObject.SetActive(true);
    }

    public void CancelRoomScreen()
    {
        soundEffects.PressHudButtonSound();

        DebugHelper.Log("CancelRoomScreen clicked");
        roomScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);

        PhotonNetwork.LeaveRoom(false);

        var menu = FindFirstObjectByType<Menu>();
        menu.EnableMenu();

        if (nameDisplay != null)
            nameDisplay.gameObject.SetActive(true);
    }

    public void RefreshLobbyScreen()
    {
        DebugHelper.Log("Refreshing clicked");
        soundEffects.PressHudButtonSound();

        DestroyRooms();

        DebugHelper.Log("Disconecting and Reconecting");
        RefreshConection();

        this.DelayedCall(0.5f, ListRooms);

    }

    public void ListRooms()
    {
        gameConnection.Lobby();
    }

    public void RefreshConection()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
    }

    public void BackLobbyScreen()
    {
        DebugHelper.Log("BackLobbyScreen clicked");
        soundEffects.PressHudButtonSound();

        DestroyRooms();

        lobbyScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);

        var menu = FindFirstObjectByType<Menu>();
        menu.EnableMenu();

        if (nameDisplay != null)
            nameDisplay.gameObject.SetActive(true);

        DebugHelper.Log("Disconecting and Reconecting");
        RefreshConection();
    }

    public void ActivateButtons(bool activate)
    {
        Button[] buttons = lobbyScreen.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = activate;
        }
    }

    public void DestroyRooms()
    {
        foreach (Room room in roomListContent.GetComponentsInChildren<Room>())
        {
            if (!room.CompareTag("Undestructable"))
            {
                DebugHelper.Log("Destruindo sala : " + room.GetInstanceID());
                DebugHelper.Log("Destruindo sala : " + room.GetComponentInChildren<TMP_Text>().text);
                Destroy(room.gameObject);
            }

        }
    }

    public void Verifications()
    {
        string roomName = roomNameInput.GetComponent<TMP_InputField>().text;
        string password = passwordInput != null ? passwordInput.GetComponent<TMP_InputField>().text : "";
        string privateRoom = privateDropdown != null ? privateDropdown.GetComponent<TextMeshProUGUI>().text : "No";

        bool alreadyExist = gameConnection.CheckRoomName(roomName);

        if (string.IsNullOrEmpty(roomName))
        {
            roomNameWarning.SetActive(true);
            roomNameWarning.GetComponent<Animator>().SetBool("roomNameIsEmpty", true);
            createRoomButton.GetComponent<Button>().enabled = false;
            this.DelayedCall(1.5f, AfterClickStart);
        }
        else if (alreadyExist)
        {
            alreadyExistNameWarning.SetActive(true);
            alreadyExistNameWarning.GetComponent<Animator>().SetBool("alreadyExistName", true);
            createRoomButton.GetComponent<Button>().enabled = false;
            this.DelayedCall(1.5f, AfterClickStart);
        }
        else if (privateRoom != "No" && string.IsNullOrEmpty(password))
        {
            passwordWarning.SetActive(true);
            passwordWarning.GetComponent<Animator>().SetBool("passwordIsEmpty", true);
            createRoomButton.GetComponent<Button>().enabled = false;
            this.DelayedCall(1.5f, AfterClickStart);
        }
        else
        {
            AllVerifyed();
        }
    }

    private void AfterClickStart()
    {
        roomNameWarning.SetActive(false);
        alreadyExistNameWarning.SetActive(false);
        passwordWarning.SetActive(false);
        roomNameWarning.GetComponent<Animator>().SetBool("roomNameIsEmpty", false);
        alreadyExistNameWarning.GetComponent<Animator>().SetBool("alreadyExistName", false);
        passwordWarning.GetComponent<Animator>().SetBool("passwordIsEmpty", false);
   

        createRoomButton.GetComponent<Button>().enabled = true;

    }

}
