using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Auth;

public class EnterRoom : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cam;
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
        connection.Lobby();

        var menu = FindFirstObjectByType<Menu>();
        menu.DisableMenu();

        // Resetar após um delay
        this.DelayedCall(1f, ResetClickProtection);
    }

    private void ResetClickProtection()
    {
        isProcessingClick = false;
    }

    void AwaitGreenButtonAnimation()
    {
        cam.gameObject.GetComponent<Animator>().SetBool("enterMenu", false);
        cam.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);
        animator.SetBool("startGame", false);
    }
}
