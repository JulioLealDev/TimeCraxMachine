using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class FinishTurn : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private SoundEffects soundEffects;

    public void OnMouseDown()
    {
        DebugHelper.Log("Clicou no Finish");
        gameObject.GetComponent<MeshCollider>().enabled = false;
        photonView.RPC("ClickFinish", RpcTarget.All);

        animator.SetBool("finishTurn", true);
        //chamar um texto pedindo confirma��o
        this.DelayedCall(0.5f, Finish);

    }

    [PunRPC]
    public void ClickFinish()
    {
        DebugHelper.Log("Click Finish");
        soundEffects.PressHudButtonSound();
    }

    public void Finish()
    {
        DebugHelper.Log("Finish");
        animator.SetBool("finishTurn", false);
        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager.EndTurn();
    }
}
