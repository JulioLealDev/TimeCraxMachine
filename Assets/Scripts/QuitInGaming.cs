using UnityEngine;
using TimeCrax.Core;

public class QuitInGaming : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private GameManager gameManager;

    private void OnMouseDown()
    {
        DebugHelper.Log("1 -- Clicou no Quit");

        soundEffects.PressButtonSound();

        animator.SetBool("quitGame", true);
        //chamar um texto pedindo confirma��o
        this.DelayedCall(1f, QuitGame);

    }

    public void QuitGame()
    {
        animator.SetBool("quitGame", false);

        this.DelayedCall(2f, CloseHUD);

        gameManager.BackToMenu();

    }

    public void CloseHUD()
    {
        gameManager.hud.SetActive(false);

        gameManager.DeactivateAll();
        gameManager.ResetAllComponents();
        gameManager.ResetAllPlatenames();
    }
}
