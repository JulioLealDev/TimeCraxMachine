using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class RepairCard : MonoBehaviourPunCallbacks
{
    private PlayerScript[] players;
    public int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        DrawRepairCard();

    }
    public void DrawRepairCard()
    {
        gameObject.GetComponent<Animator>().enabled = true;
        gameObject.GetComponent<Animator>().SetBool("drawingRepairCard", true);

    }

    public void CheckingPlayer()
    {
        //var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<PhotonView>().OwnerActorNr == photonView.OwnerActorNr)
            {
                player.DrawRepairCard();
                ShowRepairCardOnHand(player.GetNumberOfRepairsCards());
            }
        }

        // Reativar o botão FinishTurn após a carta ser adicionada à mão
        ActivateEndButton();
    }

    public void ShowRepairCardOnHand(int numberOfRepairCards)
    {
        gameObject.GetComponent<Animator>().enabled = false;
        gameObject.SetActive(false);
        ShowRepairCards(numberOfRepairCards);
    }

    public void ShowRepairCards(int numberOfRepairCards)
    {
        DebugHelper.Log("number of cards: " + numberOfRepairCards);

        switch (numberOfRepairCards)
        {
            case 1:
                DebugHelper.Log("trocando a posi��o");
                gameObject.transform.SetPositionAndRotation(new Vector3(0f, 0.648899972f, 0.638700008f), new Quaternion(0.906307876f, 0, 0, -0.42261827f));
                index = 0;
                break;
            case 2:
                DebugHelper.Log("trocando a posi��o");
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0196000002f, 0.647700012f, 0.635900021f), new Quaternion(-0.893287599f, 0.0578520186f, -0.131124616f, 0.426024318f));
                index = 1;
                break;
            case 3:
                DebugHelper.Log("trocando a posi��o");
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0238000005f, 0.648599982f, 0.644800007f), new Quaternion(-0.9102512f, -0.0436024554f, 0.0974093974f, 0.400066316f));
                index = 2;
                break;
            case 4:
                DebugHelper.Log("trocando a posi��o");
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0368999988f, 0.642799973f, 0.637899995f), new Quaternion(-0.872396052f, 0.0844448283f, -0.222514987f, 0.426944137f));
                index = 3;
                break;
            case 5:
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0425999984f, 0.648100019f, 0.655099988f), new Quaternion(-0.893432021f, -0.0762165561f, 0.201961488f, 0.393931329f));
                index = 4;
                break;
            default:
                DebugHelper.Log("N�o possui cartas");
                break;
        }

        gameObject.SetActive(true);
    }
    public void DestroyRepairCards()
    {
        Destroy(gameObject);   
    }

    public void DeactivateMesh()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.SetActive(false);

        PlayerScript owner = null;

        foreach(var player in players)
        {
            DebugHelper.Log("player owner numb: "+ player.photonView.OwnerActorNr+ " - card owner numb: "+ gameObject.GetPhotonView().OwnerActorNr);
            if(player.photonView.OwnerActorNr == gameObject.GetPhotonView().OwnerActorNr)
            {
                owner = player;
            }
        }

        ShowRepairCards(owner.GetNumberOfRepairsCards());
    }

    /// <summary>
    /// Chamado via Animation Event após a animação de compra terminar.
    /// Reativa o botão FinishTurn para o jogador no turno.
    /// </summary>
    public void ActivateEndButton()
    {
        DebugHelper.Log("[RepairCard] ActivateEndButton");
        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager.ActivateEnd();
    }
}
