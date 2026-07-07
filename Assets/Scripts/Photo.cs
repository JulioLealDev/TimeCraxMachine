using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Themes;

public class Photo : MonoBehaviour
{
    public Menu menu;
    public ThemeSelectionUI themeSelectionUI;
    public SoundEffects soundEffects;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    private void Start()
    {
        if (themeSelectionUI != null)
        {
            themeSelectionUI.OnPanelClosed += OnThemeSelectionClosed;
        }
    }

    private void OnDestroy()
    {
        if (themeSelectionUI != null)
        {
            themeSelectionUI.OnPanelClosed -= OnThemeSelectionClosed;
        }
    }

    private void OnMouseDown()
    {
        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        DebugHelper.Log("Clicou na foto");

        menu.DisableMenu();

        themeSelectionUI.gameObject.SetActive(true);
        themeSelectionUI.Show();

        soundEffects.TagSound();
    }

    private void OnThemeSelectionClosed()
    {
        menu.EnableMenu();
        isProcessingClick = false;
        DebugHelper.Log("Menu reativado após fechar seleção de temas");
    }
}
