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
    /// Controla a lógica relacionada aos jogadores, cartas de bonus e plateNames.
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
        private BonusCard[] cachedBonusCards;
        private bool needsCacheRefresh = true;

        // Propriedades públicas
        public PlayerScript[] Players
        {
            get
            {
                if (players == null) RefreshCache();
                return players;
            }
        }

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
            cachedBonusCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
            needsCacheRefresh = false;
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
        /// Retorna as cartas de bonus do cache
        /// </summary>
        public BonusCard[] GetCachedBonusCards()
        {
            if (needsCacheRefresh || cachedBonusCards == null)
            {
                cachedBonusCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
            }
            return cachedBonusCards;
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
        /// Oculta todos os elementos de UI de um slot de jogador (1-based).
        /// </summary>
        public void ResetPlayerUIElements(int playerNum)
        {
            var plate = GameObject.Find(GameObjectNames.GetPlateName(playerNum));
            if (plate != null)
            {
                plate.GetComponent<MeshRenderer>().enabled = false;
                plate.GetComponent<MeshCollider>().enabled = false;
            }

            var bonusSymbol = GameObject.Find(GameObjectNames.GetBonusCardSymbol(playerNum));
            if (bonusSymbol != null)
                bonusSymbol.GetComponent<SpriteRenderer>().enabled = false;

            var namePlate = GameObject.Find(GameObjectNames.GetNamePlayer(playerNum));
            if (namePlate != null)
            {
                namePlate.GetComponent<TMP_Text>().text = " ";
                namePlate.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }

            var numberBonusCard = GameObject.Find(GameObjectNames.GetNumberBonusCards(playerNum));
            if (numberBonusCard != null)
                numberBonusCard.GetComponent<TextMeshProUGUI>().text = " ";
        }

        /// <summary>
        /// Reseta todos os plateNames (slots 1–4).
        /// </summary>
        public void ResetAllPlatenames()
        {
            for (int i = 1; i <= 4; i++)
                ResetPlayerUIElements(i);
        }

        #endregion

        #region Bonus Cards

        /// <summary>
        /// Altera a visualização das cartas de bonus para o jogador do turno
        /// </summary>
        public void ChangeBonusCardsView(PlayerScript player)
        {
            if (player == null) return;

            var bonusCards = GetCachedBonusCards();

            foreach (var card in bonusCards)
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
        /// Transfere uma carta de bonus entre jogadores
        /// </summary>
        [PunRPC]
        public void RPC_GiveBonusCard(int numberPlayer, int time)
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
                return;
            }

            if (playerSending.GetNumberOfBonusCards() > 0 && playerReceiving.GetNumberOfBonusCards() < 5)
            {
                InvalidateCache();
                var bonusCards = GetCachedBonusCards();
                List<BonusCard> orderedList = new List<BonusCard>();
                List<BonusCard> playerCards = new List<BonusCard>();

                foreach (var bonusCard in bonusCards)
                {
                    if (bonusCard != null && bonusCard.photonView.OwnerActorNr == playerSending.photonView.OwnerActorNr)
                    {
                        playerCards.Add(bonusCard);
                    }
                }

                if (playerCards.Count == 0)
                {
                    return;
                }

                orderedList = playerCards.OrderByDescending(x => x.index).ToList();
                BonusCard lastCard = orderedList[0];

                var orderedPlayerList = GetOrderedPlayerList();

                if (playerReceiving.index >= 0 && playerReceiving.index < orderedPlayerList.Length)
                {
                    if (lastCard.photonView != null && lastCard.photonView.ViewID > 0) { lastCard.photonView.TransferOwnership(orderedPlayerList[playerReceiving.index]); }
                }

                playerReceiving.numberBonusCards++;

                var findReceiverNumberCards = GameObject.Find(GameObjectNames.GetNumberBonusCards(numberPlayer));
                if (findReceiverNumberCards != null)
                {
                    findReceiverNumberCards.GetComponent<TextMeshProUGUI>().text = playerReceiving.numberBonusCards.ToString();
                }

                playerSending.numberBonusCards--;

                var findSenderNumberCards = GameObject.Find(GameObjectNames.GetNumberBonusCards(time + 1));
                if (findSenderNumberCards != null)
                {
                    findSenderNumberCards.GetComponent<TextMeshProUGUI>().text = playerSending.numberBonusCards.ToString();
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
