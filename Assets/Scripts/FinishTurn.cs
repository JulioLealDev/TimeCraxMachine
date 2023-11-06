using UnityEngine;

public class FinishTurn : MonoBehaviour
{
    public Animator animator;

    private void OnMouseDown()
    {
        Debug.Log("Clicou no Finish");
        animator.SetBool("finishTurn", true);
        //chamar um texto pedindo confirmação
        Invoke("Finish", 1f);

    }

    public void Finish()
    {
        animator.SetBool("finishTurn", false);
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.EndTurn();
    }
}
