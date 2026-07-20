using UnityEngine;
using Photon.Pun;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Managers;

public class BonusCard : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public int index = 0;

    [Header("Tipo da Carta")]
    [SerializeField] private BonusCardType cardType;

    [Header("Referências Visuais")]
    [SerializeField] private MeshRenderer cardRenderer;
    [SerializeField] private TextMeshPro cardText;

    // Cache de componentes
    private Animator cachedAnimator;

    // Estado da carta
    private bool isInCenter = false;
    private Vector3 savedHandPosition;
    private Quaternion savedHandRotation;
    private Vector3 savedHandScale;

    // Posição central para ativação — zoom-out
    private readonly Vector3    centerPosition = new Vector3(0.1129f, 0.8044f, 0.5021f);
    private readonly Quaternion centerRotation = new Quaternion(-0.9202125f, 0f, 0f, 0.3914192f);

    // Posição central para ativação — zoom-in (igual para todos os índices)
    private readonly Vector3    centerPositionZoomIn = new Vector3(0.1129f, 0.470197f, -0.023674f);
    private readonly Quaternion centerRotationZoomIn = new Quaternion(-0.8388f, 0f, 0f, 0.5445f);

    private const float centerScaleMultiplier = 1.2f; // +20%

    // Propriedades
    public BonusCardType CardType => cardType;
    public bool IsInCenter => isInCenter;

    void Start()
    {
        // Auto-referências se não definidas no Inspector
        if (cardRenderer == null)
            cardRenderer = GetComponent<MeshRenderer>();
        if (cardText == null)
            cardText = GetComponentInChildren<TextMeshPro>();
        cachedAnimator = GetComponent<Animator>();

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
        Texture2D texture = Resources.Load<Texture2D>($"BonusCardImages/{imageName}Image");

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
            BonusCardType.RepairComponent     => "Repair Componente",
            BonusCardType.BonusTime           => "Bonus Time",
            BonusCardType.SecondChanceSlot    => "Second Chance",
            BonusCardType.CoolThermometer     => "Cool Thermometer",
            BonusCardType.KillChallengeOption => "Kill Challenge Option",
            BonusCardType.SkipChallenge       => "Skip Challenge",
            _                                 => string.Empty,
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
        if (!GameStateManager.IsAnyOf(GamePhase.IM_Turn, GamePhase.IM_ChoosingSlot)) return;

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

        // Salvar posição, rotação e scale atuais (world)
        savedHandPosition = transform.position;
        savedHandRotation = transform.rotation;
        savedHandScale = transform.localScale;

        // Mover para centro com scale aumentado (world) — posição depende do estado de zoom
        bool zoomed = CameraController.IsZoomed || CameraController.IsAnimating;
        Vector3    targetPos = zoomed ? centerPositionZoomIn  : centerPosition;
        Quaternion targetRot = zoomed ? centerRotationZoomIn : centerRotation;
        transform.SetPositionAndRotation(targetPos, targetRot);
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
        var allPlayers = PlayerManager.Instance?.Players;
        if (allPlayers != null)
        {
            foreach (var player in allPlayers)
            {
                if (player != null && player.photonView.OwnerActorNr == photonView.OwnerActorNr)
                {
                    player.RemoveBonusCard();
                    break;
                }
            }
        }

        // Reposicionar cartas com index maior imediatamente (desloca cada uma um slot para baixo)
        if (photonView.IsMine)
        {
            int consumedIndex = index;
            var allCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
            foreach (var card in allCards)
            {
                if (card != null && card != this &&
                    card.photonView.OwnerActorNr == photonView.OwnerActorNr &&
                    card.index > consumedIndex)
                {
                    if (CameraController.IsZoomed || CameraController.IsAnimating)
                        card.index -= 1;
                    else
                        card.ShowBonusCards(card.index);
                }
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void DrawBonusCard()
    {
        cachedAnimator.enabled = true;
        cachedAnimator.SetBool("drawingBonusCard", true);
    }

    public void ChangeGameStateAfterDrawing()
    {
        GameStateManager.TransitionTo(GamePhase.IM_Turn);
    }

    public void CheckingPlayer()
    {
        var players = PlayerManager.Instance?.Players;

        if (players == null || players.Length == 0)
        {
            ActivateEndButton();
            return;
        }

        foreach (var player in players)
        {
            if (player != null && player.photonView.OwnerActorNr == photonView.OwnerActorNr)
            {
                player.DrawBonusCard();
                ShowBonusCardOnHand(player.GetNumberOfBonusCards());
            }
        }

        // Reativar o botão FinishTurn após a carta ser adicionada à mão
        GameStateManager.TransitionTo(GamePhase.IM_Turn);
        ActivateEndButton();
    }

    public void ShowBonusCardOnHand(int numberOfBonusCards)
    {
        cachedAnimator.enabled = false;
        gameObject.SetActive(false);
        ShowBonusCards(numberOfBonusCards);
    }

    public void ShowBonusCards(int numberOfBonusCards)
    {

        switch (numberOfBonusCards)
        {
            case 1:
                transform.SetPositionAndRotation(new Vector3(0f, 0.648899972f, 0.638700008f), new Quaternion(0.906307876f, 0, 0, -0.42261827f));
                index = 0;
                break;
            case 2:
                transform.SetPositionAndRotation(new Vector3(0.0196000002f, 0.647700012f, 0.635900021f), new Quaternion(-0.893287599f, 0.0578520186f, -0.131124616f, 0.426024318f));
                index = 1;
                break;
            case 3:
                transform.SetPositionAndRotation(new Vector3(-0.0238000005f, 0.648599982f, 0.644800007f), new Quaternion(-0.9102512f, -0.0436024554f, 0.0974093974f, 0.400066316f));
                index = 2;
                break;
            case 4:
                transform.SetPositionAndRotation(new Vector3(0.0368999988f, 0.642799973f, 0.637899995f), new Quaternion(-0.8569837808609009f, 0.10065678507089615f, -0.3076842427253723f, 0.4009706974029541f));
                index = 3;
                break;
            case 5:
                transform.SetPositionAndRotation(new Vector3(-0.0425999984f, 0.648100019f, 0.655099988f), new Quaternion(-0.893432021f, -0.0762165561f, 0.201961488f, 0.393931329f));
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
        cardRenderer.enabled = false;
        gameObject.SetActive(false);

        var players = PlayerManager.Instance?.Players;

        if (players == null || players.Length == 0)
        {
            return;
        }

        PlayerScript owner = null;

        foreach(var player in players)
        {
            if (player == null) continue;

            if(player.photonView.OwnerActorNr == photonView.OwnerActorNr)
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

    #region IPunInstantiateMagicCallback

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var data = info.photonView.InstantiationData;
        if (data == null || data.Length == 0) return;

        // Garantir referências antes de aplicar visuais (Start ainda não rodou)
        if (cardRenderer == null) cardRenderer = GetComponent<MeshRenderer>();
        if (cardText == null) cardText = GetComponentInChildren<TextMeshPro>();
        if (cachedAnimator == null) cachedAnimator = GetComponent<Animator>();

        SetCardType((BonusCardType)(int)data[0]);
    }

    #endregion
}
