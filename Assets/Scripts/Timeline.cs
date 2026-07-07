using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class Timeline : MonoBehaviourPunCallbacks
{
    private bool zoom;
    private FinishTurn endButton;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    // Start is called before the first frame update
    void Start()
    {
        zoom = false;
        endButton = FindFirstObjectByType<FinishTurn>(FindObjectsInactive.Include);
    }

    public void OnMouseDown()
    {
        DebugHelper.Log($"[Timeline] clique: blocked={InputBlocker.IsBlocked}, animating={CameraController.IsAnimating}, processing={isProcessingClick}");
        if (InputBlocker.IsBlocked) return;
        if (CameraController.IsAnimating) return;

        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        if (gameObject.CompareTag("Selectable"))
        {
            // Calcular novo estado localmente
            bool newZoom = !zoom;

            // Só habilitar o botão End Turn se for o turno do jogador local
            if (IsMyTurn())
            {
                endButton.GetComponent<MeshCollider>().enabled = zoom;
            }
            ActiveTimeline(false);

            // Enviar estado explícito via RPC (não toggle)
            photonView.RPC("SetZoomState", RpcTarget.All, newZoom);
        }
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
    public void SetZoomState(bool newZoom)
    {
        zoom = newZoom;

        var camera = FindFirstObjectByType<CameraController>();
        if (zoom)
        {
            camera.ZoomTimeline();
        }
        else
        {
            camera.DistanceTimeline();
        }

        // Resetar proteção contra clique duplo após animação da câmera
        this.DelayedCall(1.5f, ResetClickProtection);
    }

    /// <summary>
    /// Reseta a proteção contra clique duplo
    /// </summary>
    public void ResetClickProtection()
    {
        isProcessingClick = false;
    }

    public void ActiveTimeline(bool activate)
    {
        gameObject.GetComponent<MeshCollider>().enabled = activate;
    }

    /// <summary>
    /// Reseta o estado do zoom (usado ao iniciar nova partida)
    /// </summary>
    public void ResetZoom()
    {
        zoom = false;
    }
}
