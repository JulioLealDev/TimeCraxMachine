using UnityEngine;
using Photon.Pun;

public class EventCard : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public int slotCount;

    // Start is called before the first frame update
    void Start()
    {
        camera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawEventCard()
    {
        photonView.RPC("DrawingEventCard", RpcTarget.All);
    }

    [PunRPC]
    public void DrawingEventCard()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.tag = "Drew";
        gameObject.GetComponent<Animator>().SetBool("drawingEventCard", true);
    }

    public void ZoomTimeline()
    {
        camera.ZoomTimeline();
    }

    public void waitToDistance()
    {
        Invoke("DistanceTimeline", 3.3f);
    }

    public void DistanceTimeline()
    {
        camera.DistanceTimeline();
    }

    public void ResetStatusCard()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Animator>().SetBool("wrongSlot", false);
        gameObject.GetComponent<Animator>().SetBool("drawingEventCard", false);
        gameObject.GetComponent<Animator>().SetInteger("slotClicked", 0);
    }

}
