using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTag : MonoBehaviour
{

    public SoundEffects soundEffects;

    private void OnMouseDown()
    {
        soundEffects.TagSound();

    }
}
