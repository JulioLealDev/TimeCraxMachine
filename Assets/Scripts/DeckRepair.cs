using UnityEngine;
using Photon.Pun;

public class DeckRepair : MonoBehaviourPunCallbacks
{
    public DeckEvent deckEvent;
    public GameManager gameManager;
    public Canvas gameInfo;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
       
    }

    public void OnMouseDown()
    {
        
        if (gameObject.CompareTag("Disabled"))
        {
            var players = FindObjectsOfType<PlayerScript>();
            foreach (var player in players)
            {
                if (player.GetYourTurn())
                {
                    if (player.GetNumberOfRepairsCards() == 5)
                    {
                        Debug.Log("Você já possui 5 cartas");
                    }
                    else
                    {
                        Debug.Log("Você já realizou uma ação neste turno");

                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "ActionInfoBackground")
                            {
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        Invoke("HideActionInfo", 1.5f);
                    }
                }

            }


        }
        else
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Instantiate("repairCard", new Vector3(0.604300022f, 0.0707999989f, 0.280999988f), Quaternion.identity);
            }

            gameManager.BlockActions();

        }

    }

    public void HideActionInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ActionInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("DisableGameInfo", 0.5f);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
    }


}
