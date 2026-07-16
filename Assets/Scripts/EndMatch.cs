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
            endMatchScreenTitle.text = "YOU WIN";
            //if (endMatchScreenImage != null) endMatchScreenImage.sprite = victorySprite;
        }
        else if (GameStateManager.Is(GamePhase.GameOver))
        {
            Debug.Log("[EndMatch] GameState GameOver");
            endMatchScreenTitle.text = "YOU LOSE";
            //if (endMatchScreenImage != null) endMatchScreenImage.sprite = gameOverSprite;
        }
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
