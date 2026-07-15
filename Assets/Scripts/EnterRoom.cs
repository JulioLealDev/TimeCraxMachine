using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Auth;
using TimeCrax.Managers;

public class EnterRoom : MonoBehaviour
{
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private MenuManager menuManager;

    public void OnMouseDown()
    {
        if (!GameManager.TryBeginClick(this)) return;

        soundEffects.PressButtonSound();

        if (string.IsNullOrEmpty(SessionData.Nickname))
            SessionData.Nickname = TokenManager.UserName;

        if (gameConnection.Lobby())
            menuManager.DesablingMenuOptions();

        this.DelayedCall(1f, () => GameManager.ResetClick(this));
    }
}
