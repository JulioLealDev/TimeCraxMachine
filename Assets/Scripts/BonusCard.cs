using UnityEngine;
using Photon.Pun;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Managers;

public class BonusCard : MonoBehaviourPunCallbacks
{
    private PlayerScript[] players;
    public int index = 0;

    [Header("Tipo da Carta")]
    [SerializeField] private BonusCardType cardType = BonusCardType.Repair;

    [Header("Referências Visuais")]
    [SerializeField] private MeshRenderer cardRenderer;
    [SerializeField] private TextMeshPro cardText;

    // Estado da carta
    private bool isInCenter = false;
    private Vector3 savedHandPosition;
    private Quaternion savedHandRotation;
    private Vector3 savedHandScale;

    // Posição central para ativação
    private readonly Vector3 centerPosition = new Vector3(0.1079f, 0.7694f, 0.5021f);
    private readonly Quaternion centerRotation = new Quaternion(-0.9202125f, 0f, 0f, 0.3914192f);
    private const float centerScaleMultiplier = 1.2f; // +20%

    // Propriedades
    public BonusCardType CardType => cardType;
    public bool IsInCenter => isInCenter;

    void Start()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        // Auto-referências se não definidas no Inspector
        if (cardRenderer == null)
            cardRenderer = GetComponent<MeshRenderer>();
        if (cardText == null)
            cardText = GetComponentInChildren<TextMeshPro>();

        DrawBonusCard();
    }

    /// <summary>
    /// Define o tipo da carta (chamado pelo DeckBonus após instanciar)
    /// </summary>
    public void SetCardType(BonusCardType type)
    {
        cardType = type;
        ApplyCardVisuals();
    }

    /// <summary>
    /// Aplica a imagem e texto da carta baseado no tipo
    /// </summary>
    private void ApplyCardVisuals()
    {
        // Carregar textura da pasta Resources/BonusCardImages
        string imageName = cardType.ToString();
        Texture2D texture = Resources.Load<Texture2D>($"BonusCardImages/{imageName}");

        if (texture != null && cardRenderer != null)
        {
            // Aplicar textura ao material (usa _ImageTex para shader composto)
            Material mat = cardRenderer.material;
            if (mat.HasProperty("_ImageTex"))
            {
                mat.SetTexture("_ImageTex", texture);
            }
            else if (mat.HasProperty("_MainTex"))
            {
                // Fallback para shader padrão
                mat.SetTexture("_MainTex", texture);
            }
        }
        else
        {
        }

        // Definir texto da carta
        if (cardText != null)
        {
            cardText.text = GetCardDisplayName(cardType);
        }
    }

    /// <summary>
    /// Retorna o nome de exibição para cada tipo de carta
    /// </summary>
    private string GetCardDisplayName(BonusCardType type)
    {
        return type switch
        {
            BonusCardType.Repair => "Repair Card",
            BonusCardType.Time => "Time Card",
            BonusCardType.SecondChance => "Second Chance Card",
            BonusCardType.Thermometer => "Thermometer Card",
            _ => "Bonus Card"
        };
    }

    /// <summary>
    /// Handler de clique na carta
    /// </summary>
    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (!photonView.IsMine) return;
        if (CameraController.IsAnimating) return;

        // Se carta de reparo, não faz nada (auto-usa no componente)
        if (cardType == BonusCardType.Repair) return;

        if (!isInCenter)
        {
            // Mover para o centro
            MoveToCenter();
        }
    }

    /// <summary>
    /// Move a carta para o centro da tela e abre painel de ativação
    /// </summary>
    public void MoveToCenter()
    {
        if (isInCenter) return;

        // Verificar se pode ativar
        if (BonusCardManager.Instance != null && !BonusCardManager.Instance.CanActivateCard(cardType))
        {
            return;
        }

        // Salvar posição, rotação e scale atuais
        savedHandPosition = transform.position;
        savedHandRotation = transform.rotation;
        savedHandScale = transform.localScale;

        // Mover para centro com scale aumentado
        transform.SetPositionAndRotation(centerPosition, centerRotation);
        transform.localScale = savedHandScale * centerScaleMultiplier;
        isInCenter = true;

        // Abrir painel de ativação
        if (BonusCardManager.Instance != null)
        {
            BonusCardManager.Instance.ShowActivationPanel(this);
        }

    }

    /// <summary>
    /// Retorna a carta para a mão
    /// </summary>
    public void ReturnToHand()
    {
        if (!isInCenter) return;

        transform.SetPositionAndRotation(savedHandPosition, savedHandRotation);
        transform.localScale = savedHandScale;
        isInCenter = false;

    }

    /// <summary>
    /// Consome a carta (após ativação)
    /// </summary>
    public void ConsumeCard()
    {

        // Decrementar contador do jogador
        if (players == null || players.Length == 0)
        {
            players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        }

        foreach (var player in players)
        {
            if (player != null && player.photonView.OwnerActorNr == photonView.OwnerActorNr)
            {
                player.RemoveBonusCard();
                break;
            }
        }

        // Destruir a carta
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
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

        switch (numberOfBonusCards)
        {
            case 1:
                gameObject.transform.SetPositionAndRotation(new Vector3(0f, 0.648899972f, 0.638700008f), new Quaternion(0.906307876f, 0, 0, -0.42261827f));
                index = 0;
                break;
            case 2:
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0196000002f, 0.647700012f, 0.635900021f), new Quaternion(-0.893287599f, 0.0578520186f, -0.131124616f, 0.426024318f));
                index = 1;
                break;
            case 3:
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0238000005f, 0.648599982f, 0.644800007f), new Quaternion(-0.9102512f, -0.0436024554f, 0.0974093974f, 0.400066316f));
                index = 2;
                break;
            case 4:
                gameObject.transform.SetPositionAndRotation(new Vector3(0.0368999988f, 0.642799973f, 0.637899995f), new Quaternion(-0.872396052f, 0.0844448283f, -0.222514987f, 0.426944137f));
                index = 3;
                break;
            case 5:
                gameObject.transform.SetPositionAndRotation(new Vector3(-0.0425999984f, 0.648100019f, 0.655099988f), new Quaternion(-0.893432021f, -0.0762165561f, 0.201961488f, 0.393931329f));
                index = 4;
                break;
            default:
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
            return;
        }

        PlayerScript owner = null;

        foreach(var player in players)
        {
            if (player == null) continue;

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
        }
    }

    /// <summary>
    /// Chamado via Animation Event após a animação de compra terminar.
    /// Reativa o botão FinishTurn para o jogador no turno.
    /// </summary>
    public void ActivateEndButton()
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ActivateEnd();
        }
        else
        {
        }
    }

    #region RPCs

    /// <summary>
    /// RPC para sincronizar o tipo da carta em todos os clientes
    /// </summary>
    [PunRPC]
    public void RPC_SetCardType(int type)
    {
        cardType = (BonusCardType)type;

        // Garantir referências antes de aplicar visuais
        if (cardRenderer == null)
            cardRenderer = GetComponent<MeshRenderer>();
        if (cardText == null)
            cardText = GetComponentInChildren<TextMeshPro>();

        ApplyCardVisuals();
    }

    #endregion
}
