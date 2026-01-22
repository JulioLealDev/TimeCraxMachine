using UnityEngine;
using TimeCrax.Core;

public class Tutorial : MonoBehaviour
{
    public Canvas canvas;
    public SoundEffects soundEffects;
    public Menu menu;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    private void OnMouseDown()
    {
        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        DebugHelper.Log("Clicou no tutorial");
        soundEffects.TurnPageSound(1);
        canvas.gameObject.SetActive(true);
        menu.DisableMenu();

        // Resetar após um delay
        this.DelayedCall(0.5f, ResetClickProtection);
    }

    private void ResetClickProtection()
    {
        isProcessingClick = false;
    }
}
