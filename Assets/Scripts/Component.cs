using UnityEngine;
using Photon.Pun;

public class Component : MonoBehaviourPunCallbacks
{
    public int componentId;
    public int malfunctions = 0;
    void Start()
    {

    }
    void Update()
    {
        if(malfunctions > 1)
        {
            //Debug.Log("GAME OVER!!");
        }
    }
    public void OnMouseDown()
    {
        if(gameObject.CompareTag("Selectable")) 
        {
            var players = FindObjectsOfType<PlayerScript>();
            foreach (var player in players)
            {
                Debug.Log("Vez de " + player.nickname + " : " + player.GetYourTurn() + " -- cartas: " + player.GetNumberOfRepairsCards());
                if (player.GetYourTurn() && player.GetNumberOfRepairsCards() >= players.Length)
                {
                    photonView.RPC("RemoveMalfunction", RpcTarget.All);
                    player.RepairComponent(players.Length);
                    Debug.Log("component: " + componentId);
                }
                else
                {
                    Debug.Log("You need " + players.Length + " Repair Cards to repair one component!");
                }
            }
        }
        else
        {
            Debug.Log("Você já realizou uma ação nesse turno");
        }

    }

    public void AddMalfunction()
    {
        //ativar animação
        //gameObject.GetComponent<MeshCollider>().enabled = true;
        malfunctions++;
    }

    [PunRPC]
    public void RemoveMalfunction()
    {
        //ativar animação
        malfunctions--;
        gameObject.GetComponent<MeshCollider>().enabled = false;
    }


}
