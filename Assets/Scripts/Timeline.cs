using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class Timeline : MonoBehaviourPunCallbacks
{
    private bool zoom;
    private FinishTurn endButton;
    private Collider timelineColliderArea;

    void Start()
    {
        zoom = false;
        endButton = FindFirstObjectByType<FinishTurn>(FindObjectsInactive.Include);

        var areaObj = transform.Find("TimelineColliderArea");
        if (areaObj != null)
            timelineColliderArea = areaObj.GetComponent<Collider>();
    }

    public void TriggerZoom()
    {
        if (InputBlocker.IsBlocked) return;
        if (CameraController.IsAnimating) return;
        if (!GameManager.TryBeginClick(this)) return;

        bool newZoom = !zoom;

        if (IsMyTurn())
            endButton.GetComponent<MeshCollider>().enabled = zoom;

        ActiveTimeline(false);

        if (PhotonNetwork.InRoom)
            photonView.RPC("SetZoomState", RpcTarget.All, newZoom);
        else
            SetZoomState(newZoom);
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
        if (camera == null)
        {
            GameManager.ResetClick(this);
            return;
        }

        if (zoom)
            camera.ZoomTimeline();
        else
            camera.DistanceTimeline();

        this.DelayedCall(1.5f, ResetClickProtection);
    }

    public void ResetClickProtection()
    {
        GameManager.ResetClick(this);
    }

    public void ActiveTimeline(bool activate)
    {
        if (timelineColliderArea != null)
        {
            if (activate)
                Debug.Log($"[Timeline] ActiveTimeline(true) — IsMalfunctionPending={GameManager.IsMalfunctionPending}");
            timelineColliderArea.enabled = activate;
        }
    }

    /// <summary>
    /// Reseta o estado do zoom (usado ao iniciar nova partida)
    /// </summary>
    public void ResetZoom()
    {
        zoom = false;
    }
}
