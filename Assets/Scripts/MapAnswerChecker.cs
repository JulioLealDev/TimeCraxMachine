using System.Collections;
using UnityEngine;
using Photon.Pun;
using TimeCrax.Core;
using TimeCrax.Themes;
using TimeCrax.Managers;

public class MapAnswerChecker : MonoBehaviour
{
    public static MapAnswerChecker Instance { get; private set; }

    [Header("Superfície do Mapa")]
    [SerializeField] private Renderer mapRenderer;

    [Header("MapPins")]
    [SerializeField] private Transform mapPin01;
    [SerializeField] private Transform mapPin02;
    [SerializeField] private Transform mapPin03;
    [SerializeField] private Transform mapPin04;

    [Header("ResetMapFrame()")]
    [SerializeField] private GameObject map;

    [Header("Hover dos Pins")]
    [SerializeField] private Material pinHoverMaterial;

    [Header("Ícones de resultado")]
    [SerializeField] private GameObject correctIcon;
    [SerializeField] private GameObject incorrectIcon;

    private int correctPinIndex;
    private int currentSlotCount;
    private bool answered;

    private void Awake()
    {
        Instance = this;
        HideIcons();
    }

    private void OnEnable()
    {
        Debug.Log("[MapAnswerChecker] OnEnable");
        answered = false;
        HideIcons();
        Setup();
    }

    private void Setup()
    {
        Debug.Log("[MapAnswerChecker] Setup iniciado");

        int targetSlot = GameManager.CurrentPersonsSlotCount;
        Debug.Log($"[MapAnswerChecker] Buscando EventCard com slotCount={targetSlot}");

        EventCard drawnCard = null;
        foreach (var card in FindObjectsByType<EventCard>(FindObjectsSortMode.None))
        {
            if (card.slotCount == targetSlot)
            {
                drawnCard = card;
                break;
            }
        }

        if (drawnCard == null)
        {
            Debug.LogWarning($"[MapAnswerChecker] Nenhuma EventCard com slotCount={targetSlot} encontrada. Desativando Map.");
            gameObject.SetActive(false);
            return;
        }

        Debug.Log($"[MapAnswerChecker] EventCard encontrada: {drawnCard.name}, tag={drawnCard.tag}");

        var mapData = drawnCard.GetThemeCard()?.map;
        if (mapData == null)
        {
            Debug.LogWarning("[MapAnswerChecker] ThemeCard não tem dados de mapa (map == null). Desativando Map.");
            gameObject.SetActive(false);
            return;
        }

        Debug.Log($"[MapAnswerChecker] mapData OK — pins={mapData.pins?.Count}, correctPinIndex={mapData.correctPinIndex}, localImagePath={mapData.localImagePath}");

        currentSlotCount = drawnCard.slotCount;
        correctPinIndex  = mapData.correctPinIndex;

        if (mapRenderer != null)
        {
            var texture = ThemeStorage.LoadLocalImage(mapData.localImagePath);
            if (texture != null)
            {
                mapRenderer.material.mainTexture = texture;
                Debug.Log("[MapAnswerChecker] Textura do mapa aplicada com sucesso");
            }
            else
                Debug.LogWarning($"[MapAnswerChecker] Falha ao carregar textura: {mapData.localImagePath}");
        }
        else
            Debug.LogWarning("[MapAnswerChecker] mapRenderer é null — verifique o Inspector");

        Transform[] pins = { mapPin01, mapPin02, mapPin03, mapPin04 };

        for (int i = 0; i < pins.Length; i++)
        {
            if (pins[i] == null)
            {
                Debug.LogWarning($"[MapAnswerChecker] mapPin{i + 1:00} é null no Inspector");
                continue;
            }

            if (mapData.pins != null && i < mapData.pins.Count)
            {
                var pinData = mapData.pins[i];
                var localPos  = pins[i].localPosition;
                localPos.x    = pinData.x;
                localPos.z    = pinData.y;
                pins[i].localPosition = localPos;

                pins[i].gameObject.SetActive(true);
                pins[i].tag = "Selectable";

                //var col = pins[i].GetComponent<Collider>();
                //if (col != null) col.enabled = false;
                //else Debug.LogWarning($"[MapAnswerChecker] MapPin{i + 1:00} não tem Collider!");

                var hover = pins[i].GetComponent<HoverMaterialAdder>();
                if (hover == null) hover = pins[i].gameObject.AddComponent<HoverMaterialAdder>();
                hover.SetMaterial(pinHoverMaterial);

                var click = pins[i].GetComponent<MapPinClick>();
                if (click == null) click = pins[i].gameObject.AddComponent<MapPinClick>();
                click.PinIndex = i;

                Debug.Log($"[MapAnswerChecker] Pin{i} posicionado em localPos=({localPos.x:F2}, {localPos.z:F2}), correto={i == correctPinIndex}");
            }
            else
            {
                pins[i].gameObject.SetActive(false);
                Debug.Log($"[MapAnswerChecker] Pin{i} desativado (sem dados no manifest)");
            }
        }

        Debug.Log("[MapAnswerChecker] Setup concluído — aguardando clique do jogador");
    }

    public void OnPinClicked(int pinIndex)
    {
        if (answered) return;
        answered = true;

        Debug.Log($"[MapAnswerChecker] Pin clicado: index={pinIndex}, correto={pinIndex == correctPinIndex}");

        GameStateManager.TransitionTo(GamePhase.IM_ChallengeFeedback);
        InputBlocker.Block();
        Cursor.visible = true;

        Transform[] pins = { mapPin01, mapPin02, mapPin03, mapPin04 };
        foreach (var pin in pins)
        {
            if (pin != null)
            {
                pin.tag = "Untagged";
                pin.GetComponent<HoverMaterialAdder>()?.HideHover();
                var col = pin.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        StartCoroutine(RevealFeedbackSequence(pinIndex));
    }

    private IEnumerator RevealFeedbackSequence(int clickedIndex)
    {
        Debug.Log("[MapAnswerChecker] RevealFeedbackSequence iniciado — aguardando 1s");
        yield return new WaitForSeconds(1f);

        bool isCorrect = clickedIndex == correctPinIndex;
        Debug.Log($"[MapAnswerChecker] Feedback: isCorrect={isCorrect}");
        correctIcon?.SetActive(isCorrect);
        incorrectIcon?.SetActive(!isCorrect);

        yield return new WaitForSeconds(2f);

        var gameManager = FindFirstObjectByType<GameManager>();
        Debug.Log($"[MapAnswerChecker] gameManager encontrado={gameManager != null}");

        var local = PlayerManager.Instance?.GetLocalPlayer();
        if (local != null)
        {
            if (gameManager != null && PhotonNetwork.InRoom)
                gameManager.photonView.RPC("RPC_TrackMapChallenge", RpcTarget.All, isCorrect, local.actorNumber, local.nickname);
            else if (isCorrect) MatchStats.AddChallengeCorrect(local.actorNumber, local.nickname);
            else MatchStats.AddMapError(local.actorNumber, local.nickname);
        }

        if (!isCorrect)
        {
            if (gameManager != null && PhotonNetwork.InRoom)
            {
                Debug.Log($"[MapAnswerChecker] Enviando RPC_HandleMapWrong para slotCount={currentSlotCount}");
                gameManager.photonView.RPC("RPC_HandleMapWrong", RpcTarget.All, currentSlotCount);
            }
            else
            {
                Debug.Log($"[MapAnswerChecker] Chamando HandleMapWrong local para slotCount={currentSlotCount}");
                gameManager?.HandleMapWrong(currentSlotCount);
            }
        }

        // Fecha a NewTimeline
        gameManager?.CloseNewTimeline();

        Debug.Log("[MapAnswerChecker] Aguardando 2.5s antes de ResetMapFrame");
        yield return new WaitForSeconds(2.5f);

        Debug.Log("[MapAnswerChecker] Chamando ResetState e ResetMapFrame");
        ResetState();
        ResetMapFrame(gameManager);
    }

    private void ResetMapFrame(GameManager gameManager)
    {
        Transform[] pins = { mapPin01, mapPin02, mapPin03, mapPin04 };
        foreach (var pin in pins)
        {
            if (pin == null) continue;
            var col = pin.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (map != null) map.SetActive(false);
        EventSlot.ResetClickProtection();

        if (!GameManager.IsInTurnTransition)
            gameManager?.DelayedCall(0.5f, gameManager.MapZoomOut);
        gameManager?.CheckWinAfterMiniGame();
    }

    private void ResetState()
    {
        answered = false;
        HideIcons();

        Transform[] pins = { mapPin01, mapPin02, mapPin03, mapPin04 };
        foreach (var pin in pins)
        {
            if (pin != null) pin.tag = "Untagged";
        }
    }

    private void HideIcons()
    {
        correctIcon?.SetActive(false);
        incorrectIcon?.SetActive(false);
    }

    public void DisableInteraction()
    {
        Transform[] pins = { mapPin01, mapPin02, mapPin03, mapPin04 };
        foreach (var pin in pins)
        {
            if (pin == null) continue;
            pin.tag = "Untagged";
            pin.GetComponent<HoverMaterialAdder>()?.HideHover();
            var col = pin.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    public void EnableInteraction()
    {
        Transform[] pins = { mapPin01, mapPin02, mapPin03, mapPin04 };
        foreach (var pin in pins)
        {
            if (pin == null || !pin.gameObject.activeSelf) continue;
            var col = pin.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }
    }

    public void ApplyKillChallengeOption()
    {
        Transform[] allPins = { mapPin01, mapPin02, mapPin03, mapPin04 };
        var activePins   = new System.Collections.Generic.List<Transform>();
        var incorrectPins = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < allPins.Length; i++)
        {
            if (allPins[i] == null || !allPins[i].gameObject.activeSelf) continue;
            activePins.Add(allPins[i]);
            if (i != correctPinIndex) incorrectPins.Add(allPins[i]);
        }
        if (incorrectPins.Count == 0) return;

        Transform target = incorrectPins[UnityEngine.Random.Range(0, incorrectPins.Count)];
        StartCoroutine(KillPinRoulette(activePins, target));
    }

    private IEnumerator KillPinRoulette(System.Collections.Generic.List<Transform> candidates, Transform target)
    {
        var sound = FindFirstObjectByType<SoundEffects>();
        int lastIdx = -1;
        float interval = 0.3f;

        // Desativar colliders durante a roulette
        foreach (var pin in candidates)
        {
            var col = pin.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        for (int cond = 0; cond < 15; cond++)
        {
            int idx = UnityEngine.Random.Range(0, candidates.Count);
            while (idx == lastIdx && candidates.Count > 1)
                idx = UnityEngine.Random.Range(0, candidates.Count);
            lastIdx = idx;

            var hover = candidates[idx].GetComponent<HoverMaterialAdder>();
            hover?.ShowHover();
            sound?.PlayRouletteSound();
            yield return new WaitForSeconds(interval);
            hover?.HideHover();

            interval -= 0.015f;
        }

        // Destaque final no alvo
        var finalHover = target.GetComponent<HoverMaterialAdder>();
        finalHover?.ShowHover();
        sound?.PlayRouletteSound();
        yield return new WaitForSeconds(interval);
        finalHover?.HideHover();

        target.gameObject.SetActive(false);

        // Reativar colliders dos pins restantes
        foreach (var pin in candidates)
        {
            if (pin == target) continue;
            var col = pin.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }
    }
}
