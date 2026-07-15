using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class FinishTurn : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private SoundEffects soundEffects;

    public void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (CameraController.IsAnimating) return;
        if (!GameManager.TryBeginClick(this)) return;

        if (!IsMyTurn())
        {
            GameManager.ResetClick(this);
            return;
        }

        InputBlocker.Block();

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
            bool isMine = player.photonView.IsMine;
            bool yourTurn = player.GetYourTurn();

            if (isMine && yourTurn)
            {
                return true;
            }
        }
        return false;
    }

    [PunRPC]
    public void ClickFinish()
    {
        soundEffects.PressHudButtonSound();

        // Marcar que está em transição de turno
        GameManager.IsInTurnTransition = true;

        InputBlocker.Block();
    }

    public void Finish()
    {
        animator.SetBool("finishTurn", false);
        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager.EndTurn();

        GameManager.ResetClick(this);
    }
}
