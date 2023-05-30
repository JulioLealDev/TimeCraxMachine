using UnityEngine;
using Photon.Pun;

public class LobbyOptions : MonoBehaviourPunCallbacks
{
    public Canvas loading;
    public GameObject gameManager;

    public void ClickStart()
    {
        photonView.RPC("StartMatch", RpcTarget.All);
    }
    public void ClickCancel()
    {
        gameObject.SetActive(false);
        PhotonNetwork.LeaveRoom(true);
        loading.gameObject.SetActive(true);
        Invoke("WaitCountOfRooms", 5.5f);
    }
    public void WaitCountOfRooms()
    {
        loading.gameObject.SetActive(false);
        var menu = FindObjectOfType<Menu>();
        menu.EnableMenu();
    }
    [PunRPC]
    public void StartMatch()
    {
        PlayerPrefs.SetString("gameStarted", "true");
        PhotonNetwork.CurrentRoom.IsOpen = false;
        gameObject.SetActive(false);
        gameManager.SetActive(true);
    }
}
