using UnityEngine;
using TimeCrax.Core;

public class Sticker : MonoBehaviour
{
    public SoundEffects soundEffects;
    public Menu menu;
    public Configurations configurations;

    private void OnMouseDown()
    {
        DebugHelper.Log("Clicou no sticker");
        menu.DisableMenu();
        configurations.SetDefaultSlidersValues();
        configurations.gameObject.SetActive(true);
        soundEffects.TagSound();
    }
}
