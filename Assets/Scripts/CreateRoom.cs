using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Auth;
using TimeCrax.Managers;

public class CreateRoom : MonoBehaviour
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

        if (gameConnection.CreateRoom())
            menuManager.DesablingMenuOptions();

        this.DelayedCall(1f, () => GameManager.ResetClick(this));
    }
}
