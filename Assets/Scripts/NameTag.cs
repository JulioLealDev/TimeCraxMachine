using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTag : MonoBehaviour
{

    public SoundEffects soundEffects;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    private void OnMouseDown()
    {
        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        soundEffects.TagSound();

        // Resetar após um pequeno delay para evitar som duplo
        Invoke(nameof(ResetClickProtection), 0.3f);
    }

    private void ResetClickProtection()
    {
        isProcessingClick = false;
    }
}
