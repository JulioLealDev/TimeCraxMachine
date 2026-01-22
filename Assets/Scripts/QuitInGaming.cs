using UnityEngine;
using TimeCrax.Core;

public class QuitInGaming : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private GameManager gameManager;

    private bool isQuitting = false;

    private void OnMouseDown()
    {
        // Prevenir múltiplos cliques
        if (isQuitting) return;
        isQuitting = true;

        DebugHelper.Log("1 -- Clicou no Quit");

        if (soundEffects != null) soundEffects.PressButtonSound();

        if (animator != null) animator.SetBool("quitGame", true);

        this.DelayedCall(1f, QuitGame);
    }

    public void QuitGame()
    {
        if (animator != null) animator.SetBool("quitGame", false);

        this.DelayedCall(2f, CloseHUD);

        if (gameManager != null) gameManager.BackToMenu();
    }

    public void CloseHUD()
    {
        if (gameManager == null) return;

        if (gameManager.hud != null) gameManager.hud.SetActive(false);

        gameManager.DeactivateAll();
        gameManager.ResetAllComponents();
        gameManager.ResetAllPlatenames();

        // Resetar flag para permitir novo quit em próxima partida
        isQuitting = false;
    }
}
