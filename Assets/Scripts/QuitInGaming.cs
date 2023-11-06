using UnityEngine;

public class QuitInGaming : MonoBehaviour
{
    public Animator animator;

    private void OnMouseDown()
    {
        Debug.Log("Clicou no Quit");
        animator.SetBool("quitGame", true);
        //chamar um texto pedindo confirmação
        Invoke("QuitGame", 1.5f);

    }

    public void QuitGame()
    {
        animator.SetBool("finishTurn", false);
        Application.Quit();
    }
}
