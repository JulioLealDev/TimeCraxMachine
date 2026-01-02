using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Auth;

public class EnterRoom : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cam;
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
        connection.Lobby();

        var menu = FindFirstObjectByType<Menu>();
        menu.DisableMenu();
    }

    void AwaitGreenButtonAnimation()
    {
        cam.gameObject.GetComponent<Animator>().SetBool("enterMenu", false);
        cam.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);
        animator.SetBool("startGame", false);
    }
}
