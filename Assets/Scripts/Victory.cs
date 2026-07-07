using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victory : MonoBehaviour
{
    public SoundEffects soundEffects;
    public GameManager gameManager;
    public GameObject victoryImage;

    public void BackToMenu()
    {
        victoryImage.SetActive(false);
        soundEffects.PressHudButtonSound();

        gameManager.BackToMenu();
    }
}
