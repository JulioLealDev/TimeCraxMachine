using UnityEngine;
using Photon.Pun;

public class Timeline : MonoBehaviourPunCallbacks
{
    private bool zoom;
    // Start is called before the first frame update
    void Start()
    {
        zoom = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        if (gameObject.CompareTag("Selectable"))
        {
            ActiveTimeline(false);
            photonView.RPC("ClickTimeline", RpcTarget.All);
        }

    }

    [PunRPC]
    public void ClickTimeline()
    {
        zoom = !zoom;

        var camera = FindObjectOfType<Camera>();
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
