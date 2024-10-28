using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public bool exitGame = false;
    public bool gameIsOver = false;
    public GameObject gameOverImage;
    public SoundEffects soundEffects;
    public GameManager gameManager;

    public void QuitGame()
    {
        gameOverImage.SetActive(false);
        soundEffects.PressHudButtonSound();

        exitGame = true;
        Debug.Log("exiteGame: " + exitGame);

        gameManager.BackToMenu();
    }


    public void BackToMenu()
    {
        gameOverImage.SetActive(false);
        soundEffects.PressHudButtonSound();

        gameManager.BackToMenu();
    }
}
