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
        DebugHelper.Log("Clicou na foto");

        menu.DisableMenu();

        themeSelectionUI.gameObject.SetActive(true);
        themeSelectionUI.Show();

        soundEffects.TagSound();
    }

    private void OnThemeSelectionClosed()
    {
        menu.EnableMenu();
        DebugHelper.Log("Menu reativado após fechar seleção de temas");
    }
}
