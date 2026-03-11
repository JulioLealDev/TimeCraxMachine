using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using System.Collections;

namespace TimeCrax.Managers
{
    /// <summary>
    /// Gerenciador do termômetro da máquina do tempo.
    /// Controla a progressão de temperatura que leva ao malfunction.
    /// Progressão baseada na dificuldade:
    /// - Easy: 20 → 50 → 80 → 100
    /// - Medium/Hard: 20 → 60 → 100
    /// </summary>
    public class ThermometerManager : MonoBehaviourPunCallbacks
    {
        private static ThermometerManager _instance;
        public static ThermometerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ThermometerManager>();
                }
                return _instance;
            }
        }

        [Header("Referências")]
        [SerializeField] private Animator thermometerAnimator;
        [SerializeField] private ParticleSystem smokeParticle01;
        [SerializeField] private ParticleSystem smokeParticle02;

        [Header("Configuração")]
        [SerializeField] private int[] temperatureLevels = { 0, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        [SerializeField] private float malfunctionDelay = 3f; // Delay antes de iniciar malfunction

        // Progressões por dificuldade
        private readonly int[] easyProgression = { 20, 50, 80, 100 };
        private readonly int[] mediumHardProgression = { 20, 60, 100 };

        // Estado atual
        private int currentTemperature = 0;
        private int currentProgressionIndex = 0;
        private string currentDifficulty = "Normal";

        // Referência ao GameManager para chamar malfunction
        private GameManager gameManager;

        // Propriedades públicas
        public int CurrentTemperature => currentTemperature;
        public bool IsAtMaxTemperature => currentTemperature >= 100;

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

            // Se o animator não foi atribuído, tentar encontrar
            if (thermometerAnimator == null)
            {
                var pointer = GameObject.Find("Pointer_12");
                if (pointer != null)
                {
                    thermometerAnimator = pointer.GetComponent<Animator>();
                }
            }

            // Garantir que começa em 0
            SetTemperature(0);
        }

        /// <summary>
        /// Inicializa o termômetro para uma nova partida (vai para 20°C)
        /// </summary>
        public void Initialize()
        {
            DebugHelper.Log("[ThermometerManager] Inicializando termômetro para nova partida");

            // Obter dificuldade da sala
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("dif"))
            {
                currentDifficulty = PhotonNetwork.CurrentRoom.CustomProperties["dif"].ToString();
            }
            else
            {
                currentDifficulty = "Normal";
            }

            DebugHelper.Log($"[ThermometerManager] Dificuldade: {currentDifficulty}");

            // Resetar índice de progressão
            currentProgressionIndex = 0;

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_Initialize", RpcTarget.All, currentDifficulty);
            }
        }

        [PunRPC]
        public void RPC_Initialize(string difficulty)
        {
            currentDifficulty = difficulty;
            currentProgressionIndex = 0;
            SetTemperature(20);
            ActivateSmokeParticles();
            DebugHelper.Log($"[ThermometerManager] Inicializado com dificuldade: {difficulty}");
        }

        /// <summary>
        /// Retorna a progressão de temperatura baseada na dificuldade
        /// </summary>
        private int[] GetCurrentProgression()
        {
            if (currentDifficulty.ToLower() == "easy")
            {
                return easyProgression;
            }
            else
            {
                // Medium e Hard usam a mesma progressão
                return mediumHardProgression;
            }
        }

        /// <summary>
        /// Verifica se o próximo erro causará malfunction (temperatura chegará em 100)
        /// </summary>
        public bool WillNextErrorCauseMalfunction()
        {
            int nextTemperature = GetNextTemperature();
            return nextTemperature >= 100;
        }

        /// <summary>
        /// Retorna o tempo total que o sistema leva para processar um erro
        /// </summary>
        public float GetErrorProcessingTime()
        {
            if (WillNextErrorCauseMalfunction())
            {
                // Delay para malfunction + animação da roleta (~5s)
                return malfunctionDelay + 5f;
            }
            else
            {
                // Sem malfunction, sem delay adicional
                return 0f;
            }
        }

        /// <summary>
        /// Chamado quando o jogador erra (posicionamento, quiz ou turno finalizado).
        /// Aumenta a temperatura em 1 nível. Se chegar em 100, causa malfunction.
        /// </summary>
        public void OnPlayerError()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            DebugHelper.Log($"[ThermometerManager] Erro do jogador - Temperatura atual: {currentTemperature}");

            // Encontrar próximo nível de temperatura
            int nextTemperature = GetNextTemperature();

            if (nextTemperature >= 100)
            {
                // Chegou em 100 - causa malfunction e reseta para 20
                DebugHelper.Log("[ThermometerManager] Temperatura máxima! Causando malfunction...");
                photonView.RPC("RPC_SetTemperature", RpcTarget.All, 100);

                // Aguardar antes de causar malfunction
                this.DelayedCall(malfunctionDelay, () =>
                {
                    TriggerMalfunction();
                });
            }
            else
            {
                // Apenas aumenta temperatura
                DebugHelper.Log($"[ThermometerManager] Aumentando temperatura para {nextTemperature}");
                photonView.RPC("RPC_SetTemperatureWithIndex", RpcTarget.All, nextTemperature, currentProgressionIndex + 1);
            }
        }

        /// <summary>
        /// Obtém o próximo nível de temperatura baseado na dificuldade
        /// </summary>
        private int GetNextTemperature()
        {
            int[] progression = GetCurrentProgression();

            // Encontrar o próximo nível na progressão
            int nextIndex = currentProgressionIndex + 1;

            if (nextIndex >= progression.Length)
            {
                return 100; // Máximo
            }

            return progression[nextIndex];
        }

        /// <summary>
        /// Causa malfunction e reseta termômetro para 20
        /// </summary>
        private void TriggerMalfunction()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            DebugHelper.Log("[ThermometerManager] Disparando malfunction!");

            // Chamar malfunction no GameManager
            if (gameManager != null)
            {
                gameManager.RandomComponentNumber();
            }

            // Resetar para 20 após um delay para a animação do malfunction
            this.DelayedCall(3f, () =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // Resetar para primeiro nível (20) e índice 0
                    photonView.RPC("RPC_SetTemperatureWithIndex", RpcTarget.All, 20, 0);
                }
            });
        }

        /// <summary>
        /// Define a temperatura diretamente (usado internamente e no reset)
        /// </summary>
        private void SetTemperature(int temperature)
        {
            currentTemperature = temperature;
            UpdateAnimator();
            UpdateSmokeParticlesSpeed();
            DebugHelper.Log($"[ThermometerManager] Temperatura definida para {temperature}");
        }

        // Coroutine atual de transição de velocidade
        private Coroutine smokeSpeedTransitionCoroutine;
        private float currentSmokeSpeed = 1f;

        /// <summary>
        /// Atualiza a velocidade das partículas de fumaça baseado na temperatura
        /// </summary>
        private void UpdateSmokeParticlesSpeed()
        {
            float targetSpeed;

            if (currentTemperature >= 80)
            {
                targetSpeed = 5f;
            }
            else if (currentTemperature >= 50)
            {
                targetSpeed = 3f;
            }
            else
            {
                targetSpeed = 1f;
            }

            // Se a velocidade alvo é diferente da atual, iniciar transição
            if (!Mathf.Approximately(targetSpeed, currentSmokeSpeed))
            {
                // Cancelar transição anterior se existir
                if (smokeSpeedTransitionCoroutine != null)
                {
                    StopCoroutine(smokeSpeedTransitionCoroutine);
                }

                smokeSpeedTransitionCoroutine = StartCoroutine(TransitionSmokeSpeed(targetSpeed, 2f));
            }
        }

        /// <summary>
        /// Coroutine que faz a transição suave da velocidade das partículas
        /// </summary>
        private IEnumerator TransitionSmokeSpeed(float targetSpeed, float duration)
        {
            float startSpeed = currentSmokeSpeed;
            float elapsed = 0f;

            DebugHelper.Log($"[ThermometerManager] Transição de velocidade: {startSpeed} → {targetSpeed}");

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Interpolação suave (ease in-out)
                t = t * t * (3f - 2f * t);

                currentSmokeSpeed = Mathf.Lerp(startSpeed, targetSpeed, t);

                SetSmokeParticleSpeed(smokeParticle01, currentSmokeSpeed);
                SetSmokeParticleSpeed(smokeParticle02, currentSmokeSpeed);

                yield return null;
            }

            // Garantir valor final exato
            currentSmokeSpeed = targetSpeed;
            SetSmokeParticleSpeed(smokeParticle01, currentSmokeSpeed);
            SetSmokeParticleSpeed(smokeParticle02, currentSmokeSpeed);

            DebugHelper.Log($"[ThermometerManager] Velocidade das partículas: {currentSmokeSpeed}");
            smokeSpeedTransitionCoroutine = null;
        }

        /// <summary>
        /// Define a velocidade de simulação de um ParticleSystem
        /// </summary>
        private void SetSmokeParticleSpeed(ParticleSystem particle, float speed)
        {
            if (particle != null)
            {
                var main = particle.main;
                main.simulationSpeed = speed;
            }
        }

        /// <summary>
        /// Ativa as partículas de fumaça
        /// </summary>
        public void ActivateSmokeParticles()
        {
            if (smokeParticle01 != null)
            {
                smokeParticle01.gameObject.SetActive(true);
            }
            if (smokeParticle02 != null)
            {
                smokeParticle02.gameObject.SetActive(true);
            }
            DebugHelper.Log("[ThermometerManager] Partículas de fumaça ativadas");
        }

        /// <summary>
        /// Desativa as partículas de fumaça
        /// </summary>
        public void DeactivateSmokeParticles()
        {
            if (smokeParticle01 != null)
            {
                smokeParticle01.gameObject.SetActive(false);
            }
            if (smokeParticle02 != null)
            {
                smokeParticle02.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Atualiza o Animator do termômetro
        /// </summary>
        private void UpdateAnimator()
        {
            if (thermometerAnimator != null)
            {
                thermometerAnimator.enabled = true;
                thermometerAnimator.SetInteger("thermometer", currentTemperature);
                DebugHelper.Log($"[ThermometerManager] Animator atualizado - thermometer={currentTemperature}");
            }
            else
            {
                DebugHelper.Log("[ThermometerManager] AVISO: thermometerAnimator é null!");
            }
        }

        /// <summary>
        /// Reseta o termômetro para o estado inicial (0)
        /// </summary>
        public void ResetThermometer()
        {
            DebugHelper.Log("[ThermometerManager] Resetando termômetro");

            currentProgressionIndex = 0;

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_SetTemperatureWithIndex", RpcTarget.All, 0, 0);
            }
            else
            {
                SetTemperature(0);
            }

            // Desabilitar animator após reset
            if (thermometerAnimator != null)
            {
                thermometerAnimator.enabled = false;
            }

            // Desativar partículas de fumaça
            DeactivateSmokeParticles();
        }

        #region RPCs

        [PunRPC]
        public void RPC_SetTemperature(int temperature)
        {
            // Desabilitar cursor imediatamente quando temperatura atinge 100
            if (temperature >= 100)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            SetTemperature(temperature);
        }

        [PunRPC]
        public void RPC_SetTemperatureWithIndex(int temperature, int progressionIndex)
        {
            currentProgressionIndex = progressionIndex;
            SetTemperature(temperature);
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
