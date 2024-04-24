using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip timeUntraveledMusic;
    public AudioClip timeEnigmaMusic;
    public AudioClip echoesOfThePastMusic;
    public AudioClip victoryMusic;

    public void PlayGameSound()
    {
        gameObject.GetComponent<AudioSource>().clip = timeEnigmaMusic;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayMenuSound()
    {
        gameObject.GetComponent<AudioSource>().clip = timeUntraveledMusic;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayGameOverSound()
    {
        gameObject.GetComponent<AudioSource>().clip = echoesOfThePastMusic;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void PlayVictorySound()
    {
        gameObject.GetComponent<AudioSource>().clip = victoryMusic;
        gameObject.GetComponent<AudioSource>().Play();
    }
}
