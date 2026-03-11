using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using System.Collections.Generic;
using TimeCrax.Core;

public class MachineComponent : MonoBehaviourPunCallbacks
{
    public int componentId;
    public int malfunctions = 0;
    public GameObject gameInfo;

    [Header("Smoothness - Objetos afetados pelo malfunction crítico")]
    [Tooltip("Lista de Renderers que terão Smoothness alterado para 0 quando malfunction = 2")]
    [SerializeField] private List<Renderer> smoothnessTargets = new List<Renderer>();

    private SoundEffects soundEffects;
    private GameOver gameOver;
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

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    /// <summary>
    /// Define o parâmetro bool "malfunction" no Animator, se existir
    /// </summary>
    private void SafeSetMalfunctionBool(Animator anim, bool value)
    {
        if (anim == null) return;

        foreach (var param in anim.parameters)
        {
            if (param.name == "malfunction" && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool("malfunction", value);
                return;
            }
        }
    }

    void Start()
    {
        soundEffects = FindFirstObjectByType<SoundEffects>();
        gameOver = FindFirstObjectByType<GameOver>();

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

        // Cache dos Renderers e smoothness original
        CacheRenderersAndSmoothness();
    }

    /// <summary>
    /// Cachea os valores originais de Smoothness e cor Albedo dos targets configurados
    /// </summary>
    private void CacheRenderersAndSmoothness()
    {
        originalSmoothness.Clear();
        originalAlbedoColors.Clear();

        foreach (var renderer in smoothnessTargets)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                // Guardar smoothness original (propriedade _Glossiness para Standard shader)
                float smoothness = renderer.sharedMaterial.HasProperty("_Glossiness")
                    ? renderer.sharedMaterial.GetFloat("_Glossiness")
                    : 0.5f;
                originalSmoothness.Add(smoothness);

                // Guardar cor Albedo original (propriedade _Color para Standard shader)
                Color albedo = renderer.sharedMaterial.HasProperty("_Color")
                    ? renderer.sharedMaterial.GetColor("_Color")
                    : Color.white;
                originalAlbedoColors.Add(albedo);
            }
            else
            {
                originalSmoothness.Add(0.5f); // Valor padrão
                originalAlbedoColors.Add(Color.white); // Valor padrão
            }
        }
    }

    /// <summary>
    /// Define o parâmetro "malfunction" nos Animators dos smoothnessTargets
    /// </summary>
    private void SetTargetsMalfunctionState(bool value)
    {
        foreach (var renderer in smoothnessTargets)
        {
            if (renderer != null)
            {
                var animator = renderer.GetComponent<Animator>();
                if (animator != null)
                {
                    SafeSetMalfunctionBool(animator, value);
                }
            }
        }
    }

    /// <summary>
    /// Define o Smoothness e cor Albedo de todos os materiais configurados em smoothnessTargets
    /// </summary>
    private void SetMaterialSmoothnessAndColor(float smoothness, Color albedoColor)
    {
        foreach (var renderer in smoothnessTargets)
        {
            if (renderer != null && renderer.material != null)
            {
                // Alterar Smoothness
                if (renderer.material.HasProperty("_Glossiness"))
                {
                    renderer.material.SetFloat("_Glossiness", smoothness);
                }

                // Alterar cor Albedo
                if (renderer.material.HasProperty("_Color"))
                {
                    renderer.material.SetColor("_Color", albedoColor);
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
        for (int i = 0; i < smoothnessTargets.Count; i++)
        {
            if (smoothnessTargets[i] != null && smoothnessTargets[i].material != null)
            {
                // Restaurar Smoothness
                if (i < originalSmoothness.Count && smoothnessTargets[i].material.HasProperty("_Glossiness"))
                {
                    smoothnessTargets[i].material.SetFloat("_Glossiness", originalSmoothness[i]);
                }

                // Restaurar cor Albedo
                if (i < originalAlbedoColors.Count && smoothnessTargets[i].material.HasProperty("_Color"))
                {
                    smoothnessTargets[i].material.SetColor("_Color", originalAlbedoColors[i]);
                }

                // Reativar Animator se existir
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
        // Bloquear clique durante animações de câmera
        if (CameraController.IsAnimating) return;

        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        if (gameObject.CompareTag("Selectable"))
        {
            var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                DebugHelper.Log("Vez de " + player.nickname + " : " + player.GetYourTurn());
                if (player.GetYourTurn())
                {

                    DebugHelper.Log("Number od cards: " + player.GetNumberOfRepairsCards());

                    if(player.GetNumberOfRepairsCards() >= players.Length)
                    {

                        photonView.RPC("RemoveMalfunction", RpcTarget.All);
                        player.RepairComponent(players.Length);
                        DebugHelper.Log("component: " + componentId);

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
                        DebugHelper.Log("You need " + players.Length + " Repair Cards to repair one component!");


                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "ComponentInfoBackground")
                            {
                                info.GetComponentInChildren<TextMeshProUGUI>().text = "You need " + players.Length + " Repair Cards to repair one component!";
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        this.DelayedCall(1.5f, HideComponentInfo);
                    }
                }
            }
        }
        else
        {
            DebugHelper.Log("Voc� j� realizou uma a��o nesse turno");

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
        isProcessingClick = false;
    }

    public void AddMalfunction()
    {
        malfunctions++;

        if (malfunctions >= 2)
        {
            // Componente com 2 malfunctions - ativa fumaça
            DebugHelper.Log($"----> Componente {componentId} com malfunction crítico (2)");
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

            // Definir parâmetro "malfunction" = false nos animators antes de alterar Smoothness
            SetTargetsMalfunctionState(false);

            // Alterar Smoothness para 0 e cor Albedo para #999999
            SetMaterialSmoothnessAndColor(0f, new Color(0.6f, 0.6f, 0.6f, 1f));

            // Desativar animators após 2 segundos
            DisableTargetsAnimatorsWithDelay(2f);

            // Verificar condição de derrota global (3 componentes com malfunction=2)
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.CheckGameOverCondition();
            }
        }
        else
        {
            // Primeiro malfunction - ativa animação e sparks
            DebugHelper.Log($"----> Componente {componentId} com primeiro malfunction");
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
        }

    }

    public void EndGame()
    {
        BackgroundMusic backgroundMusic = FindFirstObjectByType<BackgroundMusic>();
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        gameManager.DeactivateAll();
        gameManager.ResetAllComponents();
        gameManager.ResetAllPlatenames();

        backgroundMusic.PlayGameOverSound();
        gameOver.transform.GetChild(0).gameObject.SetActive(true);
        gameManager.Hud.SetActive(false);
        DebugHelper.Log("name ---> " + gameOver.name);
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

        malfunctions--;
        if (cachedMeshCollider != null)
        {
            cachedMeshCollider.enabled = false;
        }

        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.BlockActions();
        }
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
            cachedAnimator.enabled = false;
        }

        // Resetar Animators filhos
        for (int i = 0; i < cachedChildAnimators.Count; i++)
        {
            if (cachedChildAnimators[i] != null)
            {
                SafeSetMalfunctionBool(cachedChildAnimators[i], false);
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

        // Desabilitar MeshCollider
        if (cachedMeshCollider != null)
        {
            cachedMeshCollider.enabled = false;
        }
    }


}
