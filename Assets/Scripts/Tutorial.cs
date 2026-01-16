using UnityEngine;
using TimeCrax.Core;

public class Tutorial : MonoBehaviour
{
    public Canvas canvas;
    public SoundEffects soundEffects;
    public Menu menu;

    private void OnMouseDown()
    {
        DebugHelper.Log("Clicou no tutorial");
        soundEffects.TurnPageSound(1);
        canvas.gameObject.SetActive(true);
        menu.DisableMenu();
    }
}
