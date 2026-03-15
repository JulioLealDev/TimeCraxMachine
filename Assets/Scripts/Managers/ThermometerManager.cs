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
    /// - Easy: 20 → 40 → 60 → 80 → 100
    /// - Normal: 20 → 50 → 80 → 100
    /// - Hard: 20 → 60 → 100
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
        [SerializeField] private float malfunctionDelay = 3f; // Delay antes de iniciar malfunction

        // Progressões por dificuldade (base)
        private readonly int[] easyProgressionBase = { 20, 40, 60, 80, 100 };
        private readonly int[] mediumProgressionBase = { 20, 50, 80, 100 };
        private readonly int[] hardProgressionBase = { 20, 60, 100 };

        // Progressões atuais (podem ser modificadas pelos coolers)
        private int[] easyProgression;
        private int[] mediumProgression;
        private int[] hardProgression;

        // Estado atual
        private int currentTemperature = 0;
        private int currentProgressionIndex = 0;
        private string currentDifficulty = "Normal";

        // Contador de coolers com malfunction
        private int coolersMalfunctioning = 0;

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
                var pointer = GameObject.Find("Pointer");
                if (pointer != null)
                {
                    thermometerAnimator = pointer.GetComponent<Animator>();
                }
            }

            // Inicializar progressões
            ResetProgressions();

            // Temperatura inicial será definida em Initialize() quando a partida começar
            currentTemperature = 0;
        }

        /// <summary>
        /// Reseta as progressões para os valores base
        /// </summary>
        private void ResetProgressions()
        {
            easyProgression = (int[])easyProgressionBase.Clone();
            mediumProgression = (int[])mediumProgressionBase.Clone();
            hardProgression = (int[])hardProgressionBase.Clone();
            coolersMalfunctioning = 0;
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

            // Resetar progressões para garantir valores base em nova partida
            ResetProgressions();

            // Usar primeiro nível da progressão atual
            int[] progression = GetCurrentProgression();
            int firstLevel = progression.Length > 0 ? progression[0] : 20;
            SetTemperature(firstLevel);

            ActivateSmokeParticles();
            DebugHelper.Log($"[ThermometerManager] Inicializado com dificuldade: {difficulty}, temperatura inicial: {firstLevel}");
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
            else if(currentDifficulty.ToLower() == "normal")
            {
                return mediumProgression;
            }
            else
            {
                return hardProgression;
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
            // O reset de temperatura será feito pelo GameManager após aplicar o malfunction
            if (gameManager != null)
            {
                gameManager.RandomComponentNumber();
            }
        }

        /// <summary>
        /// Reseta a temperatura para o primeiro nível da progressão atual.
        /// Chamado pelo GameManager após aplicar malfunction no componente.
        /// </summary>
        public void ResetTemperatureToFirstLevel()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int[] progression = GetCurrentProgression();
            int firstLevel = progression.Length > 0 ? progression[0] : 20;

            photonView.RPC("RPC_SetTemperatureWithIndex", RpcTarget.All, firstLevel, 0);
            DebugHelper.Log($"[ThermometerManager] Temperatura resetada para {firstLevel}");
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
        /// Define o estado de malfunction no animator do termômetro.
        /// Chamado quando o componente Thermometer recebe malfunction.
        /// </summary>
        public void SetThermometerMalfunctionState(bool isMalfunctioning)
        {
            if (thermometerAnimator != null)
            {
                thermometerAnimator.SetBool("malfunction", isMalfunctioning);
                DebugHelper.Log($"[ThermometerManager] Animator malfunction = {isMalfunctioning}");
            }
        }

        /// <summary>
        /// Reseta o termômetro para o estado inicial (0)
        /// </summary>
        public void ResetThermometer()
        {
            DebugHelper.Log("[ThermometerManager] Resetando termômetro");

            currentProgressionIndex = 0;

            // Resetar progressões para valores base
            ResetProgressions();

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

        /// <summary>
        /// Aplica o efeito de malfunction do cooler (aumenta níveis de temperatura em 20°C)
        /// </summary>
        public void ApplyCoolerMalfunction()
        {
            coolersMalfunctioning++;
            RecalculateProgressions();

            // Aumentar temperatura atual em 20°C (capped em 100)
            int newTemperature = Mathf.Min(currentTemperature + 20, 100);

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_SetTemperature", RpcTarget.All, newTemperature);
            }
            else
            {
                SetTemperature(newTemperature);
            }

            DebugHelper.Log($"[ThermometerManager] Cooler com malfunction! Total: {coolersMalfunctioning}, Temperatura: {newTemperature}");
        }

        /// <summary>
        /// Remove o efeito de malfunction do cooler (quando reparado)
        /// </summary>
        public void RemoveCoolerMalfunction()
        {
            if (coolersMalfunctioning <= 0) return;

            coolersMalfunctioning--;
            RecalculateProgressions();

            // Diminuir temperatura atual em 20°C (mínimo do primeiro nível da progressão atual)
            int[] progression = GetCurrentProgression();
            int minTemperature = progression.Length > 0 ? progression[0] : 20;
            int newTemperature = Mathf.Max(currentTemperature - 20, minTemperature);

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_SetTemperature", RpcTarget.All, newTemperature);
            }
            else
            {
                SetTemperature(newTemperature);
            }

            DebugHelper.Log($"[ThermometerManager] Cooler reparado! Total: {coolersMalfunctioning}, Temperatura: {newTemperature}");
        }

        /// <summary>
        /// Recalcula as progressões baseado no número de coolers com malfunction
        /// Cada cooler aumenta os níveis em 20°C (mantendo 100 como máximo)
        /// </summary>
        private void RecalculateProgressions()
        {
            int offset = coolersMalfunctioning * 20;

            easyProgression = ApplyOffsetToProgression(easyProgressionBase, offset);
            mediumProgression = ApplyOffsetToProgression(mediumProgressionBase, offset);
            hardProgression = ApplyOffsetToProgression(hardProgressionBase, offset);

            DebugHelper.Log($"[ThermometerManager] Progressões recalculadas com offset de {offset}°C");
            DebugHelper.Log($"[ThermometerManager] Easy: [{string.Join(", ", easyProgression)}]");
            DebugHelper.Log($"[ThermometerManager] Medium: [{string.Join(", ", mediumProgression)}]");
            DebugHelper.Log($"[ThermometerManager] Hard: [{string.Join(", ", hardProgression)}]");
        }

        /// <summary>
        /// Aplica um offset aos níveis de progressão, mantendo 100 como máximo
        /// Remove níveis duplicados que ultrapassariam 100
        /// </summary>
        private int[] ApplyOffsetToProgression(int[] baseProgression, int offset)
        {
            var newLevels = new System.Collections.Generic.List<int>();

            foreach (int level in baseProgression)
            {
                int newLevel = level + offset;

                // Se ultrapassar 100, usar 100
                if (newLevel >= 100)
                {
                    // Adicionar 100 apenas se ainda não estiver na lista
                    if (!newLevels.Contains(100))
                    {
                        newLevels.Add(100);
                    }
                }
                else
                {
                    newLevels.Add(newLevel);
                }
            }

            return newLevels.ToArray();
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
