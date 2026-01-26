using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using UnityEngine.UI;
using TimeCrax.Core;
using TimeCrax.Themes;

public class LobbyOptions : MonoBehaviourPunCallbacks
{
    [Header("Telas")]
    [SerializeField] private GameObject lobbyBackgroundScreen;
    [SerializeField] private GameObject createRoom;
    [SerializeField] private GameObject roomScreen;
    [SerializeField] private GameObject lobbyScreen;

    [Header("Inputs")]
    [SerializeField] private GameObject roomNameInput;
    [SerializeField] private GameObject maxPlayersDropdown;
    [SerializeField] private GameObject difficultyDropdown;
    [SerializeField] private GameObject privateDropdown;
    [SerializeField] private GameObject themeDropdown;
    [SerializeField] private GameObject passwordInput;
    [SerializeField] private GameObject passwordLabel;
    [SerializeField] private InputField nameDisplay;

    [Header("UI")]
    [SerializeField] private GameObject roomListContent;
    [SerializeField] private GameObject createRoomButton;
    [SerializeField] private GameObject roomNameWarning;
    [SerializeField] private GameObject passwordWarning;
    [SerializeField] private GameObject alreadyExistNameWarning;

    [Header("Referências")]
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private GameManager gameManager;

    [Header("Áudio")]
    [SerializeField] private BackgroundMusic backgroundMusic;
    [SerializeField] private SoundEffects soundEffects;

    private bool privateRoom = false;

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
        if (soundEffects != null)
            soundEffects.PressHudButtonSound();

        string maxPlayers = "2";
        string difficulty = "Normal";
        string password = "";

        if (maxPlayersDropdown != null)
        {
            var tmp = maxPlayersDropdown.GetComponent<TextMeshProUGUI>();
            if (tmp != null) maxPlayers = tmp.text;
        }

        if (difficultyDropdown != null)
        {
            var tmp = difficultyDropdown.GetComponent<TextMeshProUGUI>();
            if (tmp != null) difficulty = tmp.text;
        }

        if (passwordInput != null)
        {
            var tmp = passwordInput.GetComponent<TMP_InputField>();
            if (tmp != null) password = tmp.text;
        }

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
            // Tema legado - pegar do ThemeDropdownUI
            var dropdownUI = FindFirstObjectByType<ThemeDropdownUI>();
            if (dropdownUI != null)
            {
                theme = dropdownUI.GetSelectedThemeName();
                if (string.IsNullOrEmpty(theme))
                    theme = "World History";
            }
            else if (themeDropdown != null)
            {
                var tmp = themeDropdown.GetComponent<TextMeshProUGUI>();
                theme = tmp != null ? tmp.text : "World History";
            }
            else
            {
                theme = "World History";
            }
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

        // Resetar flag de operação e limpar operações pendentes
        if (gameConnection != null)
        {
            gameConnection.ResetRoomOperation();
            gameConnection.ClearPendingOperations();
        }

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

        // Resetar flag de operação e limpar operações pendentes
        if (gameConnection != null)
        {
            gameConnection.ResetRoomOperation();
            gameConnection.ClearPendingOperations();
        }

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
