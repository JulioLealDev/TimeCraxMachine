using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    public AudioClip turnPage01;
    public AudioClip turnPage02;
    public AudioClip turnPage03;
    public AudioClip buttonSound;
    public AudioClip hudbuttonSound;
    public AudioClip nameTagSound;
    public AudioClip rouletteSound;
    public AudioClip drawCard;
    public AudioClip rightSlotSound;
    public AudioClip wrongSlotSound;
    public AudioClip clickSlotSound;
    public AudioClip componentExplosionSound;
    public AudioClip componentFinalExplosionSound;
    public AudioClip componentRepairSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void TurnPageSound(int number)
    {
        if (number == 1)
        {
            audioSource.PlayOneShot(turnPage01);
        }
        else if (number == 2)
        {
            audioSource.PlayOneShot(turnPage03);
        }
    }

    public void PressButtonSound()
    {
        audioSource.PlayOneShot(buttonSound);
    }

    public void PressHudButtonSound()
    {
        audioSource.PlayOneShot(hudbuttonSound);
    }

    public void TagSound()
    {
        audioSource.PlayOneShot(nameTagSound);
    }

    public void PlayRouletteSound()
    {
        audioSource.PlayOneShot(rouletteSound);
    }

    public void PlayDrawCardSound()
    {
        audioSource.PlayOneShot(drawCard);
    }

    public void PlayRightSlotSound()
    {
        audioSource.PlayOneShot(rightSlotSound);
    }

    public void PlayWrongSlotSound()
    {
        audioSource.PlayOneShot(wrongSlotSound);
    }

    public void PlayClickSlotSound()
    {
        audioSource.PlayOneShot(clickSlotSound);
    }

    public void PlayComponentExplosionSound()
    {
        audioSource.PlayOneShot(componentExplosionSound);
    }

    public void PlayFinalComponentExplosionSound()
    {
        audioSource.PlayOneShot(componentFinalExplosionSound);
    }

    public void PlayComponentRepairSound()
    {
        audioSource.PlayOneShot(componentRepairSound);
    }
}