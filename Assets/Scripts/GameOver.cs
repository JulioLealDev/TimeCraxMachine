using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeCrax.Core;

public class GameOver : MonoBehaviour
{
    //public bool gameIsOver = false;
    public GameObject gameOverImage;
    public SoundEffects soundEffects;
    public GameManager gameManager;

    public void QuitGame()
    {
        GameStateManager.TransitionTo(GamePhase.ExitingGame);
        BackToMenu();
    }


    public void BackToMenu()
    {
        gameOverImage.SetActive(false);
        soundEffects.PressHudButtonSound();

        gameManager.BackToMenu();
    }
}
