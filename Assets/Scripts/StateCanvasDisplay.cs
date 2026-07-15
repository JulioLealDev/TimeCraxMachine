using UnityEngine;
using TMPro;

public class StateCanvasDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text stateText;

    private void OnEnable()
    {
        GameStateManager.OnPhaseChanged += OnPhaseChanged;
        Refresh(GameStateManager.CurrentPhase, GameStateManager.CurrentPhase);
    }

    private void OnDisable()
    {
        GameStateManager.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(GamePhase previous, GamePhase current)
    {
        Refresh(current, previous);
    }

    private void Refresh(GamePhase current, GamePhase previous)
    {
        if (stateText != null)
            stateText.text = $"Atual: {current} - Anterior: {previous}";
    }
}
