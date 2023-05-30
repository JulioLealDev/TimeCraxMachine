using UnityEngine;
using Photon.Pun;

public class DeckRepair : MonoBehaviourPunCallbacks
{
    public DeckEvent deckEvent;
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
            //Debug.Log("Você já possui 5 cartas");
        }
        else
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Instantiate("repairCard", new Vector3(0.604300022f, 0.0707999989f, 0.280999988f), Quaternion.identity);
            }
            gameObject.tag = "Disabled";
            deckEvent.tag = "Disabled";

        }

    }


}
