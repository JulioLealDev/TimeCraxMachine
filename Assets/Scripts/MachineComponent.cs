using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using System.Collections.Generic;
using TimeCrax.Core;
using TimeCrax.Managers;

public class MachineComponent : MonoBehaviourPunCallbacks
{
    public int componentId;
    public int malfunctions = 0;
    [SerializeField] private GameObject gameInfo;

    [Header("Componente Especial")]
    [Tooltip("Tipo de componente especial (None, Battery, Cooler)")]
    [SerializeField] private SpecialComponentType specialType = SpecialComponentType.None;

    [Header("Smoothness - Objetos afetados pelo malfunction crítico")]
    [Tooltip("Lista de Renderers que terão Smoothness alterado para 0 quando malfunction = 2")]
    [SerializeField] private List<Renderer> smoothnessTargets = new List<Renderer>();

    /// <summary>
    /// Tipos de componentes especiais
    /// </summary>
    public enum SpecialComponentType
    {
        None,
        Battery,
        Cooler,
        Thermometer
    }

    private SoundEffects soundEffects;
    private EndMatch endMatchScreen;
    private GameManager cachedGameManager;
    private BackgroundMusic cachedBackgroundMusic;
    private TurnTimer cachedTurnTimer;
    private Transform sparks;
    private Transform smoke;
    private Transform componentWithAnimator = null;
    private List<Transform> childrenWithanimator = new List<Transform>();

    // Componentes cacheados para evitar GetComponent repetido
    private MeshCollider cachedMeshCollider;
    private Animator cachedAnimator;
    private List<Animator> cachedChildAnimators = new List<Animator>();

    // Cache para smoothness e cor original dos targets
    private List<float> originalSmoothness = new List<float>();
    private List<Color> originalAlbedoColors = new List<Color>();

    // Instâncias de materiais para cada target (evita compartilhamento)
    private List<Material> materialInstances = new List<Material>();

    /// <summary>
    /// Define o parâmetro bool "malfunction" no Animator, se existir
    /// </summary>
    private void SafeSetMalfunctionBool(Animator anim, bool value)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;

        foreach (var param in anim.parameters)
        {
            if (param.name == "malfunction" && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool("malfunction", value);
                return;
            }
        }
    }

        /// <summary>
    /// Define o parâmetro bool "broken" no Animator, se existir
    /// </summary>
    private void SafeSetBrokenBool(Animator anim, bool value)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;

        foreach (var param in anim.parameters)
        {
            if (param.name == "broken" && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool("broken", value);
                return;
            }
        }
    }

    void Start()
    {
        soundEffects = FindFirstObjectByType<SoundEffects>();
        endMatchScreen = FindFirstObjectByType<EndMatch>();
        cachedGameManager = FindFirstObjectByType<GameManager>();
        cachedBackgroundMusic = FindFirstObjectByType<BackgroundMusic>();
        cachedTurnTimer = FindFirstObjectByType<TurnTimer>();

        // Cache do MeshCollider
        cachedMeshCollider = GetComponent<MeshCollider>();

        var parent = gameObject.transform.parent;
        if (parent.name != "Enviroment")
        {
            Transform[] opcoes = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < opcoes.Length; i++)
            {
                Animator anim = opcoes[i].GetComponent<Animator>();
                if (anim != null)
                {
                    childrenWithanimator.Add(opcoes[i]);
                    cachedChildAnimators.Add(anim); // Cache do Animator
                }
                else if (opcoes[i].CompareTag("Sparks"))
                {
                    sparks = opcoes[i];
                }
                else if (opcoes[i].CompareTag("Smoke"))
                {
                    smoke = opcoes[i];
                }
            }
        }
        else
        {
            componentWithAnimator = gameObject.GetComponent<Transform>();
            cachedAnimator = GetComponent<Animator>(); // Cache do Animator principal

            Transform[] childs = gameObject.GetComponentsInChildren<Transform>(true);
            foreach(var child in childs)
            {
                if (child.CompareTag("Sparks"))
                {
                    sparks = child;
                }
                else if (child.CompareTag("Smoke"))
                {
                    smoke = child;
                }
            }
        }

        // Cache dos Renderers, criar instâncias de material e guardar valores originais
        CacheRenderersAndSmoothness();
    }

    /// <summary>
    /// Cachea os valores originais de Smoothness e cor Albedo dos targets configurados
    /// e cria instâncias de material para cada renderer
    /// </summary>
    private void CacheRenderersAndSmoothness()
    {
        originalSmoothness.Clear();
        originalAlbedoColors.Clear();
        materialInstances.Clear();

        foreach (var renderer in smoothnessTargets)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                // Criar instância de material para este renderer (força cópia única)
                Material matInstance = renderer.material; // Isso cria uma instância
                materialInstances.Add(matInstance);

                // Guardar smoothness original
                float smoothness = matInstance.HasProperty("_Glossiness")
                    ? matInstance.GetFloat("_Glossiness")
                    : 0.5f;
                originalSmoothness.Add(smoothness);

                // Guardar cor Albedo original
                Color albedo = matInstance.HasProperty("_Color")
                    ? matInstance.GetColor("_Color")
                    : Color.white;
                originalAlbedoColors.Add(albedo);
            }
            else
            {
                materialInstances.Add(null);
                originalSmoothness.Add(0.5f);
                originalAlbedoColors.Add(Color.white);
            }
        }
    }

        /// <summary>
    /// Define o parâmetro "malfunction" nos Animators dos smoothnessTargets
    /// </summary>
    private void SetTargetsBrokenState(bool value)
    {
        foreach (var renderer in smoothnessTargets)
        {
            if (renderer != null)
            {
                var animator = renderer.GetComponent<Animator>();
                if (animator != null)
                {
                    SafeSetBrokenBool(animator, value);
                }
            }
        }
    }

    /// <summary>
    /// Desativa os Animators dos smoothnessTargets com delay
    /// </summary>
    private void DisableTargetsAnimatorsWithDelay(float delay)
    {
        this.DelayedCall(delay, DisableTargetsAnimators);
    }

    /// <summary>
    /// Desativa os Animators dos smoothnessTargets
    /// </summary>
    private void DisableTargetsAnimators()
    {
        foreach (var renderer in smoothnessTargets)
        {
            if (renderer != null)
            {
                var animator = renderer.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Restaura o Smoothness e cor Albedo original de todos os materiais configurados
    /// e reativa os Animators desses objetos
    /// </summary>
    private void RestoreMaterialSmoothness()
    {
        for (int i = 0; i < materialInstances.Count; i++)
        {
            var mat = materialInstances[i];
            if (mat != null)
            {
                // Restaurar Smoothness
                if (i < originalSmoothness.Count && mat.HasProperty("_Glossiness"))
                {
                    mat.SetFloat("_Glossiness", originalSmoothness[i]);
                }

                // Restaurar cor Albedo
                if (i < originalAlbedoColors.Count && mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", originalAlbedoColors[i]);
                }
            }

            // Reativar Animator se existir
            if (i < smoothnessTargets.Count && smoothnessTargets[i] != null)
            {
                var animator = smoothnessTargets[i].GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = true;
                }
            }
        }
    }

    public void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        // Bloquear clique durante animações de câmera
        if (CameraController.IsAnimating) return;

        if (!GameManager.TryBeginClick(this)) return;

        if (gameObject.CompareTag("Selectable"))
        {
            var player = PlayerManager.Instance?.GetCurrentTurnPlayer();
            if (player != null)
            {
                BonusCard repairCard = BonusCardManager.Instance?.GetRepairCard(player);

                if (repairCard != null)
                {
                    photonView.RPC("RemoveMalfunction", RpcTarget.All);
                    repairCard.ConsumeCard();

                    Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                    gameInfo.gameObject.SetActive(true);

                    foreach (var info in infos)
                    {
                        if (info.gameObject.name == "RepairInfoBackground")
                        {
                            info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                        }
                    }

                    this.DelayedCall(1.5f, HideRepairInfo);
                }
                else
                {
                    Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                    gameInfo.gameObject.SetActive(true);

                    foreach (var info in infos)
                    {
                        if (info.gameObject.name == "ComponentInfoBackground")
                        {
                            info.GetComponentInChildren<TextMeshProUGUI>().text = "You need a Repair Component Card to repair a component!";
                            info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                        }
                    }

                    this.DelayedCall(1.5f, HideComponentInfo);
                }
            }
        }
        else
        {

            Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
            gameInfo.gameObject.SetActive(true);

            foreach (var info in infos)
            {
                if (info.gameObject.name == "ActionInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }

            this.DelayedCall(1.5f, HideActionInfo);
        }

    }
    public void HideRepairInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "RepairInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }
    public void HideActionInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ActionInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }
    public void HideComponentInfo()
    {

        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ComponentInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
        GameManager.ResetClick(this);
    }

    public void AddMalfunction()
    {
        malfunctions++;

        if (malfunctions >= 2)
        {
            // Componente com 2 malfunctions - ativa fumaça
            soundEffects.PlayFinalComponentExplosionSound();

            if (smoke != null)
            {
                smoke.gameObject.SetActive(true);
            }

            // Desativar sparks quando malfunction = 2
            if (sparks != null)
            {
                sparks.gameObject.SetActive(false);
            }

            // Alterar tag para Untagged quando malfunction crítico
            gameObject.tag = "Untagged";

            SetTargetsBrokenState(true);

            // Desativar animators após 2 segundos
            //DisableTargetsAnimatorsWithDelay(2f);

            // Verificar condição de derrota global (3 componentes com malfunction=2)
            if (cachedGameManager != null)
            {
                cachedGameManager.CheckGameOverCondition();
            }
        }
        else
        {
            // Primeiro malfunction - ativa animação e sparks
            if (componentWithAnimator != null)
            {
                if (cachedAnimator != null)
                {
                    cachedAnimator.enabled = true;
                    SafeSetMalfunctionBool(cachedAnimator, true);
                }
            }
            else
            {
                for (int i = 0; i < cachedChildAnimators.Count; i++)
                {
                    if (cachedChildAnimators[i] != null)
                    {
                        cachedChildAnimators[i].enabled = true;
                        SafeSetMalfunctionBool(cachedChildAnimators[i], true);
                    }
                }

            }

            if (sparks != null)
            {
                sparks.gameObject.SetActive(true);
            }
            soundEffects.PlayComponentExplosionSound();

            // Aplicar efeito de componente especial
            ApplySpecialComponentEffect();
        }

    }

    /// <summary>
    /// Aplica o efeito especial do componente quando malfunction = 1
    /// </summary>
    private void ApplySpecialComponentEffect()
    {
        switch (specialType)
        {
            case SpecialComponentType.Battery:
                // Reduz o tempo do timer pela metade
                if (cachedTurnTimer != null)
                    cachedTurnTimer.ApplyBatteryMalfunction();
                break;

            case SpecialComponentType.Cooler:
                // Aumenta os níveis de temperatura em 20°C
                if (ThermometerManager.Instance != null)
                {
                    ThermometerManager.Instance.ApplyCoolerMalfunction();
                }
                break;

            case SpecialComponentType.Thermometer:
                // Define malfunction = true no animator do termômetro
                if (ThermometerManager.Instance != null)
                {
                    ThermometerManager.Instance.SetThermometerMalfunctionState(true);
                }
                break;
        }
    }

    /// <summary>
    /// Remove o efeito especial do componente quando reparado
    /// </summary>
    private void RemoveSpecialComponentEffect()
    {
        switch (specialType)
        {
            case SpecialComponentType.Battery:
                // Restaura o tempo do timer
                if (cachedTurnTimer != null)
                    cachedTurnTimer.RestoreBatteryEffect();
                break;

            case SpecialComponentType.Cooler:
                // Reduz os níveis de temperatura em 20°C
                if (ThermometerManager.Instance != null)
                {
                    ThermometerManager.Instance.RemoveCoolerMalfunction();
                }
                break;

            case SpecialComponentType.Thermometer:
                // Define malfunction = false no animator do termômetro
                if (ThermometerManager.Instance != null)
                {
                    ThermometerManager.Instance.SetThermometerMalfunctionState(false);
                }
                break;
        }
    }

    public void EndGame()
    {
        cachedGameManager.DeactivateAll();
        cachedGameManager.ResetAllComponents();
        cachedGameManager.ResetAllPlatenames();

        cachedBackgroundMusic.PlayGameOverSound();
        endMatchScreen.transform.GetChild(0).gameObject.SetActive(true);
        cachedGameManager.Hud.SetActive(false);
    }

    [PunRPC]
    public void RemoveMalfunction()
    {
        // Definir malfunction = false imediatamente
        if (componentWithAnimator != null)
        {
            if (cachedAnimator != null)
            {
                SafeSetMalfunctionBool(cachedAnimator, false);
            }
        }
        else
        {
            for (int i = 0; i < cachedChildAnimators.Count; i++)
            {
                if (cachedChildAnimators[i] != null)
                {
                    SafeSetMalfunctionBool(cachedChildAnimators[i], false);
                }
            }
        }

        if (sparks != null)
        {
            sparks.gameObject.SetActive(false);
        }
        soundEffects.PlayComponentRepairSound();

        // Remover efeito especial se tinha malfunction = 1
        if (malfunctions == 1)
        {
            RemoveSpecialComponentEffect();
        }

        malfunctions--;
        if (cachedMeshCollider != null)
        {
            cachedMeshCollider.enabled = false;
        }

        if (cachedGameManager != null)
            cachedGameManager.BlockActions();
    }

    /// <summary>
    /// Reseta o componente para o estado inicial (usado ao reiniciar partida)
    /// </summary>
    public void ResetComponent()
    {
        malfunctions = 0;
        gameObject.tag = "Component";

        // Resetar Animator principal
        if (cachedAnimator != null)
        {
            SafeSetMalfunctionBool(cachedAnimator, false);
            SafeSetBrokenBool(cachedAnimator, false);
            cachedAnimator.enabled = false;
        }

        // Resetar Animators filhos
        for (int i = 0; i < cachedChildAnimators.Count; i++)
        {
            if (cachedChildAnimators[i] != null)
            {
                SafeSetMalfunctionBool(cachedChildAnimators[i], false);
                SafeSetBrokenBool(cachedChildAnimators[i], false);
                cachedChildAnimators[i].enabled = false;
            }
        }

        // Desativar efeitos de partículas
        if (sparks != null)
        {
            sparks.gameObject.SetActive(false);
        }
        if (smoke != null)
        {
            smoke.gameObject.SetActive(false);
        }

        // Restaurar Smoothness original dos materiais
        RestoreMaterialSmoothness();

        // Resetar parâmetro broken nos smoothnessTargets
        SetTargetsBrokenState(false);

        // Desabilitar MeshCollider
        if (cachedMeshCollider != null)
        {
            cachedMeshCollider.enabled = false;
        }
    }


}
