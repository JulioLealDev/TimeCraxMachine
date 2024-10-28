using UnityEngine;

public class QuitInGaming : MonoBehaviour
{
    public Animator animator;
    public SoundEffects soundEffects;
    public GameManager gameManager;

    private void OnMouseDown()
    {
        Debug.Log("1 -- Clicou no Quit");

        soundEffects.PressButtonSound();

        animator.SetBool("quitGame", true);
        //chamar um texto pedindo confirmação
        Invoke("QuitGame", 1f);

    }

    public void QuitGame()
    {
        animator.SetBool("quitGame", false);

        Invoke("CloseHUD", 2f);

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
