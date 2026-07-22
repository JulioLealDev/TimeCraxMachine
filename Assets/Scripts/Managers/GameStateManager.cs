using System;
using UnityEngine;

public enum GamePhase
{
    Menu,
    ExitingGame,
    GameOver,
    Victory,
    In_Match,
    IM_FirstTurn,
    IM_DrewEventCard,
    IM_Turn,
    IM_MapChallenge,
    IM_PersonsChallenge,
    IM_ChallengeFeedback,
    IM_MalfunctionRoulette,
    IM_UnlockBonusDeck,
    IM_DrewBonusCard,
    IM_CheckingSlot,
    IM_ChoosingSlot,
    IM_KillingOption,

}

public static class GameStateManager
{
    public static GamePhase CurrentPhase { get; private set; } = GamePhase.Menu;

    public static event Action<GamePhase, GamePhase> OnPhaseChanged;

    public static void TransitionTo(GamePhase next)
    {
        if (CurrentPhase == next) return;

        GamePhase previous = CurrentPhase;
        CurrentPhase = next;

        OnPhaseChanged?.Invoke(previous, next);
    }

    public static bool Is(GamePhase phase) => CurrentPhase == phase;

    public static bool IsAnyOf(params GamePhase[] phases)
    {
        foreach (var p in phases)
            if (CurrentPhase == p) return true;
        return false;
    }
}
