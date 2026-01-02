using UnityEngine;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Auth;

public class CreateRoom : MonoBehaviour
{
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private SoundEffects soundEffects;

    public void OnMouseDown()
    {
        soundEffects.PressButtonSound();

        // Usa o nome do usuário logado (da tag)
        if (string.IsNullOrEmpty(SessionData.Nickname))
        {
            SessionData.Nickname = TokenManager.UserName;
        }

        var connection = FindFirstObjectByType<GameConnection>();
        connection.CreateRoom();

        var menu = FindFirstObjectByType<Menu>();
        menu.DisableMenu();
    }
}
