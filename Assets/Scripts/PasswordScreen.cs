using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;

public class PasswordScreen : MonoBehaviour
{
    public Image background02;
    public GameObject passwordInput;
    public Button enterPasswordButton;
    public GameConnection gameConnection;
    public LobbyOptions lobbyOptions;
    public GameObject warning;
    private string roomName;

    public void ActivateBackground(bool activate)
    {
        background02.gameObject.SetActive(activate);
    }

    public void SetRoomName(string name)
    {
        roomName = name;
    }

    public void CancelPasswordButton()
    {
        passwordInput.GetComponent<TMP_InputField>().text = " ";
        lobbyOptions.ActivateButtons(true);
        ActivateBackground(false);
        gameObject.SetActive(false);
    }

    public void EnterPasswordButton()
    {
        DebugHelper.Log("EnterPasswordButton");
        bool correctPass = gameConnection.CheckPassword(roomName, passwordInput.GetComponent<TMP_InputField>().text);

        if (correctPass)
        {
            // Reativar botões do lobby antes de entrar na sala
            lobbyOptions.ActivateButtons(true);

            ActivateBackground(false);
            gameObject.SetActive(false);
            gameConnection.JoinRoomInList(roomName);
            passwordInput.GetComponent<TMP_InputField>().text = " ";
        }
        else
        {
            DebugHelper.Log("Password Incorreto");
            passwordInput.GetComponent<TMP_InputField>().text = " ";
            warning.gameObject.SetActive(true);
            warning.gameObject.GetComponent<Animator>().SetBool("wrongPassword", true);
            this.DelayedCall(1.5f, WrongPassword);
        }
    }

    private void WrongPassword()
    {
        warning.gameObject.SetActive(false);
        warning.gameObject.GetComponent<Animator>().SetBool("wrongPassword", false);
    }

    public void Update()
    {
        if (string.IsNullOrWhiteSpace(passwordInput.GetComponent<TMP_InputField>().text))
        {
            enterPasswordButton.interactable = false;
        }
        else
        {
            enterPasswordButton.interactable = true;
        }
    }
}
