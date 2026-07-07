using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using TimeCrax.Core;
using TimeCrax.Themes;
using TimeCrax.Managers;
// using TimeCrax.Quiz; // desabilitado

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Referências de Cena")]
    [SerializeField] private GameObject gameInfo;
    [SerializeField] private GameObject enviroment;
    [SerializeField] private DeckEvent deckEvent;
    [SerializeField] private GameObject deckBonus;
    [SerializeField] private GameObject timeline;
    [SerializeField] private CameraController gameCamera;
    [SerializeField] private GameObject inputName;
    [SerializeField] private GameObject suitTop;
    [SerializeField] private GameObject hud;
    [SerializeField] private FinishTurn endButton;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameOver gameOver;
    [SerializeField] private Victory victory;
    [SerializeField] private GameObject map;
    [SerializeField] private GameObject personsFrame;
    [SerializeField] private Animator newTimelineAnimator;
    [SerializeField] private TMP_Text personName01;
    [SerializeField] private TMP_Text personName02;
    [SerializeField] private TMP_Text personName03;
    [SerializeField] private TMP_Text personText01;
    [SerializeField] private TMP_Text personText02;
    [SerializeField] private TMP_Text personText03;

    public static ThemeCard CurrentPersonsThemeCard { get; private set; }

    [Header("Áudio")]
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private BackgroundMusic backgroundMusic;

    [Header("Materiais")]
    [SerializeField] private Material plateNameMaterial;
    [SerializeField] private Material plateNameMaterial2;

    [Header("UI")]
    [SerializeField] private GameObject playerLeftBackground;
    [SerializeField] private Animator rightCompartmentAnimator;
    [SerializeField] private Animator leftCompartmentAnimator;

    [Header("Sistemas")]
    [SerializeField] private RandomMaterial randomMaterial;
    [SerializeField] private TurnTimer turnTimer;

    // Campos públicos necessários para RPCs e acesso externo
    public int randomId;

    // Propriedades públicas para acesso externo
    public GameObject Hud => hud;

    // Flag para rastrear se está em transição de turno (cursor deve permanecer desabilitado)
    public static bool IsInTurnTransition { get; set; } = false;

    // Campos privados
    private MachineComponent[] timeCraxComponents;
    private PlayerScript[] players;
    private int[] playersList;
    private int initialPlayersNumber;
    private int round;
    private int roundCompare;
    private int time;
    private bool gameIsOn = false;
    private List<int> componentList = new List<int>();
    private List<Transform> componentsWithAnimator = new List<Transform>();
    private PlayerScript[] orderedPlayers;

    // Cache para evitar chamadas repetitivas a FindObjectsByType
    private GiveCards[] cachedPlateNames;
    private BonusCard[] cachedBonusCards;
    private bool needsCacheRefresh = true;

    // Cache de GetComponent para componentes frequentemente acessados
    private MeshCollider cachedDeckEventMeshCollider;
    private MeshCollider cachedDeckBonusMeshCollider;
    private MeshCollider cachedTimelineMeshCollider;
    private MeshCollider cachedEndButtonMeshCollider;
    private MeshCollider cachedQuitButtonMeshCollider;
    private PhotonView cachedDeckEventPhotonView;
    private PhotonView cachedDeckBonusPhotonView;
    private PhotonView cachedTimelinePhotonView;
    private PhotonView cachedEndButtonPhotonView;
    private Animator cachedGameCameraAnimator;
    private Animator cachedSuitTopAnimator;

    /// <summary>
    /// Atualiza o cache de referências. Chamar quando jogadores entram/saem ou cartas são criadas/destruídas.
    /// </summary>
    public void RefreshCache()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        cachedPlateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);
        cachedBonusCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
        needsCacheRefresh = false;
        DebugHelper.Log("[GameManager] Cache atualizado");
    }

    /// <summary>
    /// Cache de GetComponent para componentes acessados frequentemente
    /// </summary>
    private void CacheComponents()
    {
        if (deckEvent != null)
        {
            cachedDeckEventMeshCollider = deckEvent.GetComponent<MeshCollider>();
            cachedDeckEventPhotonView = deckEvent.GetComponent<PhotonView>();
        }

        if (deckBonus != null)
        {
            cachedDeckBonusMeshCollider = deckBonus.GetComponent<MeshCollider>();
            cachedDeckBonusPhotonView = deckBonus.GetComponent<PhotonView>();
        }

        if (timeline != null)
        {
            cachedTimelineMeshCollider = timeline.GetComponent<MeshCollider>();
            cachedTimelinePhotonView = timeline.GetComponent<PhotonView>();
        }

        if (endButton != null)
        {
            cachedEndButtonMeshCollider = endButton.GetComponent<MeshCollider>();
            cachedEndButtonPhotonView = endButton.GetComponent<PhotonView>();
        }

        if (quitButton != null)
        {
            cachedQuitButtonMeshCollider = quitButton.GetComponent<MeshCollider>();
        }

        if (gameCamera != null)
        {
            cachedGameCameraAnimator = gameCamera.gameObject.GetComponent<Animator>();
        }

        if (suitTop != null)
        {
            cachedSuitTopAnimator = suitTop.GetComponent<Animator>();
        }

        DebugHelper.Log("[GameManager] Componentes cacheados");
    }

    #region Callbacks para TurnManager

    /// <summary>
    /// Callback chamado pelo TurnManager quando o turno é sincronizado
    /// </summary>
    public void OnTurnSynced()
    {
        Turn();
    }

    /// <summary>
    /// Callback chamado pelo TurnManager quando a info do round é escondida
    /// </summary>
    public void OnRoundInfoHidden()
    {
        StartTurn();
    }

    #endregion

    #region Callbacks para QuizManager

    /// <summary>
    /// Callback chamado quando o quiz é completado.
    /// Se acertou: aguarda camera zoomOut e então abre compartimento esquerdo e habilita deckBonus.
    /// </summary>
    private void OnQuizCompleted(bool correct)
    {
        DebugHelper.Log($"[GameManager] OnQuizCompleted: correct={correct}");

        if (correct)
        {
            // Aguardar camera zoomOut antes de abrir compartimento
            // 3.3s (animação da carta) + 1.5s (zoom out) = 4.8s
            this.DelayedCall(3.5f, OpenLeftCompartmentAfterZoomOut);
        }
    }

    /// <summary>
    /// Abre o compartimento esquerdo após o zoomOut da câmera.
    /// </summary>
    private void OpenLeftCompartmentAfterZoomOut()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_OpenLeftCompartment", RpcTarget.All);
        }
    }

    /// <summary>
    /// RPC para abrir o compartimento esquerdo e habilitar deckBonus.
    /// Sincroniza a abertura em todos os clientes.
    /// </summary>
    [PunRPC]
    public void RPC_OpenLeftCompartment()
    {
        DebugHelper.Log("[GameManager] RPC_OpenLeftCompartment");

        // Abrir compartimento esquerdo
        if (leftCompartmentAnimator != null)
        {
            leftCompartmentAnimator.SetBool("open", true);
        }

        // Habilitar deckBonus apenas para o jogador do turno
        foreach (var player in players)
        {
            if (player != null && player.photonView.IsMine && player.GetYourTurn())
            {
                if (cachedDeckBonusMeshCollider != null)
                {
                    cachedDeckBonusMeshCollider.enabled = true;
                }
                if (deckBonus != null)
                {
                    // Verificar se jogador já tem 5 cartas
                    if (player.GetNumberOfBonusCards() < 5)
                    {
                        deckBonus.tag = "Selectable";
                    }
                    else
                    {
                        deckBonus.tag = "Disabled";
                    }
                }
                break;
            }
        }
    }

    /// <summary>
    /// Fecha o compartimento esquerdo e desabilita deckBonus.
    /// Chamado após o jogador comprar uma carta de reparo.
    /// </summary>
    public void CloseLeftCompartment()
    {
        DebugHelper.Log("[GameManager] CloseLeftCompartment");

        // Fechar compartimento esquerdo
        if (leftCompartmentAnimator != null)
        {
            leftCompartmentAnimator.SetBool("open", false);
        }

        // Desabilitar deckBonus
        if (cachedDeckBonusMeshCollider != null)
        {
            cachedDeckBonusMeshCollider.enabled = false;
        }
        if (deckBonus != null)
        {
            deckBonus.tag = "Disabled";
        }
    }

    /// <summary>
    /// RPC para fechar o compartimento esquerdo e desabilitar deckBonus.
    /// Chamado quando o turno termina ou quando o jogador compra uma carta.
    /// </summary>
    [PunRPC]
    public void RPC_CloseLeftCompartment()
    {
        DebugHelper.Log("[GameManager] RPC_CloseLeftCompartment");

        // Fechar compartimento esquerdo
        if (leftCompartmentAnimator != null)
        {
            leftCompartmentAnimator.SetBool("open", false);
        }

        // Desabilitar deckBonus
        if (cachedDeckBonusMeshCollider != null)
        {
            cachedDeckBonusMeshCollider.enabled = false;
        }
        if (deckBonus != null)
        {
            deckBonus.tag = "Disabled";
        }
    }

    #endregion

    #region Ativação de PersonsFrame

    /// <summary>
    /// Ativa PersonsFrame e abre a NewTimeline.
    /// Chamado pelo MasterClient após carta de evento ser posicionada corretamente.
    /// </summary>
    public void ActivateRandomMapObject(int slotCount)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_ActivateRandomMapObject", RpcTarget.All, slotCount);
    }

    public static int CurrentPersonsSlotCount { get; private set; }

    [PunRPC]
    private void RPC_ActivateRandomMapObject(int slotCount)
    {
        DebugHelper.Log($"[GameManager] RPC_ActivateRandomMapObject slotCount={slotCount}");

        CurrentPersonsSlotCount = slotCount;

        if (personsFrame != null) personsFrame.SetActive(true);
        CurrentPersonsThemeCard = FindPersonsThemeCard(slotCount);
        ApplyPersonsText(CurrentPersonsThemeCard);

        if (newTimelineAnimator != null)
            newTimelineAnimator.SetBool("Open", true);

        this.DelayedCall(2.5f, ActivatePersonsSelectable);
    }

    private void ActivatePersonsSelectable()
    {
        foreach (var img in FindObjectsByType<PersonCardImage>(FindObjectsSortMode.None))
        {
            img.gameObject.tag = "Selectable";
            var outline = img.gameObject.GetComponent<OutlineComponent>();
            if (outline != null) { outline.SetColor(Color.white); outline.enabled = false; }
        }
        foreach (var desc in FindObjectsByType<PersonDescriptionClick>(FindObjectsSortMode.None))
        {
            desc.gameObject.tag = "Selectable";
            var outline = desc.gameObject.GetComponent<OutlineComponent>();
            if (outline != null) { outline.SetColor(Color.white); outline.enabled = false; }
        }
    }

    public void ClosePersonsNewTimeline()
    {
        if (newTimelineAnimator != null)
            newTimelineAnimator.SetBool("Open", false);
    }

    private void PersonsZoomOut()
    {
        DebugHelper.Log($"[GameManager] PersonsZoomOut chamado. gameCamera={gameCamera != null}, zoomMode={gameCamera?.IsInZoomMode()}");
        gameCamera?.DistanceTimeline();
    }

    public void HandlePersonsWrong()
    {
        // Re-enable the slot so SetUpSlots can select it again on the next turn
        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            if (slot.SlotNumber == CurrentPersonsSlotCount)
            {
                slot.gameObject.tag = "Untagged";
                break;
            }
        }

        var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in cards)
        {
            if (card.slotCount == CurrentPersonsSlotCount)
            {
                card.GetComponent<Animator>().SetBool("wrongSlot", true);
                int slotCount = CurrentPersonsSlotCount;
                this.DelayedCall(2f, () =>
                {
                    card.ResetStatusCard();
                    deckEvent.AddCardBack(slotCount);
                });
                break;
            }
        }
    }

    public void ResetPersonsFrame()
    {
        if (personName01 != null) personName01.text = string.Empty;
        if (personName02 != null) personName02.text = string.Empty;
        if (personName03 != null) personName03.text = string.Empty;
        if (personText01 != null) personText01.text = string.Empty;
        if (personText02 != null) personText02.text = string.Empty;
        if (personText03 != null) personText03.text = string.Empty;

        foreach (var img in FindObjectsByType<PersonCardImage>(FindObjectsSortMode.None))
            img.gameObject.tag = "Untagged";
        foreach (var desc in FindObjectsByType<PersonDescriptionClick>(FindObjectsSortMode.None))
            desc.gameObject.tag = "Untagged";

        if (personsFrame != null) personsFrame.SetActive(false);
        EventSlot.ResetClickProtection();

        // 0.5s após reset (3s após Open=false): zoom out e reabilita interações
        this.DelayedCall(0.5f, PersonsZoomOut);
    }

    private static ThemeCard FindPersonsThemeCard(int slotCount)
    {
        foreach (var card in FindObjectsByType<EventCard>(FindObjectsSortMode.None))
        {
            if (card.slotCount == slotCount)
                return card.GetThemeCard();
        }
        return null;
    }

    public static List<TimeCrax.Themes.PersonEntry> ShuffledPersonEntries { get; private set; }

    private void ApplyPersonsText(ThemeCard themeCard)
    {
        var entries = themeCard?.persons?.entries;
        if (entries == null) return;

        TMP_Text[] names = { personName01, personName02, personName03 };
        TMP_Text[] texts = { personText01, personText02, personText03 };

        ShuffledPersonEntries = entries
            .OrderBy(_ => UnityEngine.Random.value)
            .ToList();

        for (int i = 0; i < 3 && i < texts.Length; i++)
        {
            if (names[i] != null) names[i].text = string.Empty;
            if (texts[i] != null) texts[i].text = i < ShuffledPersonEntries.Count ? ShuffledPersonEntries[i].description : string.Empty;
        }
    }

    #endregion

    /// <summary>
    /// Retorna os plateNames do cache, atualizando se necessário.
    /// </summary>
    private GiveCards[] GetCachedPlateNames()
    {
        if (needsCacheRefresh || cachedPlateNames == null)
        {
            cachedPlateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);
        }
        return cachedPlateNames;
    }

    /// <summary>
    /// Retorna os bonusCards do cache, atualizando se necessário.
    /// </summary>
    private BonusCard[] GetCachedBonusCards()
    {
        if (needsCacheRefresh || cachedBonusCards == null)
        {
            cachedBonusCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
        }
        return cachedBonusCards;
    }

    void Update()
    {
        if (gameIsOn && PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerList.Length != initialPlayersNumber)
            {
                initialPlayersNumber = PhotonNetwork.PlayerList.Length;

                // Verificar se players está válido antes de iterar
                if (players == null || players.Length == 0)
                {
                    DebugHelper.Log("[Update] players está null ou vazio, ignorando verificação de saída");
                    return;
                }

                int plateNameIndex = -1;

                // Encontrar qual jogador saiu comparando players locais com PhotonNetwork.PlayerList
                foreach (var player in players)
                {
                    if (player == null) continue;

                    bool playerStillConnected = false;
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        if (player.nickname == PhotonNetwork.PlayerList[i].NickName)
                        {
                            playerStillConnected = true;
                            break;
                        }
                    }

                    // Se o jogador não está mais na lista do Photon, ele saiu
                    if (!playerStillConnected)
                    {
                        DebugHelper.Log("[Update] Jogador que saiu: " + player.nickname + " plateNameIndex: " + player.plateNameIndex);
                        plateNameIndex = player.plateNameIndex;
                        break;
                    }
                }

                if (plateNameIndex >= 0)
                {
                    DebugHelper.Log("numberPlayers: " + PhotonNetwork.PlayerList.Length + " --  initialNumber: " + initialPlayersNumber);
                    photonView.RPC("RemovePlayersPlatenames", RpcTarget.All, plateNameIndex);
                }
                else
                {
                    DebugHelper.Log("[Update] Não foi possível identificar qual jogador saiu");
                }
            }
        }
    }

    [PunRPC]
    public void UpdatePlayersIndex()
    {

        DebugHelper.Log("Resetando Index");

        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            DebugHelper.Log("player name: "+player.nickname);
            player.UpdateIndex();
        }
    }


    [PunRPC]
    public void RemovePlayersPlatenames(int index)
    {
        if (orderedPlayers == null) return;

        for(int i = 0; i < orderedPlayers.Length; i++)
        {
            if (orderedPlayers[i]?.plateNameIndex == index)
            {
                DebugHelper.Log("Removendo " + orderedPlayers[i].nickname);
                orderedPlayers[i] = null;
            }
        }

        index++;

        DebugHelper.Log("Removendo platename");

        var plate = GameObject.Find(GameObjectNames.GetPlateName(index));
        if (plate != null)
        {
            DebugHelper.Log("plate name: " + plate.name);
            plate.GetComponent<MeshRenderer>().enabled = false;
            plate.GetComponent<MeshCollider>().enabled = false;
        }

        var bonusSymbol = GameObject.Find(GameObjectNames.GetBonusCardSymbol(index));
        if (bonusSymbol != null)
        {
            DebugHelper.Log("bonusSymbol name: " + bonusSymbol.name);
            bonusSymbol.GetComponent<SpriteRenderer>().enabled = false;
        }

        var namePlate = GameObject.Find(GameObjectNames.GetNamePlayer(index));
        if (namePlate != null)
        {
            DebugHelper.Log("namePlate name: " + namePlate.name);
            namePlate.GetComponent<TMP_Text>().text = " ";
            namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
        }

        var numberBonusCard = GameObject.Find(GameObjectNames.GetNumberBonusCards(index));
        if (numberBonusCard != null)
        {
            DebugHelper.Log("bonusCardSymbol name: " + numberBonusCard.name);
            numberBonusCard.GetComponent<TextMeshProUGUI>().text = " ";
        }
    }

    void Start()
    {
        //DebugHelper.Log("Start()");

        //timeCraxComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);

        //gameCamera.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);

    }

    public void GetRandomEventCards(string theme)
    {
        randomMaterial.GetRandomMaterial(theme);

    }

    public void StartNewGame()
    {
        string theme = PhotonNetwork.CurrentRoom.CustomProperties["the"].ToString();

        DebugHelper.Log("-------------  THEME: " + theme);

        // Abrir compartimento direito
        if (rightCompartmentAnimator != null)
        {
            rightCompartmentAnimator.SetBool("open", true);
        }

        // Cache dos componentes no início do jogo
        CacheComponents();

        gameIsOn = true;
        //gameOver.gameIsOver = false;

        // Limpar lista de componentes com animator
        componentsWithAnimator.Clear();

        //Lista de todos os componentes com o Script Component
        timeCraxComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);

        Transform[] components = enviroment.GetComponentsInChildren<Transform>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].CompareTag("Component"))
            {
                //Lista de todos os componentes com a tag Component (Ou seja, possui Animator)
                var animator = components[i].GetComponent<Animator>();
                if (animator != null)
                {
                    DebugHelper.Log("Ativando animator do componente " + components[i].name);
                    animator.enabled = true;
                    componentsWithAnimator.Add(components[i]);
                }
            }
        }

        round = 1;
        roundCompare = 1;
        time = 0;

        //inputName.SetActive(false);
        PhotonNetwork.Instantiate("Player", new Vector3(7.224f, 1.01f, 0.83f), Quaternion.identity);
        playersList = new int[PhotonNetwork.PlayerList.Length];
        initialPlayersNumber = PhotonNetwork.PlayerList.Length;

        DebugHelper.Log("Starting new game");

        if (cachedGameCameraAnimator != null)
        {
            cachedGameCameraAnimator.SetBool("enterMatch", true);
        }

        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        componentList.Clear();
        componentList.AddRange(numbers);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playersList[i] = PhotonNetwork.PlayerList[i].ActorNumber;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // Verificar se há tema da API selecionado
            if (ThemeManager.Instance != null && ThemeManager.Instance.HasSelectedTheme)
            {
                // Novo sistema: usar tema da API
                var selectedTheme = ThemeManager.Instance.SelectedTheme;
                DebugHelper.Log($"[GameManager] Usando tema da API: {selectedTheme.name}");

                randomMaterial.InitializeForTheme(selectedTheme);
                randomMaterial.GetRandomMaterialFromTheme();
            }
            else
            {
                // Sistema legado: usar temas pré-definidos
                DebugHelper.Log($"[GameManager] Usando tema legado: {theme}");
                randomMaterial.GetRandomMaterial(theme);
            }

            this.DelayedCall(6f, StartGame);
        }
    }

    public void StartGame()
    {

        DebugHelper.Log("StartGame()");
        photonView.RPC("ShowHUD", RpcTarget.All);
        //ShowHUD();
    }

    [PunRPC]
    public void ShowHUD()
    {
        // Inicializar cache de referências
        RefreshCache();

        // Inicializar termômetro para nova partida
        if (ThermometerManager.Instance != null)
        {
            ThermometerManager.Instance.Initialize();
        }

        // Quiz desabilitado
        // if (QuizManager.Instance != null)
        // {
        //     QuizManager.Instance.OnQuizCompleted -= OnQuizCompleted;
        //     QuizManager.Instance.OnQuizCompleted += OnQuizCompleted;
        // }

        hud.SetActive(true);
        var outline = FindFirstObjectByType<OutlineAction>();
        if (outline != null)
        {
            outline.MakeObjectsSelectable();
        }

        SetNewTimelineColliders(true);

        var components = hud.GetComponentsInChildren<Transform>();

        DebugHelper.Log("player list lenght: " + PhotonNetwork.PlayerList.Length);

        var orderedPlayerList = PlayerManager.GetOrderedPlayerList();

        for (int i = 0; i < orderedPlayerList.Length; i++)
        {
            int playerNum = i + 1;
            string plateNameStr = GameObjectNames.GetPlateName(playerNum);
            string namePlayerStr = GameObjectNames.GetNamePlayer(playerNum);
            string bonusSymbolStr = GameObjectNames.GetBonusCardSymbol(playerNum);
            string numberCardsStr = GameObjectNames.GetNumberBonusCards(playerNum);

            for (int x = 0; x < components.Length; x++)
            {
                if (components[x].name == plateNameStr)
                {
                    components[x].gameObject.GetComponent<MeshRenderer>().enabled = true;
                }
                else if (components[x].name == "FinishTurn" || components[x].name == "QuitGame")
                {
                    components[x].gameObject.GetComponent<CanvasGroup>().LeanAlpha(1f, 2f);
                }
                else if (components[x].name == namePlayerStr)
                {
                    TextMeshProUGUI textName = components[x].gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    textName.text = orderedPlayerList[i].NickName;
                    textName.GetComponent<CanvasGroup>().LeanAlpha(1f, 2f);
                }
                else if (components[x].name == bonusSymbolStr)
                {
                    components[x].GetComponent<SpriteRenderer>().enabled = true;
                }
                else if (components[x].name == numberCardsStr)
                {
                    components[x].GetComponent<TextMeshProUGUI>().text = "0";
                }
            }
        }
        this.DelayedCall(2f, FirstTurn);
    }
    public void FirstTurn()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        DebugHelper.Log("players lenght: " + players.Length);

        orderedPlayers = new PlayerScript[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            DebugHelper.Log("player name: " + players[i].nickname);
            if (players[i].index == 0)
            {
                orderedPlayers[0] = players[i];
            }
            else if (players[i].index == 1)
            {
                orderedPlayers[1] = players[i];
            }
            else if (players[i].index == 2)
            {
                orderedPlayers[2] = players[i];
            }
            else if (players[i].index == 3)
            {
                orderedPlayers[3] = players[i];
            }
        }

        // Sincronizar início do turno via RPC
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncTurn", RpcTarget.All, time);
        }
    }

    public bool CheckTimeAndIndex(PlayerScript[] orderedPlayers)
    {
        // Verificação de segurança
        if (orderedPlayers == null || orderedPlayers.Length == 0)
        {
            DebugHelper.Log("[CheckTimeAndIndex] orderedPlayers é null ou vazio");
            return true; // Encerrar loop
        }

        // Verificar se algum jogador no índice atual está disponível
        for (int i = 0; i < orderedPlayers.Length; i++)
        {
            DebugHelper.Log("ORDERED -- player name: " + orderedPlayers[i]?.nickname);
            if (orderedPlayers[i] != null && orderedPlayers[i].index == time)
            {
                DebugHelper.Log("Entrou -- return true");
                return true;
            }
        }

        time++;

        // Limite de segurança para evitar loop infinito
        if (time >= 4)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// RPC para sincronizar o turno entre todos os clientes (apenas time)
    /// </summary>
    [PunRPC]
    public void SyncTurn(int syncedTime)
    {
        SyncTurnInternal(syncedTime, -1);
    }

    /// <summary>
    /// RPC para sincronizar o turno entre todos os clientes (time + round)
    /// </summary>
    [PunRPC]
    public void SyncTurnWithRound(int syncedTime, int syncedRound)
    {
        SyncTurnInternal(syncedTime, syncedRound);
    }

    private void SyncTurnInternal(int syncedTime, int syncedRound)
    {
        // Não processar se o jogo não está ativo
        if (!gameIsOn)
        {
            DebugHelper.Log("[GameManager] SyncTurn ignorado - jogo não está ativo");
            return;
        }

        DebugHelper.Log($"[GameManager] SyncTurn recebido: time={syncedTime}, round={syncedRound}");
        time = syncedTime;
        if (syncedRound >= 0)
        {
            round = syncedRound;
        }

        // Garantir que orderedPlayers está populado
        if (orderedPlayers == null || orderedPlayers.Length == 0)
        {
            players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
            if (players == null || players.Length == 0)
            {
                DebugHelper.Log("[GameManager] SyncTurn - nenhum player encontrado");
                return;
            }

            orderedPlayers = new PlayerScript[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;
                if (players[i].index == 0) orderedPlayers[0] = players[i];
                else if (players[i].index == 1) orderedPlayers[1] = players[i];
                else if (players[i].index == 2) orderedPlayers[2] = players[i];
                else if (players[i].index == 3) orderedPlayers[3] = players[i];
            }
        }

        Turn();
    }

    public void Turn()
    {
        // Não processar se o jogo não está ativo
        if (!gameIsOn)
        {
            DebugHelper.Log("[Turn] Ignorado - jogo não está ativo");
            return;
        }

        if (orderedPlayers == null || orderedPlayers.Length == 0)
        {
            DebugHelper.Log("[Turn] orderedPlayers é null ou vazio");
            return;
        }

        for (int i = 0; i < orderedPlayers.Length; i++)
        {
            DebugHelper.Log("ordered pos " + i + "  -  name: " + orderedPlayers[i]?.nickname);
        }

        DebugHelper.Log("Entrou no turn - time: " + time);

        int numberOfPlayers = 4;


        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        bool checkTime = false;
        int maxIterations = 5; // Proteção contra loop infinito
        int iterations = 0;

        while (!checkTime && iterations < maxIterations)
        {
            checkTime = CheckTimeAndIndex(orderedPlayers);
            iterations++;
        }

        if (iterations >= maxIterations)
        {
            DebugHelper.Log("[Turn] Loop de CheckTimeAndIndex atingiu limite máximo");
        }


        DebugHelper.Log("5 -- Turn()");
        DebugHelper.Log("time: " + time + " < numPlayers: " + numberOfPlayers);
        if (time < numberOfPlayers)
        {
            foreach (var player in players)
            {
                if (player.index == time)
                {
                    player.SetYourTurn(true);
                    ChangeBonusCardsView(player);
                    photonView.RPC("ChangePlateNameMaterial", RpcTarget.All, player.plateNameIndex);
                }
                else
                {
                    player.SetYourTurn(false);
                }
            }

            ShowRoundInfo();

        }
        else
        {
            DebugHelper.Log("Caiu no else - nova rodada");
            round++;
            time = 0;

            // Sincronizar novo round via RPC (apenas MasterClient)
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SyncTurnWithRound", RpcTarget.All, time, round);
            }
        }

    }

    [PunRPC]
    public void ChangePlateNameMaterial(int plateNameIndex)
    {
        string plateNameText = GameObjectNames.GetPlateName(plateNameIndex + 1);

        var plateNames = GetCachedPlateNames();

        foreach (GiveCards plateName in plateNames)
        {
            if (plateName == null) continue;
            if(plateName.name == plateNameText)
            {
                plateName.GetComponent<MeshRenderer>().material = plateNameMaterial2;
            }
            else
            {
                plateName.GetComponent<MeshRenderer>().material = plateNameMaterial;
            }
        }
    }

    //[PunRPC]
    public void ShowRoundInfo()
    {
        //DebugHelper.Log("ShowRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        gameInfo.gameObject.SetActive(true);

        // Verificação de segurança para orderedPlayers e time
        string currentPlayerName = "Player";
        if (orderedPlayers != null && time >= 0 && time < orderedPlayers.Length && orderedPlayers[time] != null)
        {
            currentPlayerName = orderedPlayers[time].nickname;
        }

        if (round == roundCompare)
        {
            roundCompare++;

            foreach (var info in infos)
            {

                if (info.gameObject.name == "TurnInfo")
                {
                    info.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = currentPlayerName + "'s Turn";
                }
                if (info.gameObject.name == "RoundInfo")
                {
                    info.GetComponentInChildren<TextMeshProUGUI>().text = "Starting Round " + round;
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
                    info.GetComponentInChildren<TextMeshProUGUI>().text = currentPlayerName + "'s Turn";
                }
                if (info.gameObject.name == "TurnInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }

            }
        }
        this.DelayedCall(1.5f, HideRoundInfo);
    }

    public void HideRoundInfo()
    {
        //DebugHelper.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        //DebugHelper.Log("DisableGameInfo()");
        gameInfo.gameObject.SetActive(false);
        StartTurn();
    }

    public void StartTurn()
    {
        // Finalizar transição de turno e reabilitar cursor
        IsInTurnTransition = false;
        InputBlocker.Unblock();

        // Garantir que coliders não-slot da timeline estejam reativados no início de cada turno
        SetNewTimelineNonSlotColliders(true);

        // Garantir que os slots da timeline estejam desativados no início de cada turno
        // Os slots só devem ser ativados quando o jogador comprar uma carta de evento
        var eventSlot = FindFirstObjectByType<EventSlot>();
        if (eventSlot != null)
        {
            eventSlot.SetUpSlots(false, "Undestructable");
        }

        // Iniciar o cronômetro do turno via RPC (apenas MasterClient)
        if (PhotonNetwork.IsMasterClient)
        {
            StartTurnTimerRPC();
        }

        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        DebugHelper.Log("tamanho da lista: " + PhotonNetwork.PlayerList.Length);
        DebugHelper.Log("Time: " + time);

        // Verificação de segurança para orderedPlayers e time
        PlayerScript currentOrderedPlayer = null;
        if (orderedPlayers != null && time >= 0 && time < orderedPlayers.Length)
        {
            currentOrderedPlayer = orderedPlayers[time];
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            DebugHelper.Log("player na vez: " + currentOrderedPlayer?.nickname + " -- player: "+ PhotonNetwork.PlayerList[i].NickName);
            if (currentOrderedPlayer != null && currentOrderedPlayer.actorNumber == PhotonNetwork.PlayerList[i].ActorNumber)
            {

                DebugHelper.Log("Jogador: " + currentOrderedPlayer.nickname + " está na vez  -- recebendo photon views");
                if (cachedDeckEventPhotonView != null && cachedDeckEventPhotonView.ViewID > 0) cachedDeckEventPhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (cachedDeckBonusPhotonView != null && cachedDeckBonusPhotonView.ViewID > 0) cachedDeckBonusPhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (cachedTimelinePhotonView != null && cachedTimelinePhotonView.ViewID > 0) cachedTimelinePhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (photonView != null && photonView.ViewID > 0) photonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (cachedEndButtonPhotonView != null && cachedEndButtonPhotonView.ViewID > 0) cachedEndButtonPhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);

                var plateNames = GetCachedPlateNames();

                foreach (GiveCards plateName in plateNames)
                {
                    if (plateName == null) continue;
                    var pv = plateName.GetComponent<PhotonView>();
                    if (pv != null && pv.ViewID > 0)
                    {
                        DebugHelper.Log("transferindo platename: "+plateName.name);
                        pv.TransferOwnership(PhotonNetwork.PlayerList[i]);
                    }
                }

                foreach (var component in timeCraxComponents)
                {
                    if (component == null) continue;
                    var pv = component.GetComponent<PhotonView>();
                    if (pv != null && pv.ViewID > 0)
                    {
                        pv.TransferOwnership(PhotonNetwork.PlayerList[i]);
                    }
                }
            }
        }

        // Encontrar o jogador LOCAL (controlado por este cliente)
        PlayerScript localPlayer = null;
        PlayerScript currentTurnPlayer = null;

        foreach (var player in players)
        {
            DebugHelper.Log("jogador " + player.name + " index: " + player.index + " -----  time: " + time);

            // Verificar se é o jogador local
            if (player.photonView.IsMine)
            {
                localPlayer = player;
            }

            // Verificar se é o jogador no turno atual
            if (player.index == time)
            {
                currentTurnPlayer = player;
            }
        }

        // Verificar se o jogador LOCAL está no turno
        bool isMyTurn = localPlayer != null && localPlayer.index == time;

        DebugHelper.Log($"[StartTurn] isMyTurn: {isMyTurn}, localPlayer: {localPlayer?.nickname}, currentTurnPlayer: {currentTurnPlayer?.nickname}");

        if (isMyTurn && currentTurnPlayer != null)
        {

            foreach (var timeCraxComponent in timeCraxComponents)
            {
                if (timeCraxComponent.malfunctions == 1)
                {
                    timeCraxComponent.tag = "Selectable";
                }
            }

            DebugHelper.Log("Ativando MeshCollider dos objetos");
            if (cachedEndButtonMeshCollider != null) cachedEndButtonMeshCollider.enabled = true;
            if (cachedQuitButtonMeshCollider != null) cachedQuitButtonMeshCollider.enabled = true;

            if (cachedTimelineMeshCollider != null) cachedTimelineMeshCollider.enabled = true;
            if (cachedDeckEventMeshCollider != null) cachedDeckEventMeshCollider.enabled = true;
            // deckBonus só é habilitado após acertar quiz
            if (cachedDeckBonusMeshCollider != null) cachedDeckBonusMeshCollider.enabled = false;
            if (deckBonus != null) deckBonus.tag = "Untagged";
            deckEvent.tag = "Selectable";
            timeline.tag = "Selectable";

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                var plate = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
                if (plate != null)
                {
                    plate.GetComponent<MeshCollider>().enabled = true;
                    plate.tag = "Selectable";
                }
            }

            var findObject = GameObject.Find(GameObjectNames.GetPlateName(time + 1));
            if (findObject != null)
            {
                findObject.GetComponent<MeshCollider>().enabled = false;
            }
        }
        else
        {
            // NÃO é meu turno - desativar controles
            if (localPlayer != null)
            {
                localPlayer.SetYourTurn(false);
            }

            if (cachedEndButtonMeshCollider != null) cachedEndButtonMeshCollider.enabled = false;
            if (cachedTimelineMeshCollider != null) cachedTimelineMeshCollider.enabled = false;
            if (cachedDeckEventMeshCollider != null) cachedDeckEventMeshCollider.enabled = false;
            if (cachedDeckBonusMeshCollider != null) cachedDeckBonusMeshCollider.enabled = false;

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                var plate = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
                if (plate != null)
                {
                    plate.GetComponent<MeshCollider>().enabled = false;
                }
            }
        }
    }

    public void RandomComponentNumber()
    {
        // Criar lista de componentes elegíveis (excluir os que já têm malfunction=2)
        List<int> eligibleComponents = new List<int>();

        if (timeCraxComponents != null)
        {
            foreach (var component in timeCraxComponents)
            {
                if (component != null && component.malfunctions < 2)
                {
                    eligibleComponents.Add(component.componentId);
                }
            }
        }

        if (eligibleComponents.Count == 0)
        {
            DebugHelper.Log("[GameManager] Nenhum componente elegível para malfunction!");
            return;
        }

        // Sortear entre os elegíveis
        int randomIndex = UnityEngine.Random.Range(0, eligibleComponents.Count);
        randomId = eligibleComponents[randomIndex];
        DebugHelper.Log($"[GameManager] Componente sorteado: {randomId} (de {eligibleComponents.Count} elegíveis)");

        InputBlocker.Block();

        photonView.RPC("ComponentRandom", RpcTarget.All, randomId);
    }

    /// <summary>
    /// Verifica se a condição de derrota foi atingida (3 componentes com malfunction=2)
    /// </summary>
    public void CheckGameOverCondition()
    {
        if (timeCraxComponents == null) return;

        int criticalComponents = 0;

        foreach (var component in timeCraxComponents)
        {
            if (component != null && component.malfunctions >= 2)
            {
                criticalComponents++;
            }
        }

        DebugHelper.Log($"[GameManager] Componentes críticos: {criticalComponents}/1");

        if (criticalComponents >= 2)
        {
            DebugHelper.Log("[GameManager] GAME OVER - 1 componente com malfunction crítico!");

            if (gameOver != null)
            {
                gameOver.gameIsOver = true;
            }

            this.DelayedCall(3f, TriggerGameOver);
        }
    }

    /// <summary>
    /// Executa o Game Over
    /// </summary>
    public void SetNewTimelineColliders(bool enabled)
    {
        if (suitTop == null) return;
        Transform newTimeline = suitTop.transform.Find("NewTimeline");
        if (newTimeline == null) return;
        string tag = enabled ? "Selectable" : "Undestructable";
        foreach (Transform child in newTimeline.GetComponentsInChildren<Transform>(true))
        {
            var col = child.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = enabled;
                child.tag = tag;
                DebugHelper.Log($"[SetNewTimelineColliders({enabled})] {child.name}");
            }
        }
    }

    /// <summary>
    /// Ativa/desativa MeshColliders dos filhos de NewTimeline que NÃO pertencem a NewSlotEvents.
    /// Chamado durante zoom in/out para impedir cliques em elementos visuais da timeline
    /// enquanto o jogador posiciona uma carta de evento nos slots.
    /// </summary>
    public void SetNewTimelineNonSlotColliders(bool enabled)
    {
        if (suitTop == null) return;
        Transform newTimeline = suitTop.transform.Find("NewTimeline");
        if (newTimeline == null) return;

        foreach (Transform child in newTimeline.GetComponentsInChildren<Transform>())
        {
            if (child == newTimeline) continue;
            // Pular objetos com EventSlot e seus filhos (independente da hierarquia)
            if (child.GetComponentInParent<EventSlot>(true) != null) continue;

            var col = child.GetComponent<Collider>();
            if (col != null) col.enabled = enabled;

            var tc = child.GetComponent<TimelineChild>();
            if (tc != null) tc.enabled = enabled;
        }
        DebugHelper.Log($"[GameManager] SetNewTimelineNonSlotColliders({enabled}) executado");
    }

    private void TriggerGameOver()
    {
        BackgroundMusic bgMusic = FindFirstObjectByType<BackgroundMusic>();

        SetNewTimelineColliders(false);
        DeactivateAll();
        ResetAllComponents();
        ResetAllPlatenames();

        if (bgMusic != null)
        {
            bgMusic.PlayGameOverSound();
        }

        if (gameOver != null)
        {
            gameOver.transform.GetChild(0).gameObject.SetActive(true);
        }

        if (hud != null)
        {
            hud.SetActive(false);
        }

        InputBlocker.Unblock();

        // Fechar compartimento direito
        if (rightCompartmentAnimator != null)
        {
            rightCompartmentAnimator.SetBool("open", false);
        }
    }

    [PunRPC]
    public void ComponentRandom(int id)
    {
        randomId = id;

        StartCoroutine(Roulettecomponent());

    }

    /// <summary>
    /// Coroutine que anima a roleta de componentes antes de adicionar malfunction.
    /// Nota: Não é um RPC - é chamada localmente após receber ComponentRandom via RPC.
    /// </summary>
    private IEnumerator Roulettecomponent()
    {
        int randomIndex = 0;
        int cond = 0;
        float interval = 0.3f;
        int componentCount = timeCraxComponents != null ? timeCraxComponents.Length : 0;

        if (componentCount == 0)
        {
            DebugHelper.Log("[GameManager] ERRO: timeCraxComponents está vazio!");
            yield break;
        }

        while(cond < 15)
        {
            int index = UnityEngine.Random.Range(0, componentCount);
            while(index == randomIndex && componentCount > 1)
            {
                index = UnityEngine.Random.Range(0, componentCount);
            }
            randomIndex = index;
            //DebugHelper.Log("Random: " + index);
            //DebugHelper.Log("Ativando outline do component: " + timeCraxComponents[index].name);
            timeCraxComponents[index].GetComponent<OutlineComponent>().enabled = true;
            soundEffects.PlayRouletteSound();
            yield return new WaitForSeconds(interval);

            //DebugHelper.Log("Destivando outline do component: " + timeCraxComponents[index].name);
            timeCraxComponents[index].GetComponent<OutlineComponent>().enabled = false;

            cond++;
            interval -= 0.015f;
        }

        //DebugHelper.Log("Random: " + (randomId - 1));

        // Encontrar o componente final pelo componentId (não pelo índice)
        MachineComponent finalComponent = null;
        foreach (var comp in timeCraxComponents)
        {
            if (comp != null && comp.componentId == randomId)
            {
                finalComponent = comp;
                break;
            }
        }

        if (finalComponent != null)
        {
            var outline = finalComponent.GetComponent<OutlineComponent>();
            if (outline != null)
            {
                outline.enabled = true;
                soundEffects.PlayRouletteSound();
                yield return new WaitForSeconds(interval);
                outline.enabled = false;
            }
        }

        AddMalfunctionInComponent();
    }

    public void AddMalfunctionInComponent()
    {
        // Só reabilitar cursor se NÃO estiver em transição de turno
        if (!IsInTurnTransition)
        {
            InputBlocker.Unblock();
        }

        foreach (var component in timeCraxComponents)
        {
            if (component != null && component.componentId == randomId)
            {
                component.AddMalfunction();
            }
        }

        // Resetar temperatura para primeiro nível APÓS aplicar malfunction
        // (importante para coolers que alteram a progressão)
        if (PhotonNetwork.IsMasterClient && ThermometerManager.Instance != null)
        {
            ThermometerManager.Instance.ResetTemperatureToFirstLevel();
        }
    }

    public void EndTurn()
    {
        // Parar o cronômetro do turno via RPC (apenas MasterClient)
        if (PhotonNetwork.IsMasterClient)
        {
            StopTurnTimerRPC();
        }

        float waiting;
        waiting = 0;


        int lastPlayerIndex = 3;

        DebugHelper.Log("orderedPlayers.Length: " + orderedPlayers.Length);
        for(int i = orderedPlayers.Length - 1; i >= 0; i--)
        {
            DebugHelper.Log("-- "+orderedPlayers[i]);
            if(orderedPlayers[i] != null)
            {
                DebugHelper.Log("ultimo player �: " + orderedPlayers[i].nickname + " com index: " + orderedPlayers[i].index);
                lastPlayerIndex = orderedPlayers[i].index;
                break;
            }
        }

        this.DelayedCall(waiting, WaitForFinishTurn);

    }

    public void WaitForFinishTurn()
    {

        DebugHelper.Log(" -------------------->>>>>  game is over?: "+ gameOver.gameIsOver);
        if (!gameOver.gameIsOver)
        {
            DebugHelper.Log("Game is ON");

            // Aumentar temperatura quando turno é finalizado (apenas MasterClient)
            if (PhotonNetwork.IsMasterClient && ThermometerManager.Instance != null)
            {
                // Calcular tempo de espera baseado na animação do termômetro
                float waitTime = ThermometerManager.Instance.GetErrorProcessingTime();

                // Iniciar animação do termômetro imediatamente
                ThermometerManager.Instance.OnPlayerError();

                // Esperar a animação terminar antes de passar o turno
                this.DelayedCall(waitTime, () =>
                {
                    if (!gameOver.gameIsOver)
                    {
                        photonView.RPC("FinishTurn", RpcTarget.All);
                    }
                });
            }
            else
            {
                // Se não for MasterClient, apenas passa o turno
                photonView.RPC("FinishTurn", RpcTarget.All);
            }
        }

    }

    /// <summary>
    /// Chamado pelo TurnTimer quando o tempo do turno expira.
    /// Passa o turno automaticamente para o próximo jogador.
    /// </summary>
    public void AutoEndTurn()
    {
        DebugHelper.Log("[GameManager] AutoEndTurn - Tempo do turno expirado!");

        // Verificar se o jogo ainda está ativo
        if (!gameIsOn)
        {
            DebugHelper.Log("[GameManager] AutoEndTurn ignorado - jogo não está ativo");
            return;
        }

        // Verificar se não é game over
        if (gameOver != null && gameOver.gameIsOver)
        {
            DebugHelper.Log("[GameManager] AutoEndTurn ignorado - game over");
            return;
        }

        // Enviar RPC para todos os clientes executarem a limpeza do timeout
        photonView.RPC("RPC_HandleTimeoutCleanup", RpcTarget.All);
    }

    /// <summary>
    /// RPC que executa a limpeza quando o tempo do turno expira.
    /// Sincroniza em todos os clientes: reseta câmera, devolve carta, e passa turno.
    /// </summary>
    [PunRPC]
    public void RPC_HandleTimeoutCleanup()
    {
        DebugHelper.Log("[GameManager] RPC_HandleTimeoutCleanup - Executando limpeza de timeout");

        // Marcar que está em transição de turno
        IsInTurnTransition = true;

        InputBlocker.Block();

        // Quiz desabilitado
        // if (QuizManager.Instance != null && QuizManager.Instance.IsQuizActive)
        // {
        //     QuizManager.Instance.ForceCloseQuiz();
        // }

        // 2. Verificar se há uma carta de evento comprada (tag "Drew")
        EventCard drewCard = null;
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in eventCards)
        {
            if (card != null && card.CompareTag("Drew"))
            {
                drewCard = card;
                DebugHelper.Log($"[GameManager] Carta comprada encontrada: slotCount={card.slotCount}");
                break;
            }
        }

        // 3. Se há carta comprada, devolver ao deck e resetar visual
        if (drewCard != null)
        {
            int cardSlotCount = drewCard.slotCount;

            // Resetar estado visual da carta
            drewCard.ResetStatusCard();
            drewCard.tag = "Untagged";

            // Devolver carta ao deck (apenas MasterClient sincroniza)
            if (PhotonNetwork.IsMasterClient && deckEvent != null)
            {
                deckEvent.AddCardBack(cardSlotCount);
            }

            DebugHelper.Log($"[GameManager] Carta {cardSlotCount} devolvida ao deck");
        }

        // 4. Verificar se a câmera está em modo zoom antes de resetar
        bool wasInZoomMode = gameCamera != null && gameCamera.IsInZoomMode();

        // 5. Forçar reset da câmera e estados relacionados
        if (gameCamera != null)
        {
            if (wasInZoomMode)
            {
                gameCamera.DistanceTimeline();
                DebugHelper.Log("[GameManager] Câmera saindo do modo zoom");
            }
            else
            {
                // Apenas forçar reset dos estados sem animação
                gameCamera.ForceResetToInitialState();
                DebugHelper.Log("[GameManager] Câmera já estava no estado inicial, forçando reset");
            }
        }

        // 6. Desativar slots da timeline (redundante mas garante)
        var timeline = FindFirstObjectByType<Timeline>();
        if (timeline != null)
        {
            timeline.ActiveTimeline(false);
        }

        // 7. Desativar seleção de slots (redundante mas garante)
        var eventSlot = FindFirstObjectByType<EventSlot>();
        if (eventSlot != null)
        {
            eventSlot.SetUpSlots(false, "Undestructable");
        }

        // 8. Chamar RandomComponentNumber para adicionar malfunction (apenas MasterClient)
        if (PhotonNetwork.IsMasterClient)
        {
            // Se estava em zoom, esperar a animação; senão, delay menor
            float delay = wasInZoomMode ? 1.5f : 0.5f;
            this.DelayedCall(delay, TimeoutMalfunction);
        }
    }

    /// <summary>
    /// Chamado após o delay do timeout para avançar termômetro e passar turno.
    /// </summary>
    private void TimeoutMalfunction()
    {
        DebugHelper.Log("[GameManager] TimeoutMalfunction - Avançando termômetro e passando turno");

        // Verificar novamente se o jogo ainda está ativo
        if (!gameIsOn || (gameOver != null && gameOver.gameIsOver))
        {
            DebugHelper.Log("[GameManager] TimeoutMalfunction cancelado - jogo não está ativo ou game over");
            return;
        }

        // Obter tempo de processamento antes de chamar OnPlayerError
        float processingTime = 0f;
        if (ThermometerManager.Instance != null)
        {
            processingTime = ThermometerManager.Instance.GetErrorProcessingTime();
            // Avançar termômetro (malfunction só acontece se temperatura chegar a 100)
            ThermometerManager.Instance.OnPlayerError();
        }

        // Aguardar processamento (se houver malfunction, espera a animação)
        float delay = processingTime > 0 ? processingTime + 0.5f : 0.5f;
        this.DelayedCall(delay, FinishTurnAfterTimeout);
    }

    /// <summary>
    /// Finaliza o turno após o timeout (chamado após a animação de malfunction).
    /// </summary>
    private void FinishTurnAfterTimeout()
    {
        DebugHelper.Log("[GameManager] FinishTurnAfterTimeout - Passando turno");

        if (!gameIsOn || (gameOver != null && gameOver.gameIsOver))
        {
            return;
        }

        photonView.RPC("FinishTurn", RpcTarget.All);
    }

    #region TurnTimer RPCs

    /// <summary>
    /// Sincroniza o tempo do timer para outros clientes
    /// </summary>
    public void SyncTurnTimer(float time)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_SyncTurnTimer", RpcTarget.Others, time);
        }
    }

    [PunRPC]
    public void RPC_SyncTurnTimer(float time)
    {
        if (turnTimer != null)
        {
            turnTimer.SyncTime(time);
        }
    }

    /// <summary>
    /// Para o timer em todos os clientes via RPC
    /// </summary>
    public void StopTurnTimerRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_StopTurnTimer", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_StopTurnTimer()
    {
        if (turnTimer != null)
        {
            turnTimer.StopTimerLocal();
        }
    }

    /// <summary>
    /// Inicia o timer em todos os clientes via RPC
    /// </summary>
    public void StartTurnTimerRPC()
    {
        if (PhotonNetwork.IsMasterClient && turnTimer != null)
        {
            photonView.RPC("RPC_StartTurnTimer", RpcTarget.All, turnTimer.TimeLimit);
        }
    }

    [PunRPC]
    public void RPC_StartTurnTimer(float time)
    {
        if (turnTimer != null)
        {
            turnTimer.StartTimerLocal(time);
        }
    }

    #endregion

    public void SetUpComponents()
    {
        foreach (var player in players)
        {
            DebugHelper.Log("6 -- Player --: " + player.nickname);
            foreach (var component in timeCraxComponents)
            {
                if (component.malfunctions == 1)
                {
                    //DebugHelper.Log("component " + randomId + "mesh: " + component.GetComponent<MeshCollider>().enabled);
                    //if (component.photonView.IsMine)
                    if (player.GetYourTurn())
                    {
                        //DebugHelper.Log("Player: " + player.nickname + " - comp: " + component.name + " ativado");
                        component.GetComponent<MeshCollider>().enabled = true;
                    }
                    else
                    {
                        //DebugHelper.Log("Player: " + player.nickname + " - comp: " + component.name + " desativado");
                        component.GetComponent<MeshCollider>().enabled = false;
                    }
                    //DebugHelper.Log("component " + randomId + "mesh depois: " + component.GetComponent<MeshCollider>().enabled);
                }
            }
        }

    }

    [PunRPC]
    public void FinishTurn()
    {
        DebugHelper.Log("4 -- Finish turn, time ++");

        // Fechar compartimento esquerdo e desabilitar deckBonus
        if (leftCompartmentAnimator != null)
        {
            leftCompartmentAnimator.SetBool("open", false);
        }
        if (cachedDeckBonusMeshCollider != null)
        {
            cachedDeckBonusMeshCollider.enabled = false;
        }
        if (deckBonus != null) deckBonus.tag = "Untagged";

        // Verificações de segurança
        if (deckEvent != null) deckEvent.tag = "Disabled";
        if (timeline != null) timeline.tag = "Disabled";

        // Apenas MasterClient incrementa e sincroniza o time
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected && photonView != null)
        {
            time++;
            photonView.RPC("SyncTurn", RpcTarget.All, time);
        }

        SetUpComponents();
    }

    public void ChangeBonusCardsView(PlayerScript player)
    {
        if (player == null) return;

        var bonusCards = GetCachedBonusCards();

        foreach (var card in bonusCards)
        {
            if (card == null) continue;
            if (card.photonView.OwnerActorNr == player.photonView.OwnerActorNr)
            {
                //DebugHelper.Log("set true");
                card.GetComponent<Animator>().SetBool("sending", false);
                card.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                //DebugHelper.Log("set false");
                card.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void CheckQuitGamePlayer()
    {
        // Verificações de segurança
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            DebugHelper.Log("[CheckQuitGamePlayer] Não conectado ou LocalPlayer null");
            return;
        }

        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

        if (players == null || players.Length == 0)
        {
            DebugHelper.Log("[CheckQuitGamePlayer] Nenhum player encontrado");
            return;
        }

        foreach (var player in players)
        {
            if (player == null || player.photonView == null) continue;

            DebugHelper.Log("2 -- LocalPlayer ActrNumber: " + PhotonNetwork.LocalPlayer?.ActorNumber + " --- Photon ActrNumber: " + player.photonView.ControllerActorNr);

            bool isGameOver = gameOver != null && gameOver.gameIsOver;

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.photonView.ControllerActorNr && !isGameOver)
            {
                DebugHelper.Log("Chamando ShowLeftPlayer");

                // Enviar RPC apenas se ainda estiver conectado
                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("ShowLeftPlayerInfo", RpcTarget.Others, player.nickname);
                }

                if (player.GetYourTurn())
                {
                    DebugHelper.Log("numero de players: " + PhotonNetwork.PlayerList.Length);
                    if (PhotonNetwork.PlayerList.Length > 1 && PhotonNetwork.IsConnected && photonView != null)
                    {
                        photonView.RPC("FinishTurn", RpcTarget.Others); // Enviar apenas para outros, não para si mesmo
                    }
                }

                break; // Sair do loop após encontrar o jogador local
            }
        }
    }

    [PunRPC]
    public void ShowLeftPlayerInfo(string nickname)
    {
        DebugHelper.Log("Entrou no ShowLeftPlayer - nickname: " + nickname);

        if (gameInfo == null)
        {
            DebugHelper.Log("ShowLeftPlayerInfo: gameInfo é null");
            return;
        }

        if (playerLeftBackground == null)
        {
            DebugHelper.Log("ShowLeftPlayerInfo: playerLeftBackground é null");
            return;
        }

        DebugHelper.Log("ShowLeftPlayerInfo: Ativando gameInfo...");
        gameInfo.gameObject.SetActive(true);
        DebugHelper.Log("ShowLeftPlayerInfo: gameInfo ativado");

        var tmpText = playerLeftBackground.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = nickname + " left the game";
        }

        var canvasGroup = playerLeftBackground.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.LeanAlpha(1f, 0.5f);
        }

        this.DelayedCall(1.5f, HideLeftPlayerInfo);
    }

    public void HideLeftPlayerInfo()
    {
        DebugHelper.Log("Entrou no HideLeftPlayer");

        if (playerLeftBackground == null) return;

        var canvasGroup = playerLeftBackground.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.LeanAlpha(0f, 0.5f);
        }

        this.DelayedCall(0.5f, DisableOnlyGameInfo);
    }

    public void DisableOnlyGameInfo()
    {
        if (gameInfo != null)
        {
            gameInfo.gameObject.SetActive(false);
        }
    }

    public void BackToMenu()
    {
        DebugHelper.Log("[BackToMenu] Iniciando saída do jogo");

        // Marcar jogo como inativo ANTES de notificar outros jogadores
        // Isso evita que RPCs de turno continuem sendo processados
        gameIsOn = false;

        // Parar e esconder o timer
        if (turnTimer != null)
        {
            turnTimer.StopTimerLocal();
        }

        CheckQuitGamePlayer();

        this.DelayedCall(0.7f, SetUpBackToMenu);
    }

    public void SetUpBackToMenu()
    {
        DebugHelper.Log("8 -- SeUPBackToMenu");

        if (backgroundMusic != null) backgroundMusic.PlayMenuSound();

        gameIsOn = false;
        if (gameOver != null) gameOver.gameIsOver = false;

        // Fechar compartimento direito
        if (rightCompartmentAnimator != null)
        {
            rightCompartmentAnimator.SetBool("open", false);
        }

        if (deckEvent != null) deckEvent.ResetAllEventCards();

        var gameConnection = FindFirstObjectByType<GameConnection>();
        if (gameConnection != null)
        {
            gameConnection.OnLeftRoom();
            gameConnection.DisconectAndReconect();
        }

        if (suitTop != null && cachedSuitTopAnimator != null)
        {
            cachedSuitTopAnimator.enabled = true;
            cachedSuitTopAnimator.SetBool("openSuit", false);
        }
    }

    public void ResetAllPlatenames()
    {
        DebugHelper.Log("11 -- Resetando platenames");
        for (int i = 0; i < 4; i++)
        {
            int playerNum = i + 1;

            var plate = GameObject.Find(GameObjectNames.GetPlateName(playerNum));
            if (plate != null)
            {
                DebugHelper.Log("plate name: " + plate.name);
                plate.GetComponent<MeshRenderer>().enabled = false;
                plate.GetComponent<MeshCollider>().enabled = false;
            }

            var bonusSymbol = GameObject.Find(GameObjectNames.GetBonusCardSymbol(playerNum));
            if (bonusSymbol != null)
            {
                DebugHelper.Log("bonusSymbol name: " + bonusSymbol.name);
                bonusSymbol.GetComponent<SpriteRenderer>().enabled = false;
            }

            var namePlate = GameObject.Find(GameObjectNames.GetNamePlayer(playerNum));
            if (namePlate != null)
            {
                DebugHelper.Log("namePlate name: " + namePlate.name);
                namePlate.GetComponent<TMP_Text>().text = " ";
                namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }

            var numberBonusCard = GameObject.Find(GameObjectNames.GetNumberBonusCards(playerNum));
            if (numberBonusCard != null)
            {
                DebugHelper.Log("bonusCardSymbol name: " + numberBonusCard.name);
                numberBonusCard.GetComponent<TextMeshProUGUI>().text = " ";
            }
        }
    }
    public void ResetAllComponents()
    {
        DebugHelper.Log("10 -- ResetAllComponents");

        // Resetar termômetro
        if (ThermometerManager.Instance != null)
        {
            ThermometerManager.Instance.ResetThermometer();
        }

        if (timeCraxComponents != null)
        {
            foreach (var component in timeCraxComponents)
            {
                if (component != null)
                {
                    component.ResetComponent();
                }
            }
        }

        if (componentsWithAnimator != null)
        {
            foreach (var component in componentsWithAnimator)
            {
                if (component == null) continue;

                DebugHelper.Log("opcName: " + component.name);
                var animator = component.GetComponent<Animator>(); if (animator != null) animator.SetBool("malfunction", false);
                if (animator != null) animator.enabled = false;
                component.tag = "Component";

                ParticleSystem[] effects = component.GetComponentsInChildren<ParticleSystem>(true);

                foreach (var effect in effects)
                {
                    effect.gameObject.SetActive(false);
                }
            }
        }

        //Transform[] opcoes = enviroment.GetComponentsInChildren<Transform>();
        //foreach (var opc in opcoes)
        //{
        //    if (opc.GetComponent<Animator>())
        //    {
        //        DebugHelper.Log("opcName: "+opc.gameObject.name);
        //        opc.GetComponent<Animator>().SetBool("malfunction", false);

        //        ParticleSystem effect = opc.GetComponentInChildren<ParticleSystem>(true);
        //        effect.gameObject.SetActive(false);
        //    }
        //}
    }

    public void ActivateEnd()
    {
        // Verificar se o jogador LOCAL está na vez
        bool shouldEnable = false;
        foreach (var player in players)
        {
            if (player.index == time && player.photonView.IsMine)
            {
                DebugHelper.Log("player " + player.nickname + " est� na vez (local)");
                shouldEnable = true;
                break;
            }
        }

        if (cachedEndButtonMeshCollider != null)
        {
            cachedEndButtonMeshCollider.enabled = shouldEnable;
            DebugHelper.Log("EndButton enabled: " + shouldEnable);
        }
    }

    public void ActivateFinishButton(bool activate)
    {

        if (cachedEndButtonMeshCollider != null) cachedEndButtonMeshCollider.enabled = activate;

    }


    public void BlockActions()
    {
        deckBonus.tag = "Disabled";
        deckEvent.tag = "Disabled";

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var findObject = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
            if (findObject != null)
            {
                findObject.tag = "Disabled";
            }
        }

        var suitComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);
        foreach (var suitComponent in suitComponents)
        {
            if (suitComponent != null && suitComponent.malfunctions == 1)
            {
                suitComponent.tag = "Disabled";
            }
        }
    }

    public void DeactivateAll()
    {
        // Parar e esconder o timer
        if (turnTimer != null)
        {
            turnTimer.StopTimerLocal();
        }

        DebugHelper.Log("9 -- Desativando platenames");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var findObject = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
            if (findObject != null)
            {
                findObject.GetComponent<MeshCollider>().enabled = false;
            }
        }

        DebugHelper.Log("Desativando components");
        var suitComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);
        foreach (var suitComponent in suitComponents)
        {
            if (suitComponent != null && suitComponent.malfunctions > 0)
            {
                DebugHelper.Log(suitComponent.name + " with malfunction > 0 becoming false");
                suitComponent.GetComponent<MeshCollider>().enabled = false;
            }
        }

        DebugHelper.Log("Desativando decks");
        if (cachedDeckEventMeshCollider != null) cachedDeckEventMeshCollider.enabled = false;
        if (cachedDeckBonusMeshCollider != null) cachedDeckBonusMeshCollider.enabled = false;
        if (cachedTimelineMeshCollider != null) cachedTimelineMeshCollider.enabled = false;
        if (cachedEndButtonMeshCollider != null) cachedEndButtonMeshCollider.enabled = false;
        if (cachedQuitButtonMeshCollider != null) cachedQuitButtonMeshCollider.enabled = false;
        
    }

    public void GiveCard(int numberPlayer)
    {
        //string button = EventSystem.current.currentSelectedGameObject.name;
        //DebugHelper.Log("Nome: " + button);
        //int buttonName = int.Parse(EventSystem.current.currentSelectedGameObject.name);

        photonView.RPC("GiveBonusCard", RpcTarget.All, numberPlayer);

    }

    [PunRPC]
    public void GiveBonusCard(int numberPlayer)
    {

        PlayerScript playerSending = null;
        PlayerScript playerReceiving = null;

        // Usar cache se disponível, senão buscar novamente
        if (players == null || players.Length == 0)
        {
            players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        }

        foreach (var player in players)
        {
            if (player == null) continue;
            if (player.GetYourTurn())
            {
                playerSending = player;
            }
            else if (player.index == numberPlayer - 1)
            {
                playerReceiving = player;
            }
        }

        // Verificar se encontrou os jogadores necessários
        if (playerSending == null || playerReceiving == null)
        {
            DebugHelper.Log("[GiveBonusCard] playerSending ou playerReceiving é null");
            return;
        }

        if (playerSending.GetNumberOfBonusCards() > 0 && playerReceiving.GetNumberOfBonusCards() < 5)
        {
            BlockActions();

            // Invalidar cache de bonus cards para buscar as mais recentes
            needsCacheRefresh = true;
            var bonusCards = GetCachedBonusCards();
            List<BonusCard> orderedList = new List<BonusCard>();
            List<BonusCard> playerCards = new List<BonusCard>();

            foreach (var bonusCard in bonusCards)
            {
                if (bonusCard != null && bonusCard.photonView.OwnerActorNr == playerSending.photonView.OwnerActorNr)
                {
                    DebugHelper.Log(" - " + bonusCard.photonView.ViewID);
                    playerCards.Add(bonusCard);
                }
            }

            if (playerCards.Count == 0)
            {
                DebugHelper.Log("[GiveBonusCard] Nenhuma carta encontrada para o jogador");
                return;
            }

            orderedList = playerCards.OrderByDescending(x => x.index).ToList();
            BonusCard lastCard = orderedList[0];

            var orderedPlayerList = PlayerManager.GetOrderedPlayerList();

            if (playerReceiving.index >= 0 && playerReceiving.index < orderedPlayerList.Length)
            {
                if (lastCard.photonView != null && lastCard.photonView.ViewID > 0)
                {
                    lastCard.photonView.TransferOwnership(orderedPlayerList[playerReceiving.index]);
                }
            }

            playerReceiving.numberBonusCards++;

            var findReceiverNumberCards = GameObject.Find(GameObjectNames.GetNumberBonusCards(numberPlayer));
            if (findReceiverNumberCards != null)
            {
                DebugHelper.Log("receiver: " + findReceiverNumberCards.name);
                findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text = playerReceiving.numberBonusCards.ToString();
            }

            playerSending.numberBonusCards--;

            var findSenderNumberCards = GameObject.Find(GameObjectNames.GetNumberBonusCards(time + 1));
            if (findSenderNumberCards != null)
            {
                DebugHelper.Log("sender: " + findSenderNumberCards.name);
                DebugHelper.Log("time + 1: " + (time + 1));
                findSenderNumberCards.GetComponent<TextMeshProUGUI>().text = playerSending.numberBonusCards.ToString();
            }

            lastCard.GetComponent<Animator>().enabled = true;
            lastCard.GetComponent<Animator>().SetBool("sending", true);
        }
        else
        {
            //DebugHelper.Log("Voc� n�o possui cartas!");
        }

    }

}
