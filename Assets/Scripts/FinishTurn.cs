using UnityEngine;
using Photon.Pun;

public class FinishTurn : MonoBehaviourPunCallbacks
{
    public Animator animator;
    public SoundEffects soundEffects;

    public void OnMouseDown()
    {
        Debug.Log("Clicou no Finish");
        gameObject.GetComponent<MeshCollider>().enabled = false;
        photonView.RPC("ClickFinish", RpcTarget.All);

        animator.SetBool("finishTurn", true);
        //chamar um texto pedindo confirmação
        Invoke("Finish", 0.5f);

    }

    [PunRPC]
    public void ClickFinish()
    {
        Debug.Log("Click Finish");
        soundEffects.PressHudButtonSound();
    }

    public void Finish()
    {
        Debug.Log("Finish");
        animator.SetBool("finishTurn", false);
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.EndTurn();
    }
}
