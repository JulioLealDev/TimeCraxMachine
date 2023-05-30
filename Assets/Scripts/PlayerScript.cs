using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Runtime.ConstrainedExecution;
using System.Linq;

public class PlayerScript : MonoBehaviourPunCallbacks
{

    public int numberRepairCards;
    public string nickname;
    public int index;
    private bool yourTurn = false;

    // Start is called before the first frame update
    void Start()
    {
        numberRepairCards = 0;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == photonView.ControllerActorNr)
            {
                nickname = PhotonNetwork.PlayerList[i].NickName;
                index = i;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawEventCard()
    {
        Debug.Log("You draw one EventCard!");
    }
    public void DrawRepairCard()
    {
        numberRepairCards++;
    }

    public int GetNumberOfRepairsCards()
    {
        return numberRepairCards;
    }

    public void GiveRepairCard(PlayerScript otherPlayer)
    {
        otherPlayer.numberRepairCards++;
    }

    public void RepairComponent(int cards)
    {
        Debug.Log("cartas: " + cards);
        if(numberRepairCards >= cards) 
        {
            numberRepairCards -= cards;
            DestroyRepairCards(cards);
        }
        else
        {
            Debug.Log("You need "+cards+" Repair Cards to repair one component!");
        }
        
    }

    public void ShowRepair()
    {
        //photonView.RPC("ShowRepairCards", RpcTarget.Others);
        ShowRepairCards();
    }

    //[PunRPC]
    public void ShowRepairCards()
    {
        Debug.Log("number of cards: " + numberRepairCards);

        if(photonView.IsMine)
        {
            Debug.Log("DEntro do if");
            switch (numberRepairCards)
            {
                case 1:
                    Debug.Log("instantiate 1 ");
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0f, 0.649999976f, 0.639999986f), new Quaternion(0.906307876f, 0, 0, -0.42261827f)).tag = "Undestructable";
                    break;
                case 2:
                    Debug.Log("instantiate 2 ");
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0.0199999996f, 0.629999995f, 0.629999995f), new Quaternion(0.936116874f, 0.0298090316f, 0.0818996653f, -0.340718627f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(-0.0199999996f, 0.649999976f, 0.639999986f), new Quaternion(0.936116874f, -0.0298090316f, -0.0818996653f, -0.340718627f)).tag = "Undestructable";
                    break;
                case 3:
                    Debug.Log("instantiate 3 ");
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0.039999999f, 0.612200022f, 0.629400015f), new Quaternion(0.925416648f, 0.0593911931f, 0.163175985f, -0.336824059f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0, 0.620000005f, 0.625f), new Quaternion(0.939692676f, 0, 0, -0.342020094f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(-0.0399999991f, 0.629999995f, 0.639999986f), new Quaternion(0.925416648f, -0.0593911596f, -0.163175911f, -0.336824059f)).tag = "Undestructable";
                    break;
                case 4:
                    Debug.Log("instantiate 4 ");
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0.0653000027f, 0.613499999f, 0.630800009f), new Quaternion(0.925416648f, 0.0593911931f, 0.163175985f, -0.336824059f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0.0275999997f, 0.620000005f, 0.625f), new Quaternion(0.936116815f, 0.0298090279f, 0.0818996504f, -0.340718597f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(-0.0199999996f, 0.624499977f, 0.627699971f), new Quaternion(0.938798249f, -0.014918711f, -0.0409888327f, -0.341694564f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(-0.0549999997f, 0.630500019f, 0.643100023f), new Quaternion(0.917418242f, -0.0740266964f, -0.203386709f, -0.333912879f)).tag = "Undestructable";
                    break;
                case 5:
                    Debug.Log("instantiate 5 ");
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0.100200005f, 0.60860002f, 0.629599988f), new Quaternion(0.887030005f, 0.000955595635f, 0.208728567f, -0.411836624f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0.0625f, 0.615100026f, 0.621299982f), new Quaternion(0.899286687f, 0.00251855375f, 0.102976725f, -0.425056487f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(0.0149000008f, 0.619599998f, 0.621299982f), new Quaternion(0.902478933f, -0.0109954523f, -0.0422107987f, -0.428519845f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(-0.0626000017f, 0.625f, 0.65079999f), new Quaternion(0.877764344f, 0.028181592f, -0.214425877f, -0.427501142f)).tag = "Undestructable";
                    PhotonNetwork.Instantiate("repairCard", new Vector3(-0.0263999999f, 0.622300029f, 0.631799936f), new Quaternion(0.88717562f, -0.0502756499f, -0.143186212f, -0.435763121f)).tag = "Undestructable";
                    break;
                default:
                    Debug.Log("Não possui cartas");
                    break;
            }
        }
        Debug.Log("fora do if");
    }
    public bool GetYourTurn()
    {
        return yourTurn;
    }

    public void SetYourTurn(bool isYourTurn)
    {
        yourTurn = isYourTurn;
    }

    public void DestroyRepairCards(int cardNumber)
    {
        var cardsList = FindObjectsOfType<RepairCard>();
        var orderedlist = cardsList.OrderByDescending(x => x.photonView.ViewID).ToList();

        for (var i = 0; i < cardNumber; i++)
        {
            Debug.Log("carta -> " + orderedlist[i].photonView.ViewID);
            orderedlist[i].GetComponent<Animator>().enabled = true;
            orderedlist[i].GetComponent<Animator>().SetBool("destroyCard", true);
        }

    }
}
