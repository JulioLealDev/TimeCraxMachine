using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Managers;

public class Tutorial : MonoBehaviour
{
    public Canvas tutorialCanvas;
    public SoundEffects soundEffects;
    public MenuManager menuManager;

    private void OnMouseDown()
    {
        if (!GameManager.TryBeginClick(this)) return;

        soundEffects.TurnPageSound(1);
        tutorialCanvas.gameObject.SetActive(true);
        menuManager.DesablingMenuOptions();

        this.DelayedCall(0.5f, () => GameManager.ResetClick(this));
    }
}
