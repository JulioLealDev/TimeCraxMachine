using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;

namespace TimeCrax.Managers
{
    /// <summary>
    /// Gerenciador de componentes da máquina do tempo.
    /// Controla o estado e reset de componentes.
    /// </summary>
    public class ComponentManager : MonoBehaviourPunCallbacks
    {
        private static ComponentManager _instance;
        public static ComponentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ComponentManager>();
                }
                return _instance;
            }
        }

        [Header("Referências")]
        [SerializeField] private GameObject enviroment;

        // Estado dos componentes
        private MachineComponent[] timeCraxComponents;
        private Transform[] componentsWithAnimator = new Transform[20];

        // Propriedades públicas
        public MachineComponent[] Components => timeCraxComponents;

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
        /// Inicializa os componentes para uma nova partida
        /// </summary>
        public void Initialize()
        {
            int index = 0;

            timeCraxComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);

            if (enviroment != null)
            {
                Transform[] components = enviroment.GetComponentsInChildren<Transform>();

                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i].CompareTag("Component"))
                    {
                        DebugHelper.Log($"[ComponentManager] Ativando animator do componente {components[i].name}");
                        components[i].GetComponent<Animator>().enabled = true;
                        componentsWithAnimator[index] = components[i];
                        index++;
                    }
                }
            }

            DebugHelper.Log("[ComponentManager] Inicializado");
        }

        /// <summary>
        /// Configura o estado dos componentes com malfunction
        /// </summary>
        public void SetupComponentsState(PlayerScript[] players, int time)
        {
            if (timeCraxComponents == null || players == null) return;

            foreach (var player in players)
            {
                if (player == null) continue;

                foreach (var component in timeCraxComponents)
                {
                    if (component == null || component.malfunctions != 1) continue;

                    if (player.GetYourTurn())
                    {
                        component.GetComponent<MeshCollider>().enabled = true;
                    }
                    else
                    {
                        component.GetComponent<MeshCollider>().enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Bloqueia interação com componentes com malfunction
        /// </summary>
        public void BlockMalfunctionComponents()
        {
            var suitComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);
            foreach (var suitComponent in suitComponents)
            {
                if (suitComponent != null && suitComponent.malfunctions == 1)
                {
                    suitComponent.tag = "Disabled";
                }
            }
        }

        /// <summary>
        /// Desativa interação com todos os componentes com malfunction
        /// </summary>
        public void DeactivateMalfunctionComponents()
        {
            var suitComponents = FindObjectsByType<MachineComponent>(FindObjectsSortMode.None);
            foreach (var suitComponent in suitComponents)
            {
                if (suitComponent != null && suitComponent.malfunctions > 0)
                {
                    DebugHelper.Log($"[ComponentManager] {suitComponent.name} desativado");
                    suitComponent.GetComponent<MeshCollider>().enabled = false;
                }
            }
        }

        /// <summary>
        /// Ativa componentes com malfunction como selecionáveis
        /// </summary>
        public void ActivateMalfunctionComponentsSelectable()
        {
            if (timeCraxComponents == null) return;

            foreach (var component in timeCraxComponents)
            {
                if (component != null && component.malfunctions == 1)
                {
                    component.tag = "Selectable";
                }
            }
        }

        /// <summary>
        /// Reseta todos os componentes para o estado inicial
        /// </summary>
        public void ResetAllComponents()
        {
            DebugHelper.Log("[ComponentManager] ResetAllComponents");

            if (timeCraxComponents != null)
            {
                foreach (var component in timeCraxComponents)
                {
                    if (component != null)
                    {
                        component.malfunctions = 0;
                    }
                }
            }

            if (componentsWithAnimator != null)
            {
                foreach (var component in componentsWithAnimator)
                {
                    if (component == null) continue;

                    DebugHelper.Log($"[ComponentManager] Resetando {component.name}");
                    var animator = component.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetBool("malfunction", false);
                        animator.enabled = false;
                    }
                    component.tag = "Component";

                    ParticleSystem[] effects = component.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var effect in effects)
                    {
                        effect.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Transfere ownership dos componentes para um jogador
        /// </summary>
        public void TransferComponentsOwnership(Photon.Realtime.Player player)
        {
            if (timeCraxComponents == null || player == null) return;

            foreach (var component in timeCraxComponents)
            {
                if (component != null)
                {
                    var pv = component.GetComponent<PhotonView>(); if (pv != null && pv.ViewID > 0) { pv.TransferOwnership(player); }
                }
            }
        }

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
