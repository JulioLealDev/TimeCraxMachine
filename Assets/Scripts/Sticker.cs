using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : MonoBehaviour
{
    //public Canvas configCanvas;
    public Canvas inputName;
    public SoundEffects soundEffects;
    public Menu menu;
    public Configurations configurations;

    private void OnMouseDown()
    {
        Debug.Log("Clicou no sticker");

        menu.DisableMenu();

        configurations.SetDefaultSlidersValues();

        configurations.gameObject.SetActive(true);
        inputName.gameObject.SetActive(false);

        soundEffects.TagSound();

    }
}
