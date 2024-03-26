using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using UnityEngine.UI;

public class LobbyOptions : MonoBehaviourPunCallbacks
{
    public GameObject lobbyBackgroundScreen;
    public GameConnection gameConnection;
    public GameObject roomNameInput;
    public GameObject maxPlayersDropdown;
    public GameObject difficultyDropdown;
    public GameObject privateDropdown;
    public GameObject themeDropdown;
    public GameObject passwordInput;
    public GameObject passwordLabel;    
    public Canvas loading;
    public GameObject gameManager;
    public GameObject createRoom;
    public GameObject roomScreen;
    public GameObject lobbyScreen;
    public InputField nameDisplay;
    public GameObject roomListContent;
    public GameObject createRoomButton;
    public GameObject roomNameWarning;
    public GameObject passwordWarning;
    public GameObject alreadyExistNameWarning;

    bool privateRoom = false;

    public void ClickStart()
    {
        photonView.RPC("StartMatch", RpcTarget.All);
    }

    [PunRPC]
    public void StartMatch()
    {
        PlayerPrefs.SetString("gameStarted", "true");
        PhotonNetwork.CurrentRoom.IsOpen = false;
        gameObject.SetActive(false);
        gameManager.SetActive(true);
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
        Debug.Log("CancelCreateRoom clicked");
        createRoom.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);

        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
        nameDisplay.gameObject.SetActive(true);
    }

    public void CancelRoomScreen()
    {
        Debug.Log("CancelRoomScreen clicked");
        roomScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);


        //if (PhotonNetwork.LocalPlayer.IsMasterClient)
        //{
        //    Debug.Log("É o master");
           PhotonNetwork.LeaveRoom(true);
        //}

        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
        nameDisplay.gameObject.SetActive(true);
    }

    public void BackLobbyScreen()
    {

        foreach (Transform room in roomListContent.GetComponentsInChildren<Transform>())
        {
            if (!room.CompareTag("Undestructable"))
            {
                Debug.Log("Destruindo sala : " + room.GetInstanceID());
                Debug.Log("Destruindo sala : " + room.GetComponentInChildren<TMP_Text>().text);
                Destroy(room.gameObject);
            }

        }

        Debug.Log("BackLobbyScreen clicked");
        lobbyScreen.SetActive(false);
        lobbyBackgroundScreen.SetActive(false);

        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
        nameDisplay.gameObject.SetActive(true);

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
