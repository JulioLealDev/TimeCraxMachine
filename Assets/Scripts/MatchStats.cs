using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coleta estatísticas da partida por jogador.
/// Todos os clientes computam os mesmos dados via RPCs e GetCurrentTurnPlayer.
/// </summary>
public static class MatchStats
{
    public class PlayerData
    {
        public int    actorNumber;
        public string nickname;
        public int    slotErrors;
        public int    mapErrors;
        public int    personsErrors;
        public int    mapChallengesCorrect;
        public int    personsChallengesCorrect;
        public int    slotsCorrect;
        public int    bonusCardsObtained;
        public int    bonusCardsUsed;
        public int    malfunctionsTriggered;
        public int    componentsRepaired;
        public int    score;
    }

    private static readonly Dictionary<int, PlayerData> _players = new();
    private static float _startTime;

    public static float TotalTimeSeconds { get; private set; }
    public static IEnumerable<PlayerData> AllPlayers => _players.Values;

    public static void Reset()
    {
        _players.Clear();
        TotalTimeSeconds = 0f;
        _startTime = 0f;
    }

    public static void StartTimer() => _startTime = Time.realtimeSinceStartup;

    public static void StopTimer()
    {
        if (_startTime > 0f)
            TotalTimeSeconds = Time.realtimeSinceStartup - _startTime;
    }

    public static PlayerData GetOrCreate(int actorNumber, string nickname = "")
    {
        if (!_players.TryGetValue(actorNumber, out var d))
        {
            d = new PlayerData { actorNumber = actorNumber, nickname = nickname };
            _players[actorNumber] = d;
        }
        if (!string.IsNullOrEmpty(nickname) && string.IsNullOrEmpty(d.nickname))
            d.nickname = nickname;
        return d;
    }

    public static void AddSlotError(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).slotErrors++;

    public static void AddMapError(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).mapErrors++;

    public static void AddPersonsError(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).personsErrors++;

    public static void AddMapChallengeCorrect(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).mapChallengesCorrect++;

    public static void AddPersonsChallengeCorrect(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).personsChallengesCorrect++;

    public static void AddSlotCorrect(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).slotsCorrect++;

    public static void AddBonusCardObtained(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).bonusCardsObtained++;

    public static void AddBonusCardUsed(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).bonusCardsUsed++;

    public static void AddMalfunction(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).malfunctionsTriggered++;

    public static void AddRepair(int actorNumber, string nickname = "")
        => GetOrCreate(actorNumber, nickname).componentsRepaired++;

    public static void CalculateScores()
    {
        foreach (var d in _players.Values)
        {
            d.score = (50 + d.slotsCorrect + 2 * d.mapChallengesCorrect + 3 * d.personsChallengesCorrect) - (d.slotErrors  + d.mapErrors  + d.personsErrors);
        }
    }
}
