using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

public class BonusCard : MonoBehaviourPunCallbacks
{
    private PlayerScript[] players;
    public int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        DrawBonusCard();

    }
    public void DrawBonusCard()
    {
        gameObject.GetComponent<Animator>().enabled = true;
        gameObject.GetComponent<Animator>().SetBool("drawingBonusCard", true);

    }

    public void CheckingPlayer()
    {
        // Garantir que players está populado
        if (players == null || players.Length == 0)
        {
            players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        }

        if (players == null || players.Length == 0)
        {
            DebugHelper.Log("[BonusCard] CheckingPlayer: Nenhum player encontrado");
            ActivateEndButton();
            return;
        }

        foreach (var player in players)
        {
            if (player != null && player.GetComponent<PhotonView>().OwnerActorNr == photonView.OwnerActorNr)
            {
                player.DrawBonusCard();
                ShowBonusCardOnHand(player.GetNumberOfBonusCards());
            }
        }

        // Reativar o botão FinishTurn após a carta ser adicionada à mão
        ActivateEndButton();
    }

    public void ShowBonusCardOnHand(int numberOfBonusCards)
    {
        gameObject.GetComponent<Animator>().enabled = false;
        gameObject.SetActive(false);
        ShowBonusCards(numberOfBonusCards);
    }

    public void ShowBonusCards(int numberOfBonusCards)
    {
        DebugHelper.Log("number of cards: " + numberOfBonusCards);

        switch (numberOfBonusCards)
        {
            case 1:
                DebugHelper.Log("trocando a posição");
                gameObject.transform.SetPositionAndRotation(new Vector3(0f, 0.648899972f, 0.638700008f), new Quaternion(0.906307876f, 0, 0, -0.42261827f));
                index = 0;
                break;
            case 2:
                DebugHelper.Log("trocando a posição");
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0196000002f, 0.647700012f, 0.635900021f), new Quaternion(-0.893287599f, 0.0578520186f, -0.131124616f, 0.426024318f));
                index = 1;
                break;
            case 3:
                DebugHelper.Log("trocando a posição");
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0238000005f, 0.648599982f, 0.644800007f), new Quaternion(-0.9102512f, -0.0436024554f, 0.0974093974f, 0.400066316f));
                index = 2;
                break;
            case 4:
                DebugHelper.Log("trocando a posição");
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0368999988f, 0.642799973f, 0.637899995f), new Quaternion(-0.872396052f, 0.0844448283f, -0.222514987f, 0.426944137f));
                index = 3;
                break;
            case 5:
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0425999984f, 0.648100019f, 0.655099988f), new Quaternion(-0.893432021f, -0.0762165561f, 0.201961488f, 0.393931329f));
                index = 4;
                break;
            default:
                DebugHelper.Log("Não possui cartas");
                break;
        }

        gameObject.SetActive(true);
    }
    public void DestroyBonusCards()
    {
        Destroy(gameObject);
    }

    public void DeactivateMesh()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.SetActive(false);

        // Garantir que players está populado
        if (players == null || players.Length == 0)
        {
            players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        }

        if (players == null || players.Length == 0)
        {
            DebugHelper.Log("[BonusCard] DeactivateMesh: Nenhum player encontrado");
            return;
        }

        PlayerScript owner = null;

        foreach(var player in players)
        {
            if (player == null) continue;

            DebugHelper.Log("player owner numb: "+ player.photonView.OwnerActorNr+ " - card owner numb: "+ gameObject.GetPhotonView().OwnerActorNr);
            if(player.photonView.OwnerActorNr == gameObject.GetPhotonView().OwnerActorNr)
            {
                owner = player;
            }
        }

        if (owner != null)
        {
            ShowBonusCards(owner.GetNumberOfBonusCards());
        }
        else
        {
            DebugHelper.Log("[BonusCard] DeactivateMesh: Owner não encontrado");
        }
    }

    /// <summary>
    /// Chamado via Animation Event após a animação de compra terminar.
    /// Reativa o botão FinishTurn para o jogador no turno.
    /// </summary>
    public void ActivateEndButton()
    {
        DebugHelper.Log("[BonusCard] ActivateEndButton");
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ActivateEnd();
        }
        else
        {
            DebugHelper.Log("[BonusCard] ActivateEndButton: GameManager não encontrado");
        }
    }
}
