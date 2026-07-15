using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Themes;
using TimeCrax.Managers;

public class Photo : MonoBehaviour
{
    
    public MenuManager menuManager;
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
        if (!GameManager.TryBeginClick(this)) return;


        menuManager.DesablingMenuOptions();
        themeSelectionUI.gameObject.SetActive(true);
        
        themeSelectionUI.Show();
        soundEffects.TagSound();
    }

    private void OnThemeSelectionClosed()
    {
        menuManager.EnablingMenuOptions();
        GameManager.ResetClick(this);
    }
}
