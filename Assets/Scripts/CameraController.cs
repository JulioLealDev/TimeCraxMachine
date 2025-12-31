using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class CameraController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject suitTop;
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private bool fullScreen = true;

    // Cached components
    private Timeline timeline;
    private EventSlot slot;
    private Animator suitTopAnimator;
    private Menu menuCache;
    private GameManager gameManagerCache;

    private void Awake()
    {
        SessionData.GameStarted = false;
        gameConnection.EnterServerAndLobby();
        timeline = FindFirstObjectByType<Timeline>();
        slot = FindFirstObjectByType<EventSlot>();
        menuCache = FindFirstObjectByType<Menu>();
        gameManagerCache = FindFirstObjectByType<GameManager>();

        if (suitTop != null)
        {
            suitTopAnimator = suitTop.GetComponent<Animator>();
        }
    }

    void Start()
    {
        int targetHeight = Screen.width * 9 / 16;
        Screen.SetResolution(Screen.width, targetHeight, fullScreen);
        animator.SetBool("enterMenu", true);
    }

    void AwaitZoomAnimation()
    {
        Transform[] childrens = suitTop.GetComponentsInChildren<Transform>();
        for (int i = 0; i < childrens.Length; i++)
        {
            if (childrens[i].CompareTag("Selectable"))
            {
                childrens[i].gameObject.GetComponent<MeshCollider>().enabled = true;
            }
        }
        GameObject inputName = GameObject.FindGameObjectWithTag("InputName");
        inputName.GetComponent<Canvas>().enabled = true;
    }

    void AwaitDistanceCamera()
    {
        suitTopAnimator.enabled = true;
        suitTopAnimator.SetBool("openSuit", true);
    }

    public void ZoomTimeline()
    {
        animator.SetBool("distanceZoom", false);
        animator.SetBool("zoomTimeline", true);
    }

    public void DistanceTimeline()
    {
        animator.SetBool("zoomTimeline", false);
        animator.SetBool("distanceZoom", true);
        this.DelayedCall(1.5f, ActivateEndButton);
    }

    public void ActivateEndButton()
    {
        gameManagerCache.ActivateEnd();
    }

    void AwaitZoomTimeline()
    {
        if (timeline.photonView.IsMine)
        {
            if (CheckIfCardWasDrew())
            {
                slot.SetUpSlots(true, "Selectable");
            }
            else
            {
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
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
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
