using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;
using TMPro;

public class EndMatch : MonoBehaviour
{
    //public bool gameIsOver = false;
    public SoundEffects soundEffects;
    public GameManager gameManager;
    public TMP_Text endMatchScreenTitle;
    public Image endMatchScreenImage;
    public Sprite victorySprite;
    public Sprite gameOverSprite;


    public void UpdateTitle()
    {
        Debug.Log("[EndMatch] UpdateTitle: "+ GameStateManager.CurrentPhase);
        if (GameStateManager.Is(GamePhase.Victory))
        {
            Debug.Log("[EndMatch] GameState Victory");
            endMatchScreenTitle.text = "VICTORY";
            //if (endMatchScreenImage != null) endMatchScreenImage.sprite = victorySprite;
        }
        else if (GameStateManager.Is(GamePhase.GameOver))
        {
            Debug.Log("[EndMatch] GameState GameOver");
            endMatchScreenTitle.text = "GAMEOVER";
            //if (endMatchScreenImage != null) endMatchScreenImage.sprite = gameOverSprite;
        }

        ShowStats();
    }

    private void ShowStats()
    {

        var sb = new System.Text.StringBuilder();

        int totalSeconds = (int)MatchStats.TotalTimeSeconds;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        sb.AppendLine($"Tempo de partida: {minutes:00}:{seconds:00}");
        sb.AppendLine();

        foreach (var p in MatchStats.AllPlayers)
        {
            string name = string.IsNullOrEmpty(p.nickname) ? $"Jogador {p.actorNumber}" : p.nickname;
            sb.AppendLine($"— {name} —");
            sb.AppendLine($"  Slots: {p.slotsCorrect} acertos | {p.slotErrors} erros");
            sb.AppendLine($"  Mapa: {p.mapChallengesCorrect} acertos | {p.mapErrors} erros");
            sb.AppendLine($"  Pessoas: {p.personsChallengesCorrect} acertos | {p.personsErrors} erros");
            sb.AppendLine($"  Cartas Bônus: {p.bonusCardsObtained} obtidas | {p.bonusCardsUsed} usadas");
            sb.AppendLine($"  Malfunctions: {p.malfunctionsTriggered} | Consertos: {p.componentsRepaired}");
            sb.AppendLine();
        }


        PopulatePlayerStatsUI();
    }

    private void PopulatePlayerStatsUI()
    {
        if (endMatchScreenImage == null) return;

        var root = endMatchScreenImage.transform;
        var statsValues = root.Find("StatsValues");

        var localPlayer = Photon.Pun.PhotonNetwork.IsConnected
            ? Photon.Pun.PhotonNetwork.LocalPlayer
            : null;

        MatchStats.PlayerData data = null;
        foreach (var p in MatchStats.AllPlayers)
        {
            if (localPlayer == null || p.actorNumber == localPlayer.ActorNumber)
            {
                data = p;
                break;
            }
        }

        if (data == null) return;

        string playerName = string.IsNullOrEmpty(data.nickname) ? $"Jogador {data.actorNumber}" : data.nickname;
        SetText(root.Find("PlayerName"), playerName);

        if (statsValues != null)
        {
            SetText(statsValues.Find("SlotCorrectValue"),            data.slotsCorrect.ToString());
            SetText(statsValues.Find("SlotsIncorrectValue"),         data.slotErrors.ToString());
            SetText(statsValues.Find("PersonsCorrectValue"),         data.personsChallengesCorrect.ToString());
            SetText(statsValues.Find("PersonsIncorrectValue"),       data.personsErrors.ToString());
            SetText(statsValues.Find("MapsCorrectValue"),            data.mapChallengesCorrect.ToString());
            SetText(statsValues.Find("MapsIncorrectValue"),          data.mapErrors.ToString());
            SetText(statsValues.Find("ComponentsRepairedValue"),     data.componentsRepaired.ToString());
            SetText(statsValues.Find("ComponentsMalfunctionedValue"),data.malfunctionsTriggered.ToString());
        }
    }

    private void SetText(Transform t, string value)
    {
        if (t == null) return;
        var tmp = t.GetComponent<TMP_Text>();
        if (tmp != null) tmp.text = value;
    }

    public void QuitGame()
    {
        GameStateManager.TransitionTo(GamePhase.ExitingGame);
        BackToMenu();
    }

    public void BackToMenu()
    {
        if (endMatchScreenImage != null) endMatchScreenImage.gameObject.SetActive(false);

        soundEffects.PressHudButtonSound();

        GameStateManager.TransitionTo(GamePhase.Menu);
        gameManager.BackToMenu();
    }
}
