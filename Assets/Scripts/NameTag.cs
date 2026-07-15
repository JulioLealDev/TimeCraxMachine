using UnityEngine;
using TimeCrax.Core;

public class NameTag : MonoBehaviour
{
    [SerializeField] private SoundEffects soundEffects;

    private void OnMouseDown()
    {
        if (!GameManager.TryBeginClick(this)) return;

        soundEffects.TagSound();

        this.DelayedCall(0.3f, () => GameManager.ResetClick(this));
    }
}
