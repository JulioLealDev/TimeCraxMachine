using UnityEngine;
using Photon.Pun;

public class Timeline : MonoBehaviourPunCallbacks
{
    private bool zoom;
    private FinishTurn endButton;

    // Start is called before the first frame update
    void Start()
    {
        zoom = false;
        endButton = FindFirstObjectByType<FinishTurn>(FindObjectsInactive.Include);
    }

    public void OnMouseDown()
    {
        if (gameObject.CompareTag("Selectable"))
        {

            endButton.GetComponent<MeshCollider>().enabled = zoom;
            ActiveTimeline(false);
            photonView.RPC("ClickTimeline", RpcTarget.All);
        }

    }

    [PunRPC]
    public void ClickTimeline()
    {
        zoom = !zoom;

        var camera = FindFirstObjectByType<CameraController>();
        if (zoom)
        {
            camera.ZoomTimeline();
        }
        else
        {
            camera.DistanceTimeline();
        }

       
    }

    public void ActiveTimeline(bool activate)
    {
        gameObject.GetComponent<MeshCollider>().enabled = activate;
    }


}
