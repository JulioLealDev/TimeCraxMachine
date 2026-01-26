using UnityEngine;
using TimeCrax.Core;

public class NameTag : MonoBehaviour
{
    [SerializeField] private SoundEffects soundEffects;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    private void OnMouseDown()
    {
        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        soundEffects.TagSound();

        // Resetar após um pequeno delay para evitar som duplo
        this.DelayedCall(0.3f, ResetClickProtection);
    }

    private void ResetClickProtection()
    {
        isProcessingClick = false;
    }
}
