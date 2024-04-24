using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using UnityEngine.UI;

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
        PlayerPrefs.SetString("gameStarted", "true");
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
            Debug.Log("Não privada");
            passwordInput.GetComponent<TMP_InputField>().text = " ";
            passwordLabel.GetComponent<TextMeshProUGUI>().color = Color.gray;
        }
        else
        {
            Debug.Log("Privada");
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

        string roomName = roomNameInput.GetComponent<TMP_InputField>().text;
        string maxPlayers = maxPlayersDropdown.GetComponent<TextMeshProUGUI>().text;
        string difficulty = difficultyDropdown.GetComponent<TextMeshProUGUI>().text;
        string theme = themeDropdown.GetComponent<TextMeshProUGUI>().text;
        string password = passwordInput.GetComponent<TMP_InputField>().text;

        int max = Int32.Parse(maxPlayers.Substring(0, 1));

        gameConnection.CreatedRoom(roomName, max, difficulty, theme, password);
    }
    public void CancelCreateRoom()
    {
        soundEffects.PressHudButtonSound();

        Debug.Log("CancelCreateRoom clicked");
        createRoom.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);

        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
        nameDisplay.gameObject.SetActive(true);
    }

    public void CancelRoomScreen()
    {
        soundEffects.PressHudButtonSound();

        Debug.Log("CancelRoomScreen clicked");
        roomScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);


        //if (PhotonNetwork.LocalPlayer.IsMasterClient)
        //{
        //    Debug.Log("É o master");
           PhotonNetwork.LeaveRoom(false);
        //}

        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
        nameDisplay.gameObject.SetActive(true);
    }

    public void RefreshLobbyScreen()
    {
        Debug.Log("Refreshing clicked");
        soundEffects.PressHudButtonSound();

        DestroyRooms();

        Debug.Log("Disconecting and Reconecting");
        RefreshConection();

        Invoke("ListRooms", 0.5f);

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
        Debug.Log("BackLobbyScreen clicked");
        soundEffects.PressHudButtonSound();

        DestroyRooms();

        lobbyScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);

        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
        nameDisplay.gameObject.SetActive(true);

        Debug.Log("Disconecting and Reconecting");
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
                Debug.Log("Destruindo sala : " + room.GetInstanceID());
                Debug.Log("Destruindo sala : " + room.GetComponentInChildren<TMP_Text>().text);
                Destroy(room.gameObject);
            }

        }
    }

    public void Verifications()
    {
        string roomName = roomNameInput.GetComponent<TMP_InputField>().text;
        string password = passwordInput.GetComponent<TMP_InputField>().text;
        string privateRoom = privateDropdown.GetComponent <TextMeshProUGUI > ().text;

        bool alreadyExist = gameConnection.CheckRoomName(roomName);
        
        if (string.IsNullOrEmpty(roomName))
        {
            roomNameWarning.SetActive(true);
            roomNameWarning.GetComponent<Animator>().SetBool("roomNameIsEmpty", true);
            createRoomButton.GetComponent<Button>().enabled = false;
            Invoke("AfterClickStart", 1.5f);
        }
        else if (alreadyExist)
        {
            alreadyExistNameWarning.SetActive(true);
            alreadyExistNameWarning.GetComponent<Animator>().SetBool("alreadyExistName", true);
            createRoomButton.GetComponent<Button>().enabled = false;
            Invoke("AfterClickStart", 1.5f);
        }
        else if (privateRoom != "No" && string.IsNullOrEmpty(password))
        {
            passwordWarning.SetActive(true);
            passwordWarning.GetComponent<Animator>().SetBool("passwordIsEmpty", true);
            createRoomButton.GetComponent<Button>().enabled = false;
            Invoke("AfterClickStart", 1.5f);
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
