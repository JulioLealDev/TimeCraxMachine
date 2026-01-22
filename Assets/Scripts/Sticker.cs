using UnityEngine;
using TimeCrax.Core;

public class Sticker : MonoBehaviour
{
    public SoundEffects soundEffects;
    public Menu menu;
    public Configurations configurations;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    private void OnMouseDown()
    {
        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        DebugHelper.Log("Clicou no sticker");
        menu.DisableMenu();
        configurations.SetDefaultSlidersValues();
        configurations.gameObject.SetActive(true);
        soundEffects.TagSound();

        // Resetar após um pequeno delay
        this.DelayedCall(0.5f, ResetClickProtection);
    }

    private void ResetClickProtection()
    {
        isProcessingClick = false;
    }
}
