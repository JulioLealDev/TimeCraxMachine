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

    void Start()
    {

    }

    public void TurnPageSound(int number)
    {
        if (number == 1)
        {
            gameObject.GetComponent<AudioSource>().clip = turnPage01;
            gameObject.GetComponent<AudioSource>().Play();

        }
        else if (number == 2)
        {

            gameObject.GetComponent<AudioSource>().clip = turnPage03;
            gameObject.GetComponent<AudioSource>().Play();

        }
    }

    public void PressButtonSound()
    {
        gameObject.GetComponent<AudioSource>().clip = buttonSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PressHudButtonSound()
    {
        gameObject.GetComponent<AudioSource>().clip = hudbuttonSound;
        gameObject.GetComponent<AudioSource>().Play();
    }
    public void TagSound()
    {
        gameObject.GetComponent<AudioSource>().clip = nameTagSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayRouletteSound()
    {
        gameObject.GetComponent<AudioSource>().clip = rouletteSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayDrawCardSound()
    {
        gameObject.GetComponent<AudioSource>().clip = drawCard;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayRightSlotSound()
    {
        gameObject.GetComponent<AudioSource>().clip = rightSlotSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayWrongSlotSound()
    {
        gameObject.GetComponent<AudioSource>().clip = wrongSlotSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayClickSlotSound()
    {
        gameObject.GetComponent<AudioSource>().clip = clickSlotSound;
        gameObject.GetComponent<AudioSource>().Play();
    }
    public void PlayComponentExplosionSound()
    {
        gameObject.GetComponent<AudioSource>().clip = componentExplosionSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayFinalComponentExplosionSound()
    {
        gameObject.GetComponent<AudioSource>().clip = componentFinalExplosionSound;
        gameObject.GetComponent<AudioSource>().Play();
    }
    public void PlayComponentRepairSound()
    {
        gameObject.GetComponent<AudioSource>().clip = componentRepairSound;
        gameObject.GetComponent<AudioSource>().Play();
    }
}