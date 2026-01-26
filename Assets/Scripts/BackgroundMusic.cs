using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioClip timeUntraveledMusic;
    [SerializeField] private AudioClip timeEnigmaMusic;
    [SerializeField] private AudioClip echoesOfThePastMusic;
    [SerializeField] private AudioClip victoryMusic;

    private AudioSource cachedAudioSource;

    private void Awake()
    {
        cachedAudioSource = GetComponent<AudioSource>();
    }

    public void PlayGameSound()
    {
        cachedAudioSource.clip = timeEnigmaMusic;
        cachedAudioSource.Play();
    }

    public void PlayMenuSound()
    {
        cachedAudioSource.clip = timeUntraveledMusic;
        cachedAudioSource.Play();
    }

    public void PlayGameOverSound()
    {
        cachedAudioSource.clip = echoesOfThePastMusic;
        cachedAudioSource.Play();
    }

    public void PlayVictorySound()
    {
        cachedAudioSource.clip = victoryMusic;
        cachedAudioSource.Play();
    }
}
