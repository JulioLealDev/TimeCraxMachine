
using UnityEngine;
using Photon.Pun;

public class Camera : MonoBehaviourPunCallbacks
{
    public Animator animator;
    public GameObject suitTop;
    public GameConnection gameConnection;
    private Timeline timeline;
    private EventSlot slot;

    private void Awake()
    {
        PlayerPrefs.SetString("gameStarted", "false");
        gameConnection.EnterServerAndLobby();
        timeline = FindObjectOfType<Timeline>();
        slot = FindObjectOfType<EventSlot>();

    }
    void Update()
    {

        if (PlayerPrefs.GetString("gameStarted") == "false")
        {
            var menu = FindObjectOfType<Menu>();
            if (PhotonNetwork.CountOfRooms > 0)
            {
                menu.DisableRoulette();
                //Debug.Log("Sala criada: " + PhotonNetwork.CountOfRooms);
            }
            else
            {
                menu.EnableRoulette();
                //Debug.Log("Não há Sala criada: " + PhotonNetwork.CountOfRooms);
            }
        }
        
    }

    void Start()
    {
        animator.SetBool("enterMenu", true);
    }

    void AwaitZoomAnimation()
    {

        //Active menu to interact
        Transform[] childrens = suitTop.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childrens.Length; i++)
        {
            if (childrens[i].CompareTag("Selectable") || childrens[i].CompareTag("Roulette") )
            {
                childrens[i].gameObject.GetComponent<MeshCollider>().enabled = true;
            }
        }
        GameObject inputName = GameObject.FindGameObjectWithTag("InputName");
        //Debug.Log("name: "+inputName.name);
        inputName.GetComponent<Canvas>().enabled = true;
    }

    void AwaitDistanceCamera()
    {
        suitTop.GetComponent<Animator>().enabled = true;
        suitTop.GetComponent<Animator>().SetBool("openSuit", true);
    }


    public void ZoomTimeline()
    {
        //Debug.Log("Zooom");
        gameObject.GetComponent<Animator>().SetBool("distanceZoom", false);
        gameObject.GetComponent<Animator>().SetBool("zoomTimeline", true);
    }


    public void DistanceTimeline()
    {
        //Debug.Log("Distance");
        gameObject.GetComponent<Animator>().SetBool("zoomTimeline", false);
        gameObject.GetComponent<Animator>().SetBool("distanceZoom", true);

       // var gameManager = FindObjectOfType<GameManager>();
        //gameManager.ActivateFinishButton(true);
    }

    void AwaitZoomTimeline()
    {
        if(timeline.photonView.IsMine)
        {
            //Debug.Log("entrou");
            if (CheckIfCardWasDrew())
            {
                //Debug.Log("tem drew");
                slot.SetUpSlots(true, "Selectable");
            }
            else
            {
                //Debug.Log("nao tem drew");
                timeline.ActiveTimeline(true);
            }
        }
    }


    void AwaitDistanceTimeline()
    {
        if (timeline.photonView.IsMine)
        {
            timeline.ActiveTimeline(true);
            if (CheckIfCardWasDrew())
            {
                slot.SetUpSlots(false, "Undestructable");
            }
        }
    }
    
    bool CheckIfCardWasDrew()
    {
        var eventCards = FindObjectsOfType<EventCard>();
        foreach (var card in eventCards)
        {
            if (card.CompareTag("Drew"))
            {
                return true;
            }
        }
        return false;
    }

}

