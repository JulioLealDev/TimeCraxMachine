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
    private readonly Vector3    centerPosition = new Vector3(0.0929f, 0.7984f, 0.5021f);
    private readonly Quaternion centerRotation = new Quaternion(-0.95f, 0f, 0f, 0.3914192f);

    // Posição central para ativação — zoom-in (igual para todos os índices)
    private readonly Vector3    centerPositionZoomIn = new Vector3(0.0929f, 0.437197f, -0.016674f);
    private readonly Quaternion centerRotationZoomIn = new Quaternion(-0.8388f, 0f, 0f, 0.5445f);

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
        if (!GameStateManager.IsAnyOf(GamePhase.IM_Turn, GamePhase.IM_ChoosingSlot, GamePhase.IM_MapChallenge, GamePhase.IM_PersonsChallenge)) return;

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

        // Mover para centro (world) — posição depende do estado de zoom
        bool zoomed = CameraController.IsZoomed || CameraController.IsAnimating;
        Vector3    targetPos = zoomed ? centerPositionZoomIn  : centerPosition;
        Quaternion targetRot = zoomed ? centerRotationZoomIn : centerRotation;
        transform.SetPositionAndRotation(targetPos, targetRot);
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

        if (photonView.IsMine)
        {
            // RPC enviado antes do Destroy: as cartas ainda existem quando o
            // reposicionamento é processado em todos os clientes.
            photonView.RPC("RPC_ShiftCardsAfterConsume", RpcTarget.All, photonView.OwnerActorNr, index);

            var local = PlayerManager.Instance?.GetLocalPlayer();
            if (local != null)
            {
                var gm = FindFirstObjectByType<GameManager>();
                if (gm != null && PhotonNetwork.InRoom)
                    gm.photonView.RPC("RPC_TrackBonusCardUsed", RpcTarget.All, local.actorNumber, local.nickname);
                else
                    MatchStats.AddBonusCardUsed(local.actorNumber, local.nickname);
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    public void RPC_ShiftCardsAfterConsume(int ownerActorNumber, int consumedIndex)
    {
        Debug.Log($"[BonusCard] RPC_ShiftCardsAfterConsume — owner={ownerActorNumber}, consumed={consumedIndex}, IsZoomed={CameraController.IsZoomed}, IsAnimating={CameraController.IsAnimating}");
        var allCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
        foreach (var card in allCards)
        {
            if (card == null) continue;
            if (card.photonView.OwnerActorNr != ownerActorNumber) continue;
            if (card.index <= consumedIndex) continue;

            Debug.Log($"[BonusCard] RPC_ShiftCardsAfterConsume — shifting card index={card.index} → ShowBonusCards({card.index})");
            card.ShowBonusCards(card.index);
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
        bool zoomed = CameraController.IsZoomed || CameraController.IsAnimating;
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        switch (numberOfBonusCards)
        {
            case 1:
                pos = zoomed ? new Vector3(0f, 0.27767640352249148f, 0.049448609352111819f)
                             : new Vector3(0f, 0.648899972f, 0.638700008f);
                rot = zoomed ? new Quaternion(0.8142168521881104f, 0f, 0f, -0.5805608630180359f)
                             : new Quaternion(0.906307876f, 0f, 0f, -0.42261827f);
                index = 0;
                break;
            case 2:
                pos = zoomed ? new Vector3(0.0196000002f, 0.277558506f, 0.0464046597f)
                             : new Vector3(0.0196000002f, 0.647700012f, 0.635900021f);
                rot = zoomed ? new Quaternion(-0.800794184f, 0.0807581916f, -0.118400335f, 0.581539512f)
                             : new Quaternion(-0.893287599f, 0.0578520186f, -0.131124616f, 0.426024318f);
                index = 1;
                break;
            case 3:
                pos = zoomed ? new Vector3(-0.0238000005f, 0.275212228f, 0.0550368428f)
                             : new Vector3(-0.0238000005f, 0.648599982f, 0.644800007f);
                rot = zoomed ? new Quaternion(-0.822200298f, -0.0606084503f, 0.0878429338f, 0.559103787f)
                             : new Quaternion(-0.9102512f, -0.0436024554f, 0.0974093974f, 0.400066316f);
                index = 2;
                break;
            case 4:
                pos = zoomed ? new Vector3(0.0368999988f, 0.27226722240448f, 0.04651761054992676f)
                             : new Vector3(0.0368999988f, 0.642799973f, 0.637899995f);
                rot = zoomed ? new Quaternion(-0.7696585059165955f, 0.1549926996231079f, -0.28421589732170107f, 0.5502949953079224f)
                             : new Quaternion(-0.8569837808609009f, 0.10065678507089615f, -0.3076842427253723f, 0.4009706974029541f);
                index = 3;
                break;
            case 5:
                pos = zoomed ? new Vector3(-0.0425999984f, 0.271057576f, 0.0644749999f)
                             : new Vector3(-0.0425999984f, 0.648100019f, 0.655099988f);
                rot = zoomed ? new Quaternion(-0.806779146f, -0.111712635f, 0.184709758f, 0.550009191f)
                             : new Quaternion(-0.893432021f, -0.0762165561f, 0.201961488f, 0.393931329f);
                index = 4;
                break;
            default:
                Debug.LogWarning($"[BonusCard] ShowBonusCards — valor inesperado: n={numberOfBonusCards}");
                gameObject.SetActive(true);
                return;
        }

        Debug.Log($"[BonusCard] ShowBonusCards — n={numberOfBonusCards}, zoomed={zoomed} (IsZoomed={CameraController.IsZoomed}, IsAnimating={CameraController.IsAnimating}), pos={pos}, index={index}, owner={photonView.OwnerActorNr}");
        transform.SetPositionAndRotation(pos, rot);
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

        // Posicionar dentro do HUD em TODOS os clientes para que a animação de compra
        // apareça no lugar correto independentemente de quem é o dono da carta.
        var hud = GameObject.Find("Camera/HUD");
        if (hud != null)
        {
            transform.SetParent(hud.transform, false);
            transform.localPosition = new Vector3(36.93f, -20.59f, -40.29f);
            transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
        }
    }

    #endregion
}
