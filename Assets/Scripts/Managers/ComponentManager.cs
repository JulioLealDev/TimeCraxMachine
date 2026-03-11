using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TimeCrax.Core;

namespace TimeCrax.Managers
{
    /// <summary>
    /// Gerenciador de componentes da máquina do tempo.
    /// Controla a lógica de malfuncionamento, roleta e reset de componentes.
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
        [SerializeField] private SoundEffects soundEffects;

        // Estado dos componentes
        private MachineComponent[] timeCraxComponents;
        private List<int> componentList = new List<int>();
        private Transform[] componentsWithAnimator = new Transform[20];
        private int randomId;

        // Propriedades públicas
        public MachineComponent[] Components => timeCraxComponents;
        public int RandomId => randomId;

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

            // Inicializar lista de componentes
            int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
            componentList.Clear();
            componentList.AddRange(numbers);

            DebugHelper.Log("[ComponentManager] Inicializado");
        }

        /// <summary>
        /// Gera um número aleatório de componente para malfunction
        /// </summary>
        public void RandomComponentNumber()
        {
            randomId = Random.Range(1, componentList.Count + 1);
            DebugHelper.Log($"[ComponentManager] RandomId gerado: {randomId}");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            photonView.RPC("RPC_ComponentRandom", RpcTarget.All, randomId);
        }

        [PunRPC]
        public void RPC_ComponentRandom(int id)
        {
            randomId = id;
            StartCoroutine(RouletteComponent());
        }

        private IEnumerator RouletteComponent()
        {
            int randomIndex = 0;
            int cond = 0;
            float interval = 0.3f;

            int componentCount = timeCraxComponents != null ? timeCraxComponents.Length : 0;
            while (cond < 13)
            {
                int index = Random.Range(0, componentCount);
                while (index == randomIndex && componentCount > 1)
                {
                    index = Random.Range(0, componentCount);
                }
                randomIndex = index;

                if (timeCraxComponents != null && index < timeCraxComponents.Length)
                {
                    var outline = timeCraxComponents[index].GetComponent<OutlineComponent>();
                    if (outline != null) outline.enabled = true;
                }

                if (soundEffects != null) soundEffects.PlayRouletteSound();
                yield return new WaitForSeconds(interval);

                if (timeCraxComponents != null && index < timeCraxComponents.Length)
                {
                    var outline = timeCraxComponents[index].GetComponent<OutlineComponent>();
                    if (outline != null) outline.enabled = false;
                }

                cond++;
                interval -= 0.015f;
            }

            // Última iteração no componente sorteado
            int finalIndex = randomId - 1;
            if (timeCraxComponents != null && finalIndex >= 0 && finalIndex < timeCraxComponents.Length)
            {
                var outline = timeCraxComponents[finalIndex].GetComponent<OutlineComponent>();
                if (outline != null) outline.enabled = true;

                if (soundEffects != null) soundEffects.PlayRouletteSound();
                yield return new WaitForSeconds(interval);

                if (outline != null) outline.enabled = false;
            }

            AddMalfunctionInComponent();
        }

        /// <summary>
        /// Adiciona malfunction no componente sorteado
        /// </summary>
        public void AddMalfunctionInComponent()
        {
            // Só reabilitar cursor se NÃO estiver em transição de turno
            if (!GameManager.IsInTurnTransition)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (timeCraxComponents == null) return;

            foreach (var component in timeCraxComponents)
            {
                if (component != null && component.componentId == randomId)
                {
                    component.AddMalfunction();
                }
            }
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
                    component.GetComponent<PhotonView>().TransferOwnership(player);
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
