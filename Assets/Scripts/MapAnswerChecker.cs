using System.Collections;
using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Themes;

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

                var outline = pins[i].GetComponent<OutlineComponent>();
                if (outline == null) outline = pins[i].gameObject.AddComponent<OutlineComponent>();
                outline.enabled = false;

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
                var outline = pin.GetComponent<OutlineComponent>();
                if (outline != null) outline.enabled = false;
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

        if (!isCorrect)
        {
            Debug.Log($"[MapAnswerChecker] Chamando HandleMapWrong para slotCount={currentSlotCount}");
            gameManager?.HandleMapWrong(currentSlotCount);
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
}
