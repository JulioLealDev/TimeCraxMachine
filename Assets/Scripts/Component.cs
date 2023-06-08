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

        var parent = gameObject.transform.parent;
        if (parent.name != "Enviroment")
        {
            Debug.Log("parent: " + parent.name);
            Transform[] opcoes = parent.GetComponentsInChildren<Transform>();
            foreach (var opc in opcoes)
            {
                Debug.Log("opc: " + opc.name);
                if (opc.GetComponent<Animator>() != null)
                {
                    opc.GetComponent<Animator>().SetBool("malfunction", true);
                }
            }
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("malfunction", true);
        }

        malfunctions++;

        if (malfunctions > 1)
        {
            GameObject gameOver = GameObject.FindGameObjectWithTag("GameOver");
            gameOver.transform.GetChild(0).gameObject.SetActive(true);
            Debug.Log("name ---> " + gameOver.name);
        }

    }

    [PunRPC]
    public void RemoveMalfunction()
    {
        //ativar animação
        var parent = gameObject.transform.parent;
        if (parent.name != "Enviroment")
        {
            Debug.Log("parent: " + parent.name);
            Transform[] opcoes = parent.GetComponentsInChildren<Transform>();
            foreach (var opc in opcoes)
            {
                Debug.Log("opc: " + opc.name);
                if (opc.GetComponent<Animator>() != null)
                {
                    opc.GetComponent<Animator>().SetBool("malfunction", false);
                }
            }
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("malfunction", false);
        }

        malfunctions--;
        gameObject.GetComponent<MeshCollider>().enabled = false;
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.BlockActions();
    }


}
