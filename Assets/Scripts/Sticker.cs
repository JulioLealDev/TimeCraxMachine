using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Managers;

public class Sticker : MonoBehaviour
{
    public SoundEffects soundEffects;
    public MenuManager menuManager;
    public Configurations configurations;

    private void OnMouseDown()
    {
        if (!GameManager.TryBeginClick(this)) return;

        menuManager.DesablingMenuOptions();
        soundEffects.TagSound();

        configurations.SetDefaultSlidersValues();
        configurations.gameObject.SetActive(true);

        this.DelayedCall(0.5f, () => GameManager.ResetClick(this));
    }
}
