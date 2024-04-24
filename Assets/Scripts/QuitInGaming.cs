using UnityEngine;

public class QuitInGaming : MonoBehaviour
{
    public Animator animator;
    public SoundEffects soundEffects;

    private void OnMouseDown()
    {
        Debug.Log("1 -- Clicou no Quit");

        soundEffects.PressButtonSound();

        animator.SetBool("quitGame", true);
        //chamar um texto pedindo confirmação
        Invoke("QuitGame", 0.7f);

    }

    public void QuitGame()
    {
        animator.SetBool("quitGame", false);
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.BackToMenu();
        
    }
}
