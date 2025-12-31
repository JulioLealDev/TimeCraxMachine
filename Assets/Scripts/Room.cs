
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour
{
    public TextMeshProUGUI buttonName;
    public TextMeshProUGUI isLocked;

    public void JoinRoom()
    {

        PasswordScreen passwordScreen = FindFirstObjectByType<PasswordScreen>(FindObjectsInactive.Include);
        LobbyOptions lobbyOptions = FindFirstObjectByType<LobbyOptions>(FindObjectsInactive.Include);

        if (isLocked.text == "Yes")
        {
            passwordScreen.gameObject.SetActive(true);
            passwordScreen.ActivateBackground(true);
            passwordScreen.SetRoomName(gameObject.name);
            lobbyOptions.ActivateButtons(false);
        }   
        else
        {
            GameObject.Find("GameConnection").GetComponent<GameConnection>().JoinRoomInList(gameObject.name);

        }
    }

    
}
