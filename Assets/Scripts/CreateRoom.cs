using UnityEngine;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Auth;

public class CreateRoom : MonoBehaviour
{
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private SoundEffects soundEffects;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    public void OnMouseDown()
    {
        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

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

        // Resetar após um delay
        this.DelayedCall(1f, ResetClickProtection);
    }

    private void ResetClickProtection()
    {
        isProcessingClick = false;
    }
}
