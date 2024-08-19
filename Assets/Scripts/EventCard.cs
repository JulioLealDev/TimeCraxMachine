using UnityEngine;
using Photon.Pun;
using System;

public class EventCard : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public int slotCount;
    public int slotYear;

    // Start is called before the first frame update
    void Start()
    {
        camera = FindObjectOfType<Camera>();
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

    public void ActivateEndButton()
    {
        Debug.Log("ActivateEndButton");
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.ActivateEnd();
    }

}
