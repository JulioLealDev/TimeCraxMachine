using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

namespace TimeCrax.Managers
{
    /// <summary>
    /// Gerenciador centralizado para cartas bonus.
    /// Controla a UI de ativação e aplicação de efeitos das cartas.
    /// </summary>
    public class BonusCardManager : MonoBehaviour
    {
        private static BonusCardManager _instance;
        public static BonusCardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<BonusCardManager>();
                }
                return _instance;
            }
        }

        [Header("UI de Ativação")]
        [SerializeField] private GameObject activationPanel;
        [SerializeField] private UnityEngine.UI.Button activateButton;
        [SerializeField] private TMPro.TextMeshProUGUI cardTypeLabel;

        // Controle de clique para cancelar
        private bool waitingForClickOutside = false;

        [Header("Referências")]
        [SerializeField] private TurnTimer turnTimer;
        [SerializeField] private GameManager gameManager;

        // Carta atualmente selecionada para ativação
        private BonusCard selectedCard;
        private bool hasSecondChanceActive = false;

        // Propriedades
        public bool HasSecondChanceActive => hasSecondChanceActive;

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
            // Encontrar referências se não atribuídas
            if (turnTimer == null)
            {
                turnTimer = FindFirstObjectByType<TurnTimer>();
            }
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            // Configurar botão de ativar
            if (activateButton != null)
            {
                activateButton.onClick.AddListener(OnActivateClicked);
            }

            // Esconder painel inicialmente
            if (activationPanel != null)
            {
                activationPanel.SetActive(false);
            }
        }

        private void Update()
        {
            // Detectar clique fora para cancelar
            if (waitingForClickOutside && Input.GetMouseButtonDown(0))
            {
                // Verificar se clicou no botão de ativar (não cancelar nesse caso)
                if (activateButton != null &&
                    UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == activateButton.gameObject)
                {
                    return;
                }

                // Verificar se clicou na carta (não cancelar nesse caso)
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.GetComponent<BonusCard>() == selectedCard)
                    {
                        return;
                    }
                }

                // Clicou fora - cancelar
                CancelActivation();
            }
        }

        #region Public Methods

        /// <summary>
        /// Abre o painel de ativação para uma carta
        /// </summary>
        public void ShowActivationPanel(BonusCard card)
        {
            if (card == null) return;

            // Não mostrar painel para RepairCard (auto-usa)
            if (card.CardType == BonusCardType.RepairComponent) return;

            // Verificar se pode ativar
            if (!CanActivateCard(card.CardType))
            {
                return;
            }

            selectedCard = card;

            // Atualizar UI
            if (cardTypeLabel != null)
            {
                cardTypeLabel.text = GetCardTypeName(card.CardType);
            }

            // Mostrar painel
            if (activationPanel != null)
            {
                activationPanel.SetActive(true);
            }

            // Aguardar um frame antes de ativar detecção de clique fora
            // (evita cancelar imediatamente pelo clique que abriu o painel)
            this.DelayedCall(0.1f, () => waitingForClickOutside = true);

        }

        /// <summary>
        /// Fecha o painel de ativação
        /// </summary>
        public void HideActivationPanel()
        {
            selectedCard = null;
            waitingForClickOutside = false;

            if (activationPanel != null)
            {
                activationPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Verifica se uma carta pode ser ativada no momento atual
        /// </summary>
        public bool CanActivateCard(BonusCardType cardType)
        {
            switch (cardType)
            {
                case BonusCardType.BonusTime:
                case BonusCardType.CoolThermometer:
                    // Sempre ativáveis
                    return true;

                case BonusCardType.SecondChanceSlot:
                    // Ativável a qualquer momento (efeito dura até errar slot)
                    return !hasSecondChanceActive;

                case BonusCardType.RepairComponent:
                    // Auto-usa, não passa por aqui
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Consome a carta de segunda chance (chamado quando jogador erra slot)
        /// </summary>
        public void ConsumeSecondChance()
        {
            hasSecondChanceActive = false;
        }

        /// <summary>
        /// Reseta o estado de segunda chance (chamado ao iniciar novo turno)
        /// </summary>
        public void ResetSecondChance()
        {
            hasSecondChanceActive = false;
        }

        /// <summary>
        /// Verifica se jogador tem uma carta de reparo do tipo Repair
        /// </summary>
        public BonusCard GetRepairCard(PlayerScript player)
        {
            if (player == null) return null;

            var bonusCards = FindObjectsByType<BonusCard>(FindObjectsSortMode.None);
            foreach (var card in bonusCards)
            {
                if (card != null &&
                    card.photonView.OwnerActorNr == player.photonView.OwnerActorNr &&
                    card.CardType == BonusCardType.RepairComponent)
                {
                    return card;
                }
            }
            return null;
        }

        #endregion

        #region Button Handlers

        private void OnActivateClicked()
        {
            if (selectedCard == null) return;


            // Guardar referência antes de esconder painel
            var card = selectedCard;
            var cardType = selectedCard.CardType;

            HideActivationPanel();

            // Aplicar efeito (os managers internos já fazem sincronização via RPC)
            ApplyCardEffect(cardType);

            // Consumir a carta
            card.ConsumeCard();
        }

        /// <summary>
        /// Cancela a ativação e retorna a carta para a mão
        /// </summary>
        private void CancelActivation()
        {

            // Retornar carta para a mão
            if (selectedCard != null)
            {
                selectedCard.ReturnToHand();
            }

            HideActivationPanel();
        }

        #endregion

        #region Card Effects

        /// <summary>
        /// Aplica o efeito da carta baseado no tipo
        /// </summary>
        private void ApplyCardEffect(BonusCardType cardType)
        {
            switch (cardType)
            {
                case BonusCardType.BonusTime:
                    ApplyTimeEffect();
                    break;

                case BonusCardType.CoolThermometer:
                    ApplyThermometerEffect();
                    break;

                case BonusCardType.SecondChanceSlot:
                    ApplySecondChanceEffect();
                    break;
            }
        }

        private void ApplyTimeEffect()
        {
            if (turnTimer != null)
            {
                turnTimer.AddTime(60f);
            }
        }

        private void ApplyThermometerEffect()
        {
            if (ThermometerManager.Instance != null)
            {
                ThermometerManager.Instance.ResetTemperatureToFirstLevel();
            }
        }

        private void ApplySecondChanceEffect()
        {
            if (gameManager != null && gameManager.photonView != null)
                gameManager.photonView.RPC("RPC_SetSecondChanceActive", Photon.Pun.RpcTarget.All, true);
            else
                hasSecondChanceActive = true;
        }

        public void SetSecondChanceState(bool active)
        {
            hasSecondChanceActive = active;
        }

        #endregion

        #region Helpers

        private string GetCardTypeName(BonusCardType cardType)
        {
            switch (cardType)
            {
                case BonusCardType.RepairComponent:
                    return "REPAIR COMPONENT";
                case BonusCardType.BonusTime:
                    return "BONUS TIME";
                case BonusCardType.SecondChanceSlot:
                    return "SECOND CHANCE";
                case BonusCardType.CoolThermometer:
                    return "COOL THERMOMETER";
                case BonusCardType.KillChallengeOption:
                    return "KILL CHALLENGE OPTION";
                case BonusCardType.SkipChallenge:
                    return "SKIP CHALLENGE";
                default:
                    return " ";
            }
        }

        #endregion


        #region Cleanup

        private void OnDestroy()
        {
            if (activateButton != null)
            {
                activateButton.onClick.RemoveAllListeners();
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion
    }
}
