using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Linq;
using TimeCrax.Core;

namespace TimeCrax.Managers
{
    /// <summary>
    /// Gerenciador de turnos do jogo.
    /// Controla a lógica de turnos, rounds e sincronização multiplayer.
    /// </summary>
    public class TurnManager : MonoBehaviourPunCallbacks
    {
        private static TurnManager _instance;
        public static TurnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TurnManager>();
                }
                return _instance;
            }
        }

        [Header("Referências")]
        [SerializeField] private GameObject gameInfo;
        [SerializeField] private Material plateNameMaterial;
        [SerializeField] private Material plateNameMaterial2;

        // Estado do turno
        private int round;
        private int roundCompare;
        private int time;
        private PlayerScript[] orderedPlayers;

        // Cache de referências
        private GameManager gameManager;

        // Propriedades públicas
        public int CurrentRound => round;
        public int CurrentTime => time;
        public PlayerScript[] OrderedPlayers => orderedPlayers;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        /// <summary>
        /// Inicializa o gerenciador de turnos para uma nova partida
        /// </summary>
        public void Initialize(int initialRound = 1, int initialTime = 0)
        {
            round = initialRound;
            roundCompare = initialRound;
            time = initialTime;
            orderedPlayers = null;

            RefreshPlateNamesCache();
        }

        /// <summary>
        /// Atualiza o cache de plateNames (delega para PlayerManager)
        /// </summary>
        public void RefreshPlateNamesCache()
        {
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.InvalidateCache();
            }
        }

        /// <summary>
        /// Configura a lista de jogadores ordenados
        /// </summary>
        public void SetupOrderedPlayers(PlayerScript[] players)
        {
            if (players == null || players.Length == 0) return;

            orderedPlayers = new PlayerScript[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;

                if (players[i].index >= 0 && players[i].index < orderedPlayers.Length)
                {
                    orderedPlayers[players[i].index] = players[i];
                }
            }

        }

        /// <summary>
        /// Remove um jogador da lista ordenada
        /// </summary>
        public void RemovePlayerFromOrder(int plateNameIndex)
        {
            if (orderedPlayers == null) return;

            for (int i = 0; i < orderedPlayers.Length; i++)
            {
                if (orderedPlayers[i]?.plateNameIndex == plateNameIndex)
                {
                    orderedPlayers[i] = null;
                }
            }
        }

        /// <summary>
        /// Verifica se o time atual é válido e avança se necessário
        /// </summary>
        public bool CheckTimeAndIndex()
        {
            if (orderedPlayers == null || orderedPlayers.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < orderedPlayers.Length; i++)
            {
                if (orderedPlayers[i] != null && orderedPlayers[i].index == time)
                {
                    return true;
                }
            }

            time++;

            if (time >= 4)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retorna o jogador atual do turno
        /// </summary>
        public PlayerScript GetCurrentTurnPlayer()
        {
            if (orderedPlayers == null || time < 0 || time >= orderedPlayers.Length)
            {
                return null;
            }
            return orderedPlayers[time];
        }

        /// <summary>
        /// Retorna o índice do último jogador válido
        /// </summary>
        public int GetLastPlayerIndex()
        {
            if (orderedPlayers == null) return 3;

            for (int i = orderedPlayers.Length - 1; i >= 0; i--)
            {
                if (orderedPlayers[i] != null)
                {
                    return orderedPlayers[i].index;
                }
            }

            return 3;
        }

        /// <summary>
        /// Avança para o próximo turno
        /// </summary>
        public void NextTurn()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                time++;
                photonView.RPC("RPC_SyncTurn", RpcTarget.All, time);
            }
        }

        /// <summary>
        /// Inicia um novo round
        /// </summary>
        public void NextRound()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                round++;
                time = 0;
                photonView.RPC("RPC_SyncTurnWithRound", RpcTarget.All, time, round);
            }
        }

        /// <summary>
        /// Verifica se é o último turno do round
        /// </summary>
        public bool IsLastTurnOfRound()
        {
            return time == GetLastPlayerIndex();
        }

        #region Material de PlateNames

        /// <summary>
        /// Altera o material do plateName do jogador atual
        /// </summary>
        public void UpdatePlateNameMaterial(int plateNameIndex)
        {
            photonView.RPC("RPC_ChangePlateNameMaterial", RpcTarget.All, plateNameIndex);
        }

        [PunRPC]
        public void RPC_ChangePlateNameMaterial(int plateNameIndex)
        {
            string plateNameText = GameObjectNames.GetPlateName(plateNameIndex + 1);

            GiveCards[] plateNames = null;
            if (PlayerManager.Instance != null)
            {
                plateNames = PlayerManager.Instance.GetCachedPlateNames();
            }

            if (plateNames == null || plateNames.Length == 0)
            {
                plateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);
            }

            foreach (GiveCards plateName in plateNames)
            {
                if (plateName == null) continue;
                if (plateName.name == plateNameText)
                {
                    plateName.GetComponent<MeshRenderer>().material = plateNameMaterial2;
                }
                else
                {
                    plateName.GetComponent<MeshRenderer>().material = plateNameMaterial;
                }
            }
        }

        #endregion

        #region Info Display

        /// <summary>
        /// Exibe informações do round/turno
        /// </summary>
        public void ShowRoundInfo()
        {
            if (gameInfo == null) return;

            Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
            gameInfo.SetActive(true);

            string currentPlayerName = "Player";
            var currentPlayer = GetCurrentTurnPlayer();
            if (currentPlayer != null)
            {
                currentPlayerName = currentPlayer.nickname;
            }

            if (round == roundCompare)
            {
                roundCompare++;

                foreach (var info in infos)
                {
                    if (info.gameObject.name == "TurnInfo")
                    {
                        info.GetComponentInChildren<TextMeshProUGUI>().text = currentPlayerName + "'s Turn";
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

        private void HideRoundInfo()
        {
            if (gameInfo == null) return;

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

        private void DisableGameInfo()
        {
            if (gameInfo != null)
            {
                gameInfo.SetActive(false);
            }

            // Notificar GameManager para continuar o turno
            if (gameManager != null)
            {
                gameManager.OnRoundInfoHidden();
            }
        }

        #endregion

        #region RPCs

        [PunRPC]
        public void RPC_SyncTurn(int syncedTime)
        {
            time = syncedTime;

            EnsureOrderedPlayersPopulated();

            if (gameManager != null)
            {
                gameManager.OnTurnSynced();
            }
        }

        [PunRPC]
        public void RPC_SyncTurnWithRound(int syncedTime, int syncedRound)
        {
            time = syncedTime;
            round = syncedRound;

            EnsureOrderedPlayersPopulated();

            if (gameManager != null)
            {
                gameManager.OnTurnSynced();
            }
        }

        private void EnsureOrderedPlayersPopulated()
        {
            if (orderedPlayers == null || orderedPlayers.Length == 0)
            {
                var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
                SetupOrderedPlayers(players);
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion
    }
}
