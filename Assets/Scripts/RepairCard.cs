using UnityEngine;
using Photon.Pun;
using System.Linq;

public class RepairCard : MonoBehaviourPunCallbacks
{
    private PlayerScript[] players;
    // Start is called before the first frame update
    void Start()
    {
        players = FindObjectsOfType<PlayerScript>();

        DrawRepairCard();

    }
    public void DrawRepairCard()
    {
        gameObject.GetComponent<Animator>().enabled = true;
        gameObject.GetComponent<Animator>().SetBool("drawingRepairCard", true);
    }

    public void CheckingPlayer()
    {
        //var players = FindObjectsOfType<PlayerScript>();
        foreach (var player in players)
        {
            if (player.GetComponent<PhotonView>().OwnerActorNr == photonView.OwnerActorNr)
            {
                player.DrawRepairCard();
                ShowRepairCardOnHand(player.GetNumberOfRepairsCards());
            }
        }
    }

    public void ShowRepairCardOnHand(int numberOfRepairCards)
    {
        gameObject.GetComponent<Animator>().enabled = false;
        gameObject.SetActive(false);
        ShowRepairCards(numberOfRepairCards);
    }

    public void ShowRepairCards(int numberOfRepairCards)
    {
        Debug.Log("number of cards: " + numberOfRepairCards);

        switch (numberOfRepairCards)
        {
            case 1:
                gameObject.transform.SetPositionAndRotation(new Vector3(0f, 0.648899972f, 0.638700008f), new Quaternion(0.906307876f, 0, 0, -0.42261827f));
                break;
            case 2:
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0196000002f, 0.647700012f, 0.635900021f), new Quaternion(-0.893287599f, 0.0578520186f, -0.131124616f, 0.426024318f));
                break;
            case 3:
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0238000005f, 0.648599982f, 0.644800007f), new Quaternion(-0.9102512f, -0.0436024554f, 0.0974093974f, 0.400066316f));
                break;
            case 4:
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0368999988f, 0.642799973f, 0.637899995f), new Quaternion(-0.872396052f, 0.0844448283f, -0.222514987f, 0.426944137f));
                break;
            case 5:
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0425999984f, 0.648100019f, 0.655099988f), new Quaternion(-0.893432021f, -0.0762165561f, 0.201961488f, 0.393931329f));
                break;
            default:
                Debug.Log("Não possui cartas");
                break;
        }

        gameObject.SetActive(true);
    }
    public void DestroyRepairCards()
    {
        Destroy(gameObject);   
    }
}
