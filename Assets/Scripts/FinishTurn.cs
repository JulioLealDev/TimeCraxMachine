using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Managers;

public class FinishTurn : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private SoundEffects soundEffects;

    private Timeline cachedTimeline;
    private GameManager cachedGameManager;

    private void Start()
    {
        cachedTimeline = FindFirstObjectByType<Timeline>(FindObjectsInactive.Include);
        cachedGameManager = FindFirstObjectByType<GameManager>();
    }

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
        var local = PlayerManager.Instance?.GetLocalPlayer();
        return local != null && local.GetYourTurn();
    }

    [PunRPC]
    public void ClickFinish()
    {
        soundEffects.PressHudButtonSound();

        GameManager.IsInTurnTransition = true;
        InputBlocker.Block();

        // Desativar imediatamente o collider da timeline ao finalizar o turno
        if (cachedTimeline != null) cachedTimeline.ActiveTimeline(false);
    }

    public void Finish()
    {
        animator.SetBool("finishTurn", false);
        cachedGameManager.EndTurn();

        GameManager.ResetClick(this);
    }
}
