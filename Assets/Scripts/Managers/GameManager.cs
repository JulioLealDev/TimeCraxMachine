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
    [SerializeField] private EndMatch endMatchScreen;
    [SerializeField] private GameObject map;
    [SerializeField] private GameObject personsFrame;
    [SerializeField] private Animator newTimelineAnimator;
    [SerializeField] private TMP_Text personName01;
    [SerializeField] private TMP_Text personName02;
    [SerializeField] private TMP_Text personName03;
    [SerializeField] private TMP_Text personText01;
    [SerializeField] private TMP_Text personText02;
    [SerializeField] private TMP_Text personText03;

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
    [SerializeField] private LegacyThemesData legacyThemesData;
    [SerializeField] private TurnTimer turnTimer;
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private bool fullScreen = true;

    // Propriedades estáticas
    public static ThemeCard CurrentPersonsThemeCard { get; private set; }
    public static int CurrentPersonsSlotCount { get; private set; }
    public static List<TimeCrax.Themes.PersonEntry> ShuffledPersonEntries { get; private set; }
    public static bool IsInTurnTransition { get; set; } = false;
    public static bool IsMalfunctionPending { get; set; } = false;

    // Propriedades públicas
    public int randomId;
    public GameObject Hud => hud;
    public int CurrentRound => round;
    public int CurrentTime => time;
    public PlayerScript[] OrderedPlayers => orderedPlayers;

    // Estado do jogo
    private bool gameIsOn = false;
    private int round;
    private int roundCompare;
    private int time;
    private bool _pendingPlayerErrorAfterZoomOut = false;

    // Players e componentes
    private MachineComponent[] timeCraxComponents;
    private PlayerScript[] players;
    private PlayerScript[] orderedPlayers;
    private int[] playersList;
    private int initialPlayersNumber;
    private List<int> componentList = new List<int>();
    private List<Transform> componentsWithAnimator = new List<Transform>();

    // Cache de Find
    private GiveCards[] cachedPlateNames;
    private BonusCard[] cachedBonusCards;
    private bool needsCacheRefresh = true;

    // Cache de GetComponent
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

    // ─────────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        int targetHeight = Screen.width * 9 / 16;
        Screen.SetResolution(Screen.width, targetHeight, fullScreen);

        GameStateManager.TransitionTo(GamePhase.Menu);

        SessionData.GameStarted = false;
        gameConnection?.EnterServerAndLobby();
        gameCamera?.Initialize();
    }

    void Start()
    {
        gameCamera?.EnterMenu();
    }

    void Update()
    {
        if (gameIsOn && PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerList.Length != initialPlayersNumber)
            {
                initialPlayersNumber = PhotonNetwork.PlayerList.Length;

                if (players == null || players.Length == 0)
                {
                    return;
                }

                int plateNameIndex = -1;

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

                    if (!playerStillConnected)
                    {
                        plateNameIndex = player.plateNameIndex;
                        break;
                    }
                }

                if (plateNameIndex >= 0)
                {
                    photonView.RPC("RemovePlayersPlatenames", RpcTarget.All, plateNameIndex);
                }
                else
                {
                }
            }
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Initialization

    private void CacheComponents()
    {
        if (deckEvent != null)
        {
            cachedDeckEventMeshCollider = deckEvent.GetComponent<MeshCollider>();
            cachedDeckEventPhotonView   = deckEvent.GetComponent<PhotonView>();
        }
        if (deckBonus != null)
        {
            cachedDeckBonusMeshCollider = deckBonus.GetComponent<MeshCollider>();
            cachedDeckBonusPhotonView   = deckBonus.GetComponent<PhotonView>();
        }
        if (timeline != null)
        {
            cachedTimelineMeshCollider = timeline.GetComponent<MeshCollider>();
            cachedTimelinePhotonView   = timeline.GetComponent<PhotonView>();
        }
        if (endButton != null)
        {
            cachedEndButtonMeshCollider = endButton.GetComponent<MeshCollider>();
            cachedEndButtonPhotonView   = endButton.GetComponent<PhotonView>();
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
    }

    public void RefreshCache()
    {
        var pm = PlayerManager.Instance;
        if (pm != null)
        {
            pm.RefreshCache();
            players = pm.Players;
        }
        cachedPlateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);
        cachedBonusCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
        needsCacheRefresh = false;
    }

    private GiveCards[] GetCachedPlateNames()
    {
        if (needsCacheRefresh || cachedPlateNames == null)
            cachedPlateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);
        return cachedPlateNames;
    }

    private BonusCard[] GetCachedBonusCards()
    {
        if (needsCacheRefresh || cachedBonusCards == null)
            cachedBonusCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
        return cachedBonusCards;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Game Flow

    public void StartNewGame()
    {
        string theme = PhotonNetwork.CurrentRoom.CustomProperties["the"].ToString();

        if (rightCompartmentAnimator != null)
            rightCompartmentAnimator.SetBool("open", true);

        CacheComponents();

        gameIsOn = true;
        MatchStats.Reset();
        MatchStats.StartTimer();
        IsMalfunctionPending = false;
        _pendingPlayerErrorAfterZoomOut = false;
        componentsWithAnimator.Clear();
        timeCraxComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);

        Transform[] components = enviroment.GetComponentsInChildren<Transform>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].CompareTag("Component"))
            {
                var animator = components[i].GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = true;
                    componentsWithAnimator.Add(components[i]);
                }
            }
        }

        round = 1;
        roundCompare = 1;
        time = 0;

        PhotonNetwork.Instantiate("Player", new Vector3(7.224f, 1.01f, 0.83f), Quaternion.identity);
        playersList = new int[PhotonNetwork.PlayerList.Length];
        initialPlayersNumber = PhotonNetwork.PlayerList.Length;

        // Ativando animação de distanciamento da câmera
        if (cachedGameCameraAnimator != null)
            cachedGameCameraAnimator.SetBool("enterMatch", true);


        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        componentList.Clear();
        componentList.AddRange(numbers);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            playersList[i] = PhotonNetwork.PlayerList[i].ActorNumber;

        if (PhotonNetwork.IsMasterClient)
        {
            if (ThemeManager.Instance != null && ThemeManager.Instance.HasSelectedTheme)
            {
                var selectedTheme = ThemeManager.Instance.SelectedTheme;
                legacyThemesData.InitializeForTheme(selectedTheme);
                legacyThemesData.GetLegacyThemesDataFromTheme();
            }
            else
            {
                legacyThemesData.GetLegacyThemesData(theme);
            }

            this.DelayedCall(6f, StartGame);
        }
    }

    public void StartGame()
    {
        photonView.RPC("ShowHUD", RpcTarget.All);
    }

    [PunRPC]
    public void ShowHUD()
    {
        RefreshCache();

        if (ThermometerManager.Instance != null)
            ThermometerManager.Instance.Initialize();

        hud.SetActive(true);

        var outline = FindFirstObjectByType<OutlineAction>();
        if (outline != null)
            outline.MakeObjectsSelectable();

        SetNewTimelineColliders(true);

        var hudChildren = hud.GetComponentsInChildren<Transform>(true);
        var orderedPlayerList = PlayerManager.GetOrderedPlayerList();

        foreach (var t in hudChildren)
        {
            if (t.name == "FinishTurn" || t.name == "QuitGame")
            {
                var cg = t.gameObject.GetComponent<CanvasGroup>();
                if (cg != null) cg.LeanAlpha(1f, 2f);
            }
        }

        for (int i = 0; i < orderedPlayerList.Length; i++)
        {
            int playerNum = i + 1;
            string plateNameStr     = GameObjectNames.GetPlateName(playerNum);
            string plateNameTextStr = GameObjectNames.GetPlateNameText(playerNum);
            string bonusSymbolStr   = GameObjectNames.GetBonusCardSymbol(playerNum);
            string bonusCardTextStr = GameObjectNames.GetNumberBonusCards(playerNum);

            foreach (var t in hudChildren)
            {
                if (t.name == plateNameStr)
                {
                    t.gameObject.SetActive(true);
                    var mr = t.GetComponent<MeshRenderer>();
                    if (mr != null) mr.enabled = true;
                    var mc = t.GetComponent<MeshCollider>();
                    if (mc != null) mc.enabled = true;
                }
                else if (t.name == plateNameTextStr)
                {
                    var tmp = t.GetComponent<TextMeshPro>();
                    if (tmp != null)
                    {
                        string nick = orderedPlayerList[i].NickName;
                        tmp.text = string.IsNullOrEmpty(nick) ? $"Player0{playerNum}" : nick;
                        tmp.alpha = 1f;
                    }
                }
                else if (t.name == bonusSymbolStr)
                {
                    var sr = t.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.enabled = true;
                }
                else if (t.name == bonusCardTextStr)
                {
                    var tmp = t.GetComponent<TextMeshPro>();
                    if (tmp != null) tmp.text = "0";
                }
            }
        }

        this.DelayedCall(2f, FirstTurn);
    }

    public void FirstTurn()
    {
        var pm = PlayerManager.Instance;
        if (pm == null)
        {
            Debug.LogError("[GameManager] FirstTurn: PlayerManager.Instance é null.");
            return;
        }
        players = pm.Players;
        orderedPlayers = new PlayerScript[players.Length];
        GameStateManager.TransitionTo(GamePhase.IM_FirstTurn);

        for (int i = 0; i < players.Length; i++)
        {
            if      (players[i].index == 0) orderedPlayers[0] = players[i];
            else if (players[i].index == 1) orderedPlayers[1] = players[i];
            else if (players[i].index == 2) orderedPlayers[2] = players[i];
            else if (players[i].index == 3) orderedPlayers[3] = players[i];
        }

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("SyncTurn", RpcTarget.All, time);
    }

    public void Turn()
    {
        if (!gameIsOn)
        {
            return;
        }

        if (orderedPlayers == null || orderedPlayers.Length == 0)
        {
            return;
        }

        players = PlayerManager.Instance?.Players ?? players;

        bool checkTime = false;
        int maxIterations = 5;
        int iterations = 0;
        while (!checkTime && iterations < maxIterations)
        {
            checkTime = CheckTimeAndIndex(orderedPlayers);
            iterations++;
        }

        if (!checkTime)
        {
            Debug.LogError($"[GameManager] Turn(): nenhum jogador válido encontrado após {maxIterations} iterações (time={time}). Forçando novo round.");
            if (PhotonNetwork.IsMasterClient)
            {
                round++;
                photonView.RPC("SyncTurnWithRound", RpcTarget.All, 0, round);
            }
            return;
        }

        int numberOfPlayers = 4;
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

            deckEvent.TurnDeckEventSelectable(true);
            ShowRoundInfo();

        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                round++;
                photonView.RPC("SyncTurnWithRound", RpcTarget.All, 0, round);
            }
        }
    }

    public bool CheckTimeAndIndex(PlayerScript[] orderedPlayers)
    {
        if (orderedPlayers == null || orderedPlayers.Length == 0)
            return true;

        for (int i = 0; i < orderedPlayers.Length; i++)
        {
            if (orderedPlayers[i] != null && orderedPlayers[i].index == time)
                return true;
        }

        time++;

        if (time >= 4)
            return true;

        return false;
    }

    public void ShowRoundInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        gameInfo.gameObject.SetActive(true);

        string currentPlayerName = "Player";
        if (orderedPlayers != null && time >= 0 && time < orderedPlayers.Length && orderedPlayers[time] != null)
            currentPlayerName = orderedPlayers[time].nickname;

        if (round == roundCompare)
        {
            roundCompare++;
            foreach (var info in infos)
            {
                if (info.gameObject.name == "TurnInfo")
                    info.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = currentPlayerName + "'s Turn";
                if (info.gameObject.name == "RoundInfo")
                    info.GetComponentInChildren<TextMeshProUGUI>().text = "Starting Round " + round;
                if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
            }
        }
        else
        {
            foreach (var info in infos)
            {
                if (info.gameObject.name == "TurnInfo")
                    info.GetComponentInChildren<TextMeshProUGUI>().text = currentPlayerName + "'s Turn";
                if (info.gameObject.name == "TurnInfoBackground")
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
            }
        }

        this.DelayedCall(1.5f, HideRoundInfo);
    }

    public void HideRoundInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "TurnInfoBackground" || info.gameObject.name == "RoundInfoBackground")
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
        StartTurn();
    }

    public void StartTurn()
    {
        GameStateManager.TransitionTo(GamePhase.IM_Turn);
        IsInTurnTransition = false;
        InputBlocker.Unblock();
        BonusCardManager.Instance?.ResetSecondChance();

        SetNewTimelineNonSlotColliders(true);

        var eventSlot = FindFirstObjectByType<EventSlot>();
        if (eventSlot != null)
            eventSlot.SetUpSlots(false, "Undestructable");

        if (PhotonNetwork.IsMasterClient)
            StartTurnTimerRPC();

        players = PlayerManager.Instance?.Players ?? players;

        PlayerScript currentOrderedPlayer = null;
        if (orderedPlayers != null && time >= 0 && time < orderedPlayers.Length)
            currentOrderedPlayer = orderedPlayers[time];

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (currentOrderedPlayer != null && currentOrderedPlayer.actorNumber == PhotonNetwork.PlayerList[i].ActorNumber)
            {
                if (cachedDeckEventPhotonView  != null && cachedDeckEventPhotonView.ViewID  > 0) cachedDeckEventPhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (cachedDeckBonusPhotonView  != null && cachedDeckBonusPhotonView.ViewID  > 0) cachedDeckBonusPhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (cachedTimelinePhotonView   != null && cachedTimelinePhotonView.ViewID   > 0) cachedTimelinePhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (photonView                 != null && photonView.ViewID                 > 0) photonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                if (cachedEndButtonPhotonView  != null && cachedEndButtonPhotonView.ViewID  > 0) cachedEndButtonPhotonView.TransferOwnership(PhotonNetwork.PlayerList[i]);

                var plateNames = GetCachedPlateNames();
                foreach (GiveCards plateName in plateNames)
                {
                    if (plateName == null) continue;
                    var pv = plateName.GetComponent<PhotonView>();
                    if (pv != null && pv.ViewID > 0)
                        pv.TransferOwnership(PhotonNetwork.PlayerList[i]);
                }

                foreach (var component in timeCraxComponents)
                {
                    if (component == null) continue;
                    var pv = component.GetComponent<PhotonView>();
                    if (pv != null && pv.ViewID > 0)
                        pv.TransferOwnership(PhotonNetwork.PlayerList[i]);
                }
            }
        }

        PlayerScript localPlayer = null;
        foreach (var player in players)
        {
            if (player.photonView.IsMine) localPlayer = player;
        }

        bool isMyTurn = localPlayer != null && localPlayer.index == time;

        if (isMyTurn)
        {
            foreach (var timeCraxComponent in timeCraxComponents)
            {
                if (timeCraxComponent.malfunctions == 1)
                {
                    timeCraxComponent.tag = "Selectable";
                }

            }

            if (cachedEndButtonMeshCollider  != null) cachedEndButtonMeshCollider.enabled  = true;
            if (cachedQuitButtonMeshCollider != null) cachedQuitButtonMeshCollider.enabled = true;
            if (cachedTimelineMeshCollider   != null) cachedTimelineMeshCollider.enabled   = true;
            if (cachedDeckEventMeshCollider  != null) cachedDeckEventMeshCollider.enabled  = true;
            if (cachedDeckBonusMeshCollider  != null) cachedDeckBonusMeshCollider.enabled  = false;
            if (deckBonus != null) deckBonus.tag = "Untagged";
            deckEvent.tag = "Selectable";
            timeline.tag  = "Selectable";

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                var plate = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
                if (plate != null)
                {
                    plate.GetComponent<MeshCollider>().enabled = true;
                    plate.tag = "Selectable";
                }
            }

            var ownPlate = GameObject.Find(GameObjectNames.GetPlateName(time + 1));
            if (ownPlate != null)
                ownPlate.GetComponent<MeshCollider>().enabled = false;
        }
        else
        {
            if (localPlayer != null) localPlayer.SetYourTurn(false);

            if (cachedEndButtonMeshCollider  != null) cachedEndButtonMeshCollider.enabled  = false;
            if (cachedTimelineMeshCollider   != null) cachedTimelineMeshCollider.enabled   = false;
            if (cachedDeckEventMeshCollider  != null) cachedDeckEventMeshCollider.enabled  = false;
            if (cachedDeckBonusMeshCollider  != null) cachedDeckBonusMeshCollider.enabled  = false;

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                var plate = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
                if (plate != null)
                    plate.GetComponent<MeshCollider>().enabled = false;
            }
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Turn Synchronization

    [PunRPC]
    public void SyncTurn(int syncedTime)
    {
        SyncTurnInternal(syncedTime, -1);
    }

    [PunRPC]
    public void SyncTurnWithRound(int syncedTime, int syncedRound)
    {
        SyncTurnInternal(syncedTime, syncedRound);
    }

    private void SyncTurnInternal(int syncedTime, int syncedRound)
    {
        if (!gameIsOn)
        {
            return;
        }

        time = syncedTime;
        if (syncedRound >= 0) round = syncedRound;

        if (orderedPlayers == null || orderedPlayers.Length == 0)
        {
            players = PlayerManager.Instance?.Players ?? players;
            if (players == null || players.Length == 0) return;

            orderedPlayers = new PlayerScript[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if      (players[i].index == 0) orderedPlayers[0] = players[i];
                else if (players[i].index == 1) orderedPlayers[1] = players[i];
                else if (players[i].index == 2) orderedPlayers[2] = players[i];
                else if (players[i].index == 3) orderedPlayers[3] = players[i];
            }
        }

        Turn();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Turn End

    public void EndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            StopTurnTimerRPC();

        this.DelayedCall(0f, WaitForFinishTurn);
    }

    public void WaitForFinishTurn()
    {
        if (!GameStateManager.Is(GamePhase.GameOver))
        {
            if (PhotonNetwork.IsMasterClient && ThermometerManager.Instance != null)
            {
                float waitTime = ThermometerManager.Instance.GetErrorProcessingTime();
                ThermometerManager.Instance.OnPlayerError();

                this.DelayedCall(waitTime, () =>
                {
                    if (!GameStateManager.Is(GamePhase.GameOver))
                        photonView.RPC("FinishTurn", RpcTarget.All);
                });
            }
            else
            {
                photonView.RPC("FinishTurn", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void FinishTurn()
    {

        if (leftCompartmentAnimator != null) leftCompartmentAnimator.SetBool("open", false);
        if (cachedDeckBonusMeshCollider != null) cachedDeckBonusMeshCollider.enabled = false;
        if (deckBonus != null) deckBonus.tag = "Untagged";
        if (deckEvent != null) deckEvent.tag = "Disabled";
        if (timeline  != null) timeline.tag  = "Disabled";

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected && photonView != null)
        {
            time++;
            photonView.RPC("SyncTurn", RpcTarget.All, time);
        }

        SetUpComponents();
    }

    public void AutoEndTurn()
    {

        if (!gameIsOn) return;
        if (GameStateManager.Is(GamePhase.GameOver)) return;

        photonView.RPC("RPC_HandleTimeoutCleanup", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_HandleTimeoutCleanup()
    {

        IsInTurnTransition = true;
        InputBlocker.Block();

        EventCard drewCard = null;
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in eventCards)
        {
            if (card != null && card.CompareTag("Drew"))
            {
                drewCard = card;
                break;
            }
        }

        if (drewCard != null)
        {
            int cardSlotCount = drewCard.slotCount;
            drewCard.ResetStatusCard();
            drewCard.tag = "Untagged";

            if (PhotonNetwork.IsMasterClient && deckEvent != null)
                deckEvent.AddCardBack(cardSlotCount);
        }

        bool wasInZoomMode = gameCamera != null && gameCamera.IsInZoomMode();

        if (gameCamera != null)
        {
            if (wasInZoomMode)
                gameCamera.DistanceTimeline();
            else
                gameCamera.ForceResetToInitialState();
        }

        var tl = FindFirstObjectByType<Timeline>();
        if (tl != null) tl.ActiveTimeline(false);

        var eventSlot = FindFirstObjectByType<EventSlot>();
        if (eventSlot != null) eventSlot.SetUpSlots(false, "Undestructable");

        if (PhotonNetwork.IsMasterClient)
        {
            float delay = wasInZoomMode ? 1.5f : 0.5f;
            this.DelayedCall(delay, TimeoutMalfunction);
        }
    }

    private void TimeoutMalfunction()
    {
        if (!gameIsOn || GameStateManager.Is(GamePhase.GameOver)) return;

        float processingTime = 0f;
        if (ThermometerManager.Instance != null)
        {
            processingTime = ThermometerManager.Instance.GetErrorProcessingTime();
            ThermometerManager.Instance.OnPlayerError();
        }

        float delay = processingTime > 0 ? processingTime + 0.5f : 0.5f;
        this.DelayedCall(delay, FinishTurnAfterTimeout);
    }

    private void FinishTurnAfterTimeout()
    {
        if (!gameIsOn || GameStateManager.Is(GamePhase.GameOver)) return;
        photonView.RPC("FinishTurn", RpcTarget.All);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Turn Timer RPCs

    public void SyncTurnTimer(float time)
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_SyncTurnTimer", RpcTarget.Others, time);
    }

    [PunRPC]
    public void RPC_SyncTurnTimer(float time)
    {
        if (turnTimer != null) turnTimer.SyncTime(time);
    }

    public void StopTurnTimerRPC()
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_StopTurnTimer", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_StopTurnTimer()
    {
        if (turnTimer != null) turnTimer.StopTimerLocal();
    }

    public void StartTurnTimerRPC()
    {
        if (PhotonNetwork.IsMasterClient && turnTimer != null)
            photonView.RPC("RPC_StartTurnTimer", RpcTarget.All, turnTimer.TimeLimit);
    }

    [PunRPC]
    public void RPC_StartTurnTimer(float time)
    {
        if (turnTimer != null) turnTimer.StartTimerLocal(time);
    }

    public void ApplyBatteryMalfunctionRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_ApplyBatteryMalfunction", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_ApplyBatteryMalfunction()
    {
        if (turnTimer != null) turnTimer.ApplyBatteryMalfunction();
    }

    public void RestoreBatteryEffectRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_RestoreBatteryEffect", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_RestoreBatteryEffect()
    {
        if (turnTimer != null) turnTimer.RestoreBatteryEffect();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Persons Mini-Game

    public void ActivateRandomMapObject(int slotCount)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"[GameManager] ActivateRandomMapObject — slotCount={slotCount}");

        var themeCard = FindPersonsThemeCard(slotCount);
        if (themeCard == null)
        {
            Debug.LogWarning($"[GameManager] ThemeCard não encontrado para slotCount={slotCount}. Mini-game cancelado.");
            return;
        }

        bool hasMap     = themeCard.map != null;
        bool hasPersons = themeCard.persons?.entries != null;
        Debug.Log($"[GameManager] ThemeCard='{themeCard.title}' hasMap={hasMap} hasPersons={hasPersons}");

        int choice = DebugGameConfig.PickChallengeType(hasMap, hasPersons);

        Debug.Log($"[GameManager] choice={choice} ({(choice == 1 ? "Map" : "Persons")}) → enviando RPC para todos");
        photonView.RPC("RPC_ActivateRandomMapObject", RpcTarget.All, slotCount, choice);
    }

    [PunRPC]
    private void RPC_ActivateRandomMapObject(int slotCount, int choice)
    {
        Debug.Log($"[GameManager] RPC_ActivateRandomMapObject — slotCount={slotCount}, choice={choice} ({(choice == 1 ? "Map" : "Persons")})");
        CurrentPersonsSlotCount = slotCount;

        if (newTimelineAnimator != null)
            newTimelineAnimator.SetBool("Open", true);

        if (choice == 1)
        {
            Debug.Log($"[GameManager] Ativando Map. map={map != null}");
            if (map != null)
            {
                map.SetActive(true);
                this.DelayedCall(2.5f, () => EnableMeshColliders(map));
            }
            else Debug.LogWarning("[GameManager] Referência 'map' é null no Inspector!");
            GameStateManager.TransitionTo(GamePhase.IM_MapChallenge);
            var mapThemeCard = FindPersonsThemeCard(slotCount);
            ChallengeQuestionUI.Instance?.Show(mapThemeCard?.map?.question);
            return;
        }

        CurrentPersonsThemeCard = FindPersonsThemeCard(slotCount);
        Debug.Log($"[GameManager] Ativando PersonsFrame. personsFrame={personsFrame != null}, themeCard={CurrentPersonsThemeCard?.title}");
        if (personsFrame != null)
        {
            personsFrame.SetActive(true);
            this.DelayedCall(2.5f, () => EnableMeshColliders(personsFrame));
        }
        else Debug.LogWarning("[GameManager] Referência 'personsFrame' é null no Inspector!");
        GameStateManager.TransitionTo(GamePhase.IM_PersonsChallenge);
        ApplyPersonsText(CurrentPersonsThemeCard);
        ChallengeQuestionUI.Instance?.Show(CurrentPersonsThemeCard?.persons?.question);

        this.DelayedCall(2.5f, ActivatePersonsSelectable);
    }

    private void EnableMeshColliders(GameObject root)
    {
        foreach (var col in root.GetComponentsInChildren<MeshCollider>(true))
            col.enabled = true;
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

    public void CloseNewTimeline()
    {
        if (newTimelineAnimator != null)
            newTimelineAnimator.SetBool("Open", false);
    }

    [PunRPC]
    public void RPC_HandlePersonsWrong()
    {
        HandlePersonsWrong();
    }

    public void HandlePersonsWrong()
    {
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
                this.DelayedCall(4f, () =>
                {
                    deckEvent.AddCardBack(slotCount);
                });
                break;
            }
        }

        RegisterWrongAnswer();
    }

    public void PersonsZoomOut()
    {
        if (PhotonNetwork.IsMasterClient && !_pendingPlayerErrorAfterZoomOut)
            OpenLeftCompartmentAfterZoomOut();
        gameCamera?.DistanceTimeline();
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

    private void ApplyPersonsText(ThemeCard themeCard)
    {
        var entries = themeCard?.persons?.entries;
        if (entries == null) return;

        TMP_Text[] names = { personName01, personName02, personName03 };
        TMP_Text[] texts = { personText01, personText02, personText03 };

        ShuffledPersonEntries = entries.OrderBy(_ => UnityEngine.Random.value).ToList();

        for (int i = 0; i < 3 && i < texts.Length; i++)
        {
            if (names[i] != null) names[i].text = string.Empty;
            if (texts[i] != null) texts[i].text = i < ShuffledPersonEntries.Count ? ShuffledPersonEntries[i].description : string.Empty;
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Map Mini-Game

    [PunRPC]
    public void RPC_HandleMapWrong(int slotCount)
    {
        HandleMapWrong(slotCount);
    }

    public void HandleMapWrong(int slotCount)
    {
        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            if (slot.SlotNumber == slotCount)
            {
                slot.gameObject.tag = "Untagged";
                break;
            }
        }

        var cards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in cards)
        {
            if (card.slotCount == slotCount)
            {
                card.GetComponent<Animator>().SetBool("wrongSlot", true);
                int captured = slotCount;
                this.DelayedCall(4f, () =>
                {
                    deckEvent.AddCardBack(captured);
                });
                break;
            }
        }

        RegisterWrongAnswer();
    }

    private void RegisterWrongAnswer()
    {
        if (!PhotonNetwork.IsMasterClient || ThermometerManager.Instance == null) return;
        if (ThermometerManager.Instance.WillNextErrorCauseMalfunction())
            IsMalfunctionPending = true;
        _pendingPlayerErrorAfterZoomOut = true;
    }

    public void CheckWinAfterMiniGame()
    {
        var anySlot = FindFirstObjectByType<EventSlot>();
        anySlot?.CheckIfWin();
    }

    public void MapZoomOut()
    {
        if (PhotonNetwork.IsMasterClient && !_pendingPlayerErrorAfterZoomOut)
            OpenLeftCompartmentAfterZoomOut();
        gameCamera?.DistanceTimeline();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Left Compartment

    private void OpenLeftCompartmentAfterZoomOut()
    {
        if (!GameStateManager.Is(GamePhase.Victory))
        {
            GameStateManager.TransitionTo(GamePhase.IM_UnlockBonusDeck);

            if (PhotonNetwork.IsMasterClient)
                photonView.RPC("RPC_OpenLeftCompartment", RpcTarget.All);
        }

    }

    [PunRPC]
    public void RPC_OpenLeftCompartment()
    {

        if (leftCompartmentAnimator != null)
            leftCompartmentAnimator.SetBool("open", true);

        foreach (var player in players)
        {
            if (player != null && player.photonView.IsMine && player.GetYourTurn())
            {
                //if (cachedDeckBonusMeshCollider != null)
                //    cachedDeckBonusMeshCollider.enabled = true;

                if (deckBonus != null)
                    deckBonus.tag = player.GetNumberOfBonusCards() < 5 ? "Selectable" : "Disabled";

                break;
            }
        }
    }

    [PunRPC]
    public void RPC_SetSecondChanceActive(bool active)
    {
        BonusCardManager.Instance?.SetSecondChanceState(active);
    }

    public void CloseLeftCompartment()
    {
        if (leftCompartmentAnimator != null) leftCompartmentAnimator.SetBool("open", false);
        if (cachedDeckBonusMeshCollider != null) cachedDeckBonusMeshCollider.enabled = false;
        if (deckBonus != null) deckBonus.tag = "Disabled";
    }

    [PunRPC]
    public void RPC_CloseLeftCompartment()
    {
        if (leftCompartmentAnimator != null) leftCompartmentAnimator.SetBool("open", false);
        if (cachedDeckBonusMeshCollider != null) cachedDeckBonusMeshCollider.enabled = false;
        if (deckBonus != null) deckBonus.tag = "Disabled";
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Component Malfunction

    public void RandomComponentNumber()
    {
        List<int> eligibleComponents = new List<int>();

        if (timeCraxComponents != null)
        {
            foreach (var component in timeCraxComponents)
            {
                if (component != null && component.malfunctions < 2)
                    eligibleComponents.Add(component.componentId);
            }
        }

        if (eligibleComponents.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, eligibleComponents.Count);
        randomId = eligibleComponents[randomIndex];

        InputBlocker.Block();
        photonView.RPC("ComponentRandom", RpcTarget.All, randomId);
    }

    [PunRPC]
    public void ComponentRandom(int id)
    {
        randomId = id;
        StartCoroutine(Roulettecomponent());
    }

    private IEnumerator Roulettecomponent()
    {
        int randomIndex = 0;
        int cond = 0;
        float interval = 0.3f;
        int componentCount = timeCraxComponents != null ? timeCraxComponents.Length : 0;

        if (componentCount == 0)
        {
            Debug.LogError("[GameManager] Roulettecomponent(): sem componentes disponíveis.");
            AddMalfunctionInComponent();
            yield break;
        }

        while (cond < 15)
        {
            int index = UnityEngine.Random.Range(0, componentCount);
            while (index == randomIndex && componentCount > 1)
                index = UnityEngine.Random.Range(0, componentCount);

            randomIndex = index;
            var rouletteComp = timeCraxComponents[index];
            if (rouletteComp != null)
            {
                var outline = rouletteComp.GetComponent<OutlineComponent>();
                if (outline != null) outline.enabled = true;
                soundEffects?.PlayRouletteSound();
            }
            yield return new WaitForSeconds(interval);
            if (rouletteComp != null)
            {
                var outline = rouletteComp.GetComponent<OutlineComponent>();
                if (outline != null) outline.enabled = false;
            }

            cond++;
            interval -= 0.015f;
        }

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
                soundEffects?.PlayRouletteSound();
                yield return new WaitForSeconds(interval);
                outline.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] Roulettecomponent(): componente id={randomId} não encontrado. Malfunction aplicado sem animação.");
        }

        AddMalfunctionInComponent();
    }

    public void AddMalfunctionInComponent()
    {
        // Executado em todos os clientes: desbloqueio de input, câmera e transição de estado local
        if (!IsInTurnTransition) InputBlocker.Unblock();
        gameCamera?.ActivateTimelineAfterMalfunction();
        GameStateManager.TransitionTo(GamePhase.IM_Turn);

        if (!PhotonNetwork.IsMasterClient) return;

        // Apenas o MasterClient envia o RPC para aplicar o malfunction no componente sorteado
        var malfPlayer = PlayerManager.Instance?.GetCurrentTurnPlayer();
        if (malfPlayer != null)
            MatchStats.AddMalfunction(malfPlayer.actorNumber, malfPlayer.nickname);

        foreach (var component in timeCraxComponents)
        {
            if (component != null && component.componentId == randomId)
            {
                component.photonView.RPC("RPC_AddMalfunction", RpcTarget.All);
                break;
            }
        }

        if (ThermometerManager.Instance != null)
            ThermometerManager.Instance.ResetTemperatureToFirstLevel();
    }

    [PunRPC]
    public void RPC_TrackMapChallenge(bool isCorrect, int actorNumber, string nickname)
    {
        if (isCorrect) MatchStats.AddChallengeCorrect(actorNumber, nickname);
        else MatchStats.AddMapError(actorNumber, nickname);
    }

    [PunRPC]
    public void RPC_TrackPersonsChallenge(bool isCorrect, int actorNumber, string nickname)
    {
        if (isCorrect) MatchStats.AddChallengeCorrect(actorNumber, nickname);
        else MatchStats.AddPersonsError(actorNumber, nickname);
    }

    [PunRPC]
    public void RPC_TrackBonusCardUsed(int actorNumber, string nickname)
    {
        MatchStats.AddBonusCardUsed(actorNumber, nickname);
    }

    public void ProcessPendingPlayerError()
    {
        if (!_pendingPlayerErrorAfterZoomOut) return;
        _pendingPlayerErrorAfterZoomOut = false;
        if (PhotonNetwork.IsMasterClient && ThermometerManager.Instance != null)
            ThermometerManager.Instance.OnPlayerError();
    }

    public void CheckGameOverCondition()
    {
        if (timeCraxComponents == null) return;

        int criticalComponents = 0;
        foreach (var component in timeCraxComponents)
        {
            if (component != null && component.malfunctions >= 2)
                criticalComponents++;
        }

        if (criticalComponents >= 2)
        {
            MatchStats.StopTimer();
            GameStateManager.TransitionTo(GamePhase.GameOver);
            this.DelayedCall(3f, TriggerGameOver);
        }
    }

    private void TriggerGameOver()
    {
        SetNewTimelineColliders(false);
        DeactivateAll();
        ResetAllComponents();
        ResetAllPlatenames();

        var bgMusic = FindFirstObjectByType<BackgroundMusic>();
        if (bgMusic != null) bgMusic.PlayGameOverSound();

        endMatchScreen.UpdateTitle();
        endMatchScreen.transform.GetChild(0).gameObject.SetActive(true);

        if (hud != null) hud.SetActive(false);
        if (rightCompartmentAnimator != null) rightCompartmentAnimator.SetBool("open", false);

        InputBlocker.Unblock();
    }

    public void SetUpComponents()
    {
        foreach (var player in players)
        {
            foreach (var component in timeCraxComponents)
            {
                if (component.malfunctions == 1)
                    component.GetComponent<MeshCollider>().enabled = player.GetYourTurn();
            }
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Player Management

    [PunRPC]
    public void UpdatePlayersIndex()
    {
        players = PlayerManager.Instance?.Players ?? players;
        foreach (var player in players)
            player.UpdateIndex();
    }

    [PunRPC]
    public void RemovePlayersPlatenames(int index)
    {
        if (orderedPlayers == null) return;

        for (int i = 0; i < orderedPlayers.Length; i++)
        {
            if (orderedPlayers[i]?.plateNameIndex == index)
                orderedPlayers[i] = null;
        }

        PlayerManager.Instance?.ResetPlayerUIElements(index + 1);
    }

    [PunRPC]
    public void ChangePlateNameMaterial(int plateNameIndex)
    {
        string plateNameText = GameObjectNames.GetPlateName(plateNameIndex + 1);
        var plateNames = GetCachedPlateNames();

        foreach (GiveCards plateName in plateNames)
        {
            if (plateName == null) continue;
            plateName.GetComponent<MeshRenderer>().material =
                plateName.name == plateNameText ? plateNameMaterial2 : plateNameMaterial;
        }
    }

    public void ChangeBonusCardsView(PlayerScript player)
    {
        PlayerManager.Instance?.ChangeBonusCardsView(player);
    }

    public void GiveCard(int numberPlayer)
    {
        photonView.RPC("GiveBonusCard", RpcTarget.All, numberPlayer);
    }

    [PunRPC]
    public void GiveBonusCard(int numberPlayer)
    {
        PlayerScript playerSending   = null;
        PlayerScript playerReceiving = null;

        if (players == null || players.Length == 0)
            players = PlayerManager.Instance?.Players ?? players;

        foreach (var player in players)
        {
            if (player == null) continue;
            if (player.GetYourTurn())          playerSending   = player;
            else if (player.index == numberPlayer - 1) playerReceiving = player;
        }

        if (playerSending == null || playerReceiving == null) return;

        if (playerSending.GetNumberOfBonusCards() > 0 && playerReceiving.GetNumberOfBonusCards() < 5)
        {
            BlockActions();

            needsCacheRefresh = true;
            var bonusCards = GetCachedBonusCards();
            var playerCards = new List<BonusCard>();

            foreach (var bonusCard in bonusCards)
            {
                if (bonusCard != null && bonusCard.photonView.OwnerActorNr == playerSending.photonView.OwnerActorNr)
                    playerCards.Add(bonusCard);
            }

            if (playerCards.Count == 0) return;

            var orderedList  = playerCards.OrderByDescending(x => x.index).ToList();
            BonusCard lastCard = orderedList[0];

            var orderedPlayerList = PlayerManager.GetOrderedPlayerList();
            if (playerReceiving.index >= 0 && playerReceiving.index < orderedPlayerList.Length)
            {
                if (lastCard.photonView != null && lastCard.photonView.ViewID > 0)
                    lastCard.photonView.TransferOwnership(orderedPlayerList[playerReceiving.index]);
            }

            playerReceiving.numberBonusCards++;
            var findReceiverNumberCards = GameObject.Find(GameObjectNames.GetNumberBonusCards(numberPlayer));
            if (findReceiverNumberCards != null)
                findReceiverNumberCards.GetComponent<TextMeshPro>().text = playerReceiving.numberBonusCards.ToString();

            playerSending.numberBonusCards--;
            var findSenderNumberCards = GameObject.Find(GameObjectNames.GetNumberBonusCards(time + 1));
            if (findSenderNumberCards != null)
                findSenderNumberCards.GetComponent<TextMeshPro>().text = playerSending.numberBonusCards.ToString();

            lastCard.GetComponent<Animator>().enabled = true;
            lastCard.GetComponent<Animator>().SetBool("sending", true);
        }
    }

    public void CheckQuitGamePlayer()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null) return;

        players = PlayerManager.Instance?.Players ?? players;
        if (players == null || players.Length == 0) return;

        foreach (var player in players)
        {
            if (player == null || player.photonView == null) continue;

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.photonView.OwnerActorNr && !GameStateManager.Is(GamePhase.GameOver))
            {
                if (PhotonNetwork.IsConnected && photonView != null)
                    photonView.RPC("ShowLeftPlayerInfo", RpcTarget.Others, player.nickname);

                if (player.GetYourTurn() && PhotonNetwork.PlayerList.Length > 1 && PhotonNetwork.IsConnected && photonView != null)
                    photonView.RPC("FinishTurn", RpcTarget.Others);

                break;
            }
        }
    }

    [PunRPC]
    public void ShowLeftPlayerInfo(string nickname)
    {
        if (gameInfo == null || playerLeftBackground == null) return;

        gameInfo.gameObject.SetActive(true);

        var tmpText = playerLeftBackground.GetComponentInChildren<TMP_Text>();
        if (tmpText != null) tmpText.text = nickname + " left the game";

        var canvasGroup = playerLeftBackground.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.LeanAlpha(1f, 0.5f);

        this.DelayedCall(1.5f, HideLeftPlayerInfo);
    }

    public void HideLeftPlayerInfo()
    {
        if (playerLeftBackground == null) return;

        var canvasGroup = playerLeftBackground.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.LeanAlpha(0f, 0.5f);

        this.DelayedCall(0.5f, DisableOnlyGameInfo);
    }

    public void DisableOnlyGameInfo()
    {
        if (gameInfo != null) gameInfo.gameObject.SetActive(false);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Helpers & Utilities

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
                if (enabled && child.GetComponent<TimelineColliderArea>() != null)
                    Debug.Log($"[GameManager] SetNewTimelineColliders ativou TimelineColliderArea — IsMalfunctionPending={IsMalfunctionPending}");
            }
        }
    }

    public void SetNewTimelineNonSlotColliders(bool enabled)
    {
        if (suitTop == null) return;
        Transform newTimeline = suitTop.transform.Find("NewTimeline");
        if (newTimeline == null) return;

        foreach (Transform child in newTimeline.GetComponentsInChildren<Transform>())
        {
            if (child == newTimeline) continue;
            if (child.GetComponentInParent<EventSlot>(true) != null) continue;
            // Não reativar TimelineColliderArea enquanto malfunction estiver pendente
            if (enabled && IsMalfunctionPending && child.GetComponent<TimelineColliderArea>() != null) continue;

            var col = child.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = enabled;
                if (enabled && child.GetComponent<TimelineColliderArea>() != null)
                    Debug.Log($"[GameManager] SetNewTimelineNonSlotColliders ativou TimelineColliderArea — IsMalfunctionPending={IsMalfunctionPending}");
            }
        }
    }

    public void BlockActions()
    {
        deckBonus.tag = "Disabled";
        deckEvent.tag = "Disabled";

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var findObject = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
            if (findObject != null) findObject.tag = "Disabled";
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
        if (turnTimer != null) turnTimer.StopTimerLocal();

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var findObject = GameObject.Find(GameObjectNames.GetPlateName(i + 1));
            if (findObject != null)
                findObject.GetComponent<MeshCollider>().enabled = false;
        }

        var suitComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);
        foreach (var suitComponent in suitComponents)
        {
            if (suitComponent != null && suitComponent.malfunctions > 0)
                suitComponent.GetComponent<MeshCollider>().enabled = false;
        }

        if (cachedDeckEventMeshCollider  != null) cachedDeckEventMeshCollider.enabled  = false;
        if (cachedDeckBonusMeshCollider  != null) cachedDeckBonusMeshCollider.enabled  = false;
        if (cachedTimelineMeshCollider   != null) cachedTimelineMeshCollider.enabled   = false;
        if (cachedEndButtonMeshCollider  != null) cachedEndButtonMeshCollider.enabled  = false;
        if (cachedQuitButtonMeshCollider != null) cachedQuitButtonMeshCollider.enabled = false;
    }

    public void ActivateEnd()
    {
        if (GameStateManager.Is(GamePhase.Victory) || GameStateManager.Is(GamePhase.GameOver)) return;
        bool shouldEnable = false;
        foreach (var player in players)
        {
            if (player.index == time && player.photonView.IsMine)
            {
                shouldEnable = true;
                break;
            }
        }

        if (cachedEndButtonMeshCollider != null)
            cachedEndButtonMeshCollider.enabled = shouldEnable;
    }

    public void ActivateFinishButton(bool activate)
    {
        if (cachedEndButtonMeshCollider != null)
            cachedEndButtonMeshCollider.enabled = activate;
    }

    public void ResetAllPlatenames()
    {
        PlayerManager.Instance?.ResetAllPlatenames();
    }

    public void ResetAllComponents()
    {
        if (ThermometerManager.Instance != null)
            ThermometerManager.Instance.ResetThermometer();

        if (timeCraxComponents != null)
        {
            foreach (var component in timeCraxComponents)
            {
                if (component != null) component.ResetComponent();
            }
        }

        if (componentsWithAnimator != null)
        {
            foreach (var component in componentsWithAnimator)
            {
                if (component == null) continue;
                var animator = component.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetBool("malfunction", false);
                    animator.enabled = false;
                }
                component.tag = "Component";

                foreach (var effect in component.GetComponentsInChildren<ParticleSystem>(true))
                    effect.gameObject.SetActive(false);
            }
        }
    }

    public void BackToMenu()
    {
        gameIsOn = false;

        if (turnTimer != null) turnTimer.StopTimerLocal();

        CheckQuitGamePlayer();
        this.DelayedCall(0.7f, SetUpBackToMenu);
    }

    public void SetUpBackToMenu()
    {
        if (backgroundMusic != null) backgroundMusic.PlayMenuSound();

        gameIsOn = false;
        if (rightCompartmentAnimator != null) rightCompartmentAnimator.SetBool("open", false);
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

    public bool IsMyTurn()
    {
        var playerList = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in playerList)
        {
            if (player != null && player.photonView != null && player.photonView.IsMine && player.GetYourTurn())
                return true;
        }
        return false;
    }

    public bool CheckIfCardWasDrew()
    {
        var eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);
        foreach (var card in eventCards)
        {
            if (card.CompareTag("Drew")) return true;
        }
        return false;
    }

    public void GetRandomEventCards(string theme)
    {
        legacyThemesData.GetLegacyThemesData(theme);
    }

    private static readonly HashSet<object> _clickProcessing = new HashSet<object>();

    public static bool TryBeginClick(object caller)
    {
        if (_clickProcessing.Contains(caller)) return false;
        _clickProcessing.Add(caller);
        return true;
    }

    public static void ResetClick(object caller)
    {
        _clickProcessing.Remove(caller);
    }

    public static bool IsClickProcessing(object caller) => _clickProcessing.Contains(caller);

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Callbacks

    public void OnTurnSynced()
    {
        Turn();
    }

    public void OnRoundInfoHidden()
    {
        StartTurn();
    }

    #endregion
}
