using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.ComponentModel;


public class GameManager : MonoBehaviourPunCallbacks
{
    public int randomId;
    private Component[] timeCraxComponents;
    private PlayerScript[] players;
    public GameObject gameInfo;
    public GameObject deckEvent;
    public GameObject deckRepair;
    public GameObject timeline;
    public Camera gameCamera;
    public GameObject inputName;
    public GameObject suitTop;
    public GameObject gameHUD;
    private int[] playersList;
    private int round = 1;
    private int roundCompare = 1;
    private int time = 0;
    private List<int> componentList = new List<int>();

    private void Awake()
    {
        inputName.SetActive(false);
        PhotonNetwork.Instantiate("Player", new Vector3(7.224f, 1.01f, 0.83f), Quaternion.identity);
        playersList = new int[PhotonNetwork.PlayerList.Length];
    }
    void Start()
    {
        Debug.Log("Start()");

        timeCraxComponents = FindObjectsOfType<Component>();

        gameCamera.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);

        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        componentList.AddRange(numbers);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playersList[i] = PhotonNetwork.PlayerList[i].ActorNumber;

        }
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("StartGame", 6f);
        }

    }

    public void StartGame()
    {
        Debug.Log("StartGame()");
        photonView.RPC("ShowHUD", RpcTarget.All);
        //ShowHUD();
    }

    [PunRPC]
    public void ShowHUD()
    {
        gameHUD.SetActive(true);
        var outline = FindObjectOfType<OutlineAction>();
        outline.MakeObjectsSelectable();

        var components = gameHUD.GetComponentsInChildren<Transform>();
        string prefix = "PlayerImage0";
        string name;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            name = prefix + (i + 1);
            foreach (var component in components)
            {
                if (component.name == name)
                {
                    component.gameObject.GetComponent<CanvasGroup>().LeanAlpha(1f, 2f);
                    component.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.PlayerList[i].NickName;
                }
            }
        }
        Invoke("FirstTurn", 2f);
    }
    public void FirstTurn()
    {
        //Debug.Log("FirstTurn()");
        //photonView.RPC("Turn", RpcTarget.All);
        Turn();
    }

    //[PunRPC]
    public void Turn()
    {
        //Debug.Log("Turn()");
        Debug.Log("1 -- time: " + time + " < numPlayers: " + PhotonNetwork.PlayerList.Length);
        if (time < PhotonNetwork.PlayerList.Length)
        {
            ShowRoundInfo();

            Button[] components = gameHUD.GetComponentsInChildren<Button>();

            var repairCards = FindObjectsOfType<RepairCard>();
            players = FindObjectsOfType<PlayerScript>();

            deckEvent.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
            deckRepair.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
            timeline.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
            gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
            //foreach (var component in timeCraxComponents)
            //{
            //    component.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[time]);
            //    if(component.malfunctions == 1)
            //    {
            //        Debug.Log("1 component " + randomId + " viewid: " + component.photonView.ViewID);
            //        Debug.Log("1 component " + randomId + " owner: " + component.photonView.Owner.NickName);
            //        Debug.Log("1 component " + randomId + " isMine: " + component.photonView.IsMine);
            //        Debug.Log("1 component " + randomId + " mesh: " + component.GetComponent<MeshCollider>().enabled);
            //    }
            //}

            foreach (var player in players)
            {

                Debug.Log("2 -- " + player.nickname);
                if (player.index == time)
                {
                    Debug.Log("3 -- " + player.nickname + " tem " + player.GetNumberOfRepairsCards() + " cartas");

                    player.SetYourTurn(true);

                    foreach (var repairCard in repairCards)
                    {
                        //if(repairCard.photonView.OwnerActorNr != player.photonView.OwnerActorNr)
                        if (!repairCard.photonView.IsMine)
                        {
                            repairCard.GetComponent<MeshRenderer>().enabled = false;
                        }
                        else
                        {
                            repairCard.GetComponent<MeshRenderer>().enabled = true;
                        }
                    }

                    if (player.GetNumberOfRepairsCards() == 5)
                    {
                        deckRepair.tag = "Disabled";
                    }
                    else
                    {
                        deckRepair.tag = "Selectable";
                    }

                    foreach (Button component in components)
                    {
                        component.interactable = true;
                    }

                    timeline.GetComponent<MeshCollider>().enabled = true;
                    deckEvent.GetComponent<MeshCollider>().enabled = true;
                    deckRepair.GetComponent<MeshCollider>().enabled = true;
                    deckEvent.tag = "Selectable";

                }
                else
                {
                    Debug.Log("4 -- ");
                    player.SetYourTurn(false);

                    foreach (Button component in components)
                    {
                        if (!(component.name == "QuitGame"))
                        {
                            component.interactable = false;
                        }

                    }
                    timeline.GetComponent<MeshCollider>().enabled = false;
                    deckEvent.GetComponent<MeshCollider>().enabled = false;
                    deckRepair.GetComponent<MeshCollider>().enabled = false;
                }
            }

        }
        else
        {
            Debug.Log("Caiu no else");
            time = 0;
            round++;
            Turn();
        }

    }

    void Update()
    {

    }

    //[PunRPC]
    public void ShowRoundInfo()
    {
        //Debug.Log("ShowRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        gameInfo.gameObject.SetActive(true);
        if (round == roundCompare)
        {
            roundCompare++;

            foreach (var info in infos)
            {

                if (info.gameObject.name == "TurnInfo")
                {
                    info.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Is " + PhotonNetwork.PlayerList[time].NickName + " Turn";
                }
                if (info.gameObject.name == "RoundInfo")
                {
                    info.GetComponentInChildren<TextMeshProUGUI>().text = "Starting Round " + round + " -: " + randomId;
                }
                if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }
        }
        else
        {
            foreach (var info in infos)
            {

                if (info.gameObject.name == "TurnInfo")
                {
                    info.GetComponentInChildren<TextMeshProUGUI>().text = "Is " + PhotonNetwork.PlayerList[time].NickName + " Turn";
                }
                if (info.gameObject.name == "TurnInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }

            }
        }
        Invoke("HideRoundInfo", 1.5f);
    }

    public void HideRoundInfo()
    {
        //Debug.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("DisableGameInfo", 0.5f);
    }

    public void DisableGameInfo()
    {
        //Debug.Log("DisableGameInfo()");
        gameInfo.gameObject.SetActive(false);
    }

    public void RandomComponentNumber()
    {
        Debug.Log("7 --");
        randomId = Random.Range(1, componentList.Count);
        Debug.Log("result: " + randomId);
        photonView.RPC("ComponentRandom", RpcTarget.All, randomId);
        //ComponentRandom(randomId);

    }

    public void EndTurn()
    {
        //FinishTurn();
        Debug.Log("apertou Finish");
        Debug.Log("Finish - time: " + time + " == " + (PhotonNetwork.PlayerList.Length - 1));
        if (time == PhotonNetwork.PlayerList.Length - 1)
        {
            RandomComponentNumber();
        }
        photonView.RPC("FinishTurn", RpcTarget.All);
    }

    public void SetUpComponents()
    {
        foreach (var player in players)
        {
            Debug.Log("-- Player --: " + player.nickname);
            foreach (var component in timeCraxComponents)
            {
                if (component.malfunctions == 1)
                {
                    Debug.Log("component " + randomId + "mesh: " + component.GetComponent<MeshCollider>().enabled);
                    //if (component.photonView.IsMine)
                    if (player.GetYourTurn())
                    {
                        Debug.Log("Player: " + player.nickname + " - comp: " + component.name + " ativado");
                        component.GetComponent<MeshCollider>().enabled = true;
                    }
                    else
                    {
                        Debug.Log("Player: " + player.nickname + " - comp: " + component.name + " desativado");
                        component.GetComponent<MeshCollider>().enabled = false;
                    }
                    Debug.Log("component " + randomId + "mesh depois: " + component.GetComponent<MeshCollider>().enabled);
                }
            }
        }

    }

    [PunRPC]
    public void FinishTurn()
    {
        time++;
        //photonView.RPC("Turn", RpcTarget.All);
        Turn();
        Debug.Log("-- SetUpComponents --: ");
        SetUpComponents();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    [PunRPC]
    public void ComponentRandom(int id)
    {
        randomId = id;
        Debug.Log("Random --");

        foreach (var component in timeCraxComponents)
        {
            if (component.componentId == randomId)
            {
                component.AddMalfunction();
            }

        }
    }
}
