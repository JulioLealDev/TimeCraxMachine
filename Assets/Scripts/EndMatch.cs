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

    [Header("Estatísticas")]
    [SerializeField] private TMP_Text statsText;

    public void UpdateTitle()
    {
        Debug.Log("[EndMatch] UpdateTitle: "+ GameStateManager.CurrentPhase);
        if (GameStateManager.Is(GamePhase.Victory))
        {
            Debug.Log("[EndMatch] GameState Victory");
            endMatchScreenTitle.text = "YOU WIN";
            //if (endMatchScreenImage != null) endMatchScreenImage.sprite = victorySprite;
        }
        else if (GameStateManager.Is(GamePhase.GameOver))
        {
            Debug.Log("[EndMatch] GameState GameOver");
            endMatchScreenTitle.text = "YOU LOSE";
            //if (endMatchScreenImage != null) endMatchScreenImage.sprite = gameOverSprite;
        }

        ShowStats();
    }

    private void ShowStats()
    {
        if (statsText == null) return;

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
            sb.AppendLine($"  Acertos:  Slots {p.slotsCorrect} | Desafios {p.challengesCorrect}");
            sb.AppendLine($"  Erros:    Slot {p.slotErrors} | Mapa {p.mapErrors} | Pessoas {p.personsErrors}");
            sb.AppendLine($"  Cartas Bônus:  {p.bonusCardsObtained} obtidas | {p.bonusCardsUsed} usadas");
            sb.AppendLine($"  Malfunctions: {p.malfunctionsTriggered} | Consertos: {p.componentsRepaired}");
            sb.AppendLine();
        }

        statsText.text = sb.ToString();
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
