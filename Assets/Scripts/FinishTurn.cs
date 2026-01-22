using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class FinishTurn : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private SoundEffects soundEffects;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    public void OnMouseDown()
    {
        // Bloquear clique durante animações de câmera
        if (CameraController.IsAnimating) return;

        // Proteção contra clique duplo
        if (isProcessingClick) return;

        // Verificar se é o turno do jogador local antes de processar
        if (!IsMyTurn())
        {
            DebugHelper.Log("[FinishTurn] Não é meu turno, ignorando clique");
            return;
        }

        isProcessingClick = true;

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
        DebugHelper.Log($"[FinishTurn.IsMyTurn] Verificando turno. Total players: {players.Length}");

        foreach (var player in players)
        {
            bool isMine = player.photonView.IsMine;
            bool yourTurn = player.GetYourTurn();
            DebugHelper.Log($"[FinishTurn.IsMyTurn] Player: {player.nickname}, IsMine: {isMine}, YourTurn: {yourTurn}");

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
        DebugHelper.Log("Click Finish");
        soundEffects.PressHudButtonSound();
    }

    public void Finish()
    {
        DebugHelper.Log("Finish");
        animator.SetBool("finishTurn", false);
        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager.EndTurn();

        // Resetar proteção contra clique duplo para o próximo turno
        isProcessingClick = false;
    }
}
