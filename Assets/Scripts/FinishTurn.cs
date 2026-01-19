using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class FinishTurn : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private SoundEffects soundEffects;

    public void OnMouseDown()
    {
        // Verificar se é o turno do jogador local antes de processar
        if (!IsMyTurn())
        {
            DebugHelper.Log("[FinishTurn] Não é meu turno, ignorando clique");
            return;
        }

        DebugHelper.Log("Clicou no Finish");
        gameObject.GetComponent<MeshCollider>().enabled = false;
        photonView.RPC("ClickFinish", RpcTarget.All);

        animator.SetBool("finishTurn", true);
        this.DelayedCall(0.5f, Finish);
    }

    /// <summary>
    /// Verifica se é o turno do jogador local
    /// </summary>
    private bool IsMyTurn()
    {
        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.photonView.IsMine && player.GetYourTurn())
            {
                return true;
            }
        }
        return false;
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
