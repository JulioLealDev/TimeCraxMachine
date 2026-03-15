using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using TimeCrax.Core;

namespace TimeCrax.Managers
{
    /// <summary>
    /// Gerenciador de jogadores.
    /// Controla a lógica relacionada aos jogadores, cartas de reparo e plateNames.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks
    {
        private static PlayerManager _instance;
        public static PlayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PlayerManager>();
                }
                return _instance;
            }
        }

        [Header("Referências")]
        [SerializeField] private GameObject gameInfo;
        [SerializeField] private GameObject playerLeftBackground;

        // Cache de jogadores
        private PlayerScript[] players;
        private GiveCards[] cachedPlateNames;
        private RepairCard[] cachedRepairCards;
        private bool needsCacheRefresh = true;

        // Propriedades públicas
        public PlayerScript[] Players => players;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <summary>
        /// Retorna a lista de jogadores ordenada por ActorNumber.
        /// Garante ordem consistente em todos os clientes.
        /// </summary>
        public static Photon.Realtime.Player[] GetOrderedPlayerList()
        {
            return PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        }

        /// <summary>
        /// Atualiza o cache de jogadores e referências
        /// </summary>
        public void RefreshCache()
        {
            players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
            cachedPlateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);
            cachedRepairCards = FindObjectsByType<RepairCard>(FindObjectsSortMode.None);
            needsCacheRefresh = false;
            DebugHelper.Log("[PlayerManager] Cache atualizado");
        }

        /// <summary>
        /// Retorna os plateNames do cache
        /// </summary>
        public GiveCards[] GetCachedPlateNames()
        {
            if (needsCacheRefresh || cachedPlateNames == null)
            {
                cachedPlateNames = FindObjectsByType<GiveCards>(FindObjectsSortMode.None);
            }
            return cachedPlateNames;
        }

        /// <summary>
        /// Retorna as cartas de reparo do cache
        /// </summary>
        public RepairCard[] GetCachedRepairCards()
        {
            if (needsCacheRefresh || cachedRepairCards == null)
            {
                cachedRepairCards = FindObjectsByType<RepairCard>(FindObjectsSortMode.None);
            }
            return cachedRepairCards;
        }

        /// <summary>
        /// Invalida o cache
        /// </summary>
        public void InvalidateCache()
        {
            needsCacheRefresh = true;
        }

        /// <summary>
        /// Retorna o jogador local
        /// </summary>
        public PlayerScript GetLocalPlayer()
        {
            if (players == null) RefreshCache();

            foreach (var player in players)
            {
                if (player != null && player.photonView.IsMine)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Retorna o jogador no turno atual
        /// </summary>
        public PlayerScript GetPlayerByIndex(int index)
        {
            if (players == null) RefreshCache();

            foreach (var player in players)
            {
                if (player != null && player.index == index)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Retorna o jogador que está no turno.
        /// Delega para TurnManager para evitar duplicação.
        /// </summary>
        public PlayerScript GetCurrentTurnPlayer()
        {
            if (TurnManager.Instance != null)
            {
                return TurnManager.Instance.GetCurrentTurnPlayer();
            }

            // Fallback caso TurnManager não exista
            if (players == null) RefreshCache();

            foreach (var player in players)
            {
                if (player != null && player.GetYourTurn())
                {
                    return player;
                }
            }
            return null;
        }

        #region PlateNames Management

        /// <summary>
        /// Remove um jogador que saiu do jogo
        /// </summary>
        [PunRPC]
        public void RPC_RemovePlayerPlatename(int index)
        {
            index++;

            DebugHelper.Log("[PlayerManager] Removendo platename");

            var plate = GameObject.Find(GameObjectNames.GetPlateName(index));
            if (plate != null)
            {
                plate.GetComponent<MeshRenderer>().enabled = false;
                plate.GetComponent<MeshCollider>().enabled = false;
            }

            var repairSymbol = GameObject.Find(GameObjectNames.GetRepairCardSymbol(index));
            if (repairSymbol != null)
            {
                repairSymbol.GetComponent<SpriteRenderer>().enabled = false;
            }

            var namePlate = GameObject.Find(GameObjectNames.GetNamePlayer(index));
            if (namePlate != null)
            {
                namePlate.GetComponent<TMP_Text>().text = " ";
                namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }

            var numberRepairCard = GameObject.Find(GameObjectNames.GetNumberRepairCards(index));
            if (numberRepairCard != null)
            {
                numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";
            }
        }

        /// <summary>
        /// Reseta todos os plateNames
        /// </summary>
        public void ResetAllPlatenames()
        {
            DebugHelper.Log("[PlayerManager] Resetando platenames");

            for (int i = 0; i < 4; i++)
            {
                int playerNum = i + 1;

                var plate = GameObject.Find(GameObjectNames.GetPlateName(playerNum));
                if (plate != null)
                {
                    plate.GetComponent<MeshRenderer>().enabled = false;
                    plate.GetComponent<MeshCollider>().enabled = false;
                }

                var repairSymbol = GameObject.Find(GameObjectNames.GetRepairCardSymbol(playerNum));
                if (repairSymbol != null)
                {
                    repairSymbol.GetComponent<SpriteRenderer>().enabled = false;
                }

                var namePlate = GameObject.Find(GameObjectNames.GetNamePlayer(playerNum));
                if (namePlate != null)
                {
                    namePlate.GetComponent<TMP_Text>().text = " ";
                    namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
                }

                var numberRepairCard = GameObject.Find(GameObjectNames.GetNumberRepairCards(playerNum));
                if (numberRepairCard != null)
                {
                    numberRepairCard.GetComponent<TextMeshProUGUI>().text = " ";
                }
            }
        }

        #endregion

        #region Repair Cards

        /// <summary>
        /// Altera a visualização das cartas de reparo para o jogador do turno
        /// </summary>
        public void ChangeRepairCardsView(PlayerScript player)
        {
            if (player == null) return;

            var repairCards = GetCachedRepairCards();

            foreach (var card in repairCards)
            {
                if (card == null) continue;
                if (card.photonView.OwnerActorNr == player.photonView.OwnerActorNr)
                {
                    card.GetComponent<Animator>().SetBool("sending", false);
                    card.GetComponent<MeshRenderer>().enabled = true;
                }
                else
                {
                    card.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        /// <summary>
        /// Transfere uma carta de reparo entre jogadores
        /// </summary>
        [PunRPC]
        public void RPC_GiveRepairCard(int numberPlayer, int time)
        {
            PlayerScript playerSending = null;
            PlayerScript playerReceiving = null;

            if (players == null || players.Length == 0)
            {
                RefreshCache();
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

            if (playerSending == null || playerReceiving == null)
            {
                DebugHelper.Log("[PlayerManager] playerSending ou playerReceiving é null");
                return;
            }

            if (playerSending.GetNumberOfRepairsCards() > 0 && playerReceiving.GetNumberOfRepairsCards() < 5)
            {
                InvalidateCache();
                var repairCards = GetCachedRepairCards();
                List<RepairCard> orderedList = new List<RepairCard>();
                List<RepairCard> playerCards = new List<RepairCard>();

                foreach (var repairCard in repairCards)
                {
                    if (repairCard != null && repairCard.photonView.OwnerActorNr == playerSending.photonView.OwnerActorNr)
                    {
                        playerCards.Add(repairCard);
                    }
                }

                if (playerCards.Count == 0)
                {
                    DebugHelper.Log("[PlayerManager] Nenhuma carta encontrada para o jogador");
                    return;
                }

                orderedList = playerCards.OrderByDescending(x => x.index).ToList();
                RepairCard lastCard = orderedList[0];

                var orderedPlayerList = GetOrderedPlayerList();

                if (playerReceiving.index >= 0 && playerReceiving.index < orderedPlayerList.Length)
                {
                    lastCard.photonView.TransferOwnership(orderedPlayerList[playerReceiving.index]);
                }

                playerReceiving.numberRepairCards++;

                var findReceiverNumberCards = GameObject.Find(GameObjectNames.GetNumberRepairCards(numberPlayer));
                if (findReceiverNumberCards != null)
                {
                    findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text = playerReceiving.numberRepairCards.ToString();
                }

                playerSending.numberRepairCards--;

                var findSenderNumberCards = GameObject.Find(GameObjectNames.GetNumberRepairCards(time + 1));
                if (findSenderNumberCards != null)
                {
                    findSenderNumberCards.GetComponent<TextMeshProUGUI>().text = playerSending.numberRepairCards.ToString();
                }

                lastCard.GetComponent<Animator>().enabled = true;
                lastCard.GetComponent<Animator>().SetBool("sending", true);
            }
        }

        #endregion

        #region Player Left Notification

        /// <summary>
        /// Exibe notificação de jogador que saiu
        /// </summary>
        [PunRPC]
        public void RPC_ShowLeftPlayerInfo(string nickname)
        {
            DebugHelper.Log("[PlayerManager] ShowLeftPlayer: " + nickname);

            if (gameInfo != null) gameInfo.SetActive(true);
            if (playerLeftBackground != null)
            {
                playerLeftBackground.GetComponentInChildren<TMP_Text>().text = nickname + " left the game";
                playerLeftBackground.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
            }

            this.DelayedCall(1.5f, HideLeftPlayerInfo);
        }

        private void HideLeftPlayerInfo()
        {
            if (playerLeftBackground != null)
            {
                playerLeftBackground.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }

            this.DelayedCall(0.5f, DisableOnlyGameInfo);
        }

        private void DisableOnlyGameInfo()
        {
            if (gameInfo != null)
            {
                gameInfo.SetActive(false);
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
