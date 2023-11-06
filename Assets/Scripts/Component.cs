using UnityEngine;
using Photon.Pun;
using TMPro;

public class Component : MonoBehaviourPunCallbacks
{
    public int componentId;
    public int malfunctions = 0;
    public GameObject gameInfo;
    void Start()
    {

    }
    void Update()
    {

    }
    public void OnMouseDown()
    {

        if (gameObject.CompareTag("Selectable")) 
        {
            var players = FindObjectsOfType<PlayerScript>();
            foreach (var player in players)
            {
                Debug.Log("Vez de " + player.nickname + " : " + player.GetYourTurn());
                if (player.GetYourTurn())
                {

                    Debug.Log("Number od cards: " + player.GetNumberOfRepairsCards());

                    if(player.GetNumberOfRepairsCards() >= players.Length)
                    {

                        photonView.RPC("RemoveMalfunction", RpcTarget.All);
                        player.RepairComponent(players.Length);
                        Debug.Log("component: " + componentId);

                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "RepairInfoBackground")
                            {
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        Invoke("HideRepairInfo", 1.5f);
                    }
                    else
                    {
                        Debug.Log("You need " + players.Length + " Repair Cards to repair one component!");


                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "ComponentInfoBackground")
                            {
                                info.GetComponentInChildren<TextMeshProUGUI>().text = "You need " + players.Length + " Repair Cards to repair one component!";
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        Invoke("HideComponentInfo", 1.5f);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Você já realizou uma ação nesse turno");

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
    public void HideRepairInfo()
    {
        //Debug.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "RepairInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("DisableGameInfo", 0.5f);
    }
    public void HideActionInfo()
    {
        //Debug.Log("HideRoundInfo()");
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
    public void HideComponentInfo()
    {

        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ComponentInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("ComponentInfoBackground", 0.5f);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
    }

    public void AddMalfunction()
    {
        //ativar animação
        //gameObject.GetComponent<MeshCollider>().enabled = true;

        var parent = gameObject.transform.parent;
        if (parent.name != "Enviroment")
        {
            //Debug.Log("parent: " + parent.name);
            Transform[] opcoes = parent.GetComponentsInChildren<Transform>();
            foreach (var opc in opcoes)
            {
                //Debug.Log("opc: " + opc.name);
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
            Invoke("EndGame", 3f);
        }

    }

    public void EndGame()
    {
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.DeactivateAll();

        GameObject gameOver = GameObject.FindGameObjectWithTag("GameOver");
        gameOver.transform.GetChild(0).gameObject.SetActive(true);
        Debug.Log("name ---> " + gameOver.name);
    }

    [PunRPC]
    public void RemoveMalfunction()
    {
        //ativar animação
        var parent = gameObject.transform.parent;
        if (parent.name != "Enviroment")
        {
            //Debug.Log("parent: " + parent.name);
            Transform[] opcoes = parent.GetComponentsInChildren<Transform>();
            foreach (var opc in opcoes)
            {
               // Debug.Log("opc: " + opc.name);
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
