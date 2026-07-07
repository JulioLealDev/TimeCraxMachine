using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    [Header("UI Sounds")]
    [SerializeField] private AudioClip turnPage01;
    [SerializeField] private AudioClip turnPage02;
    [SerializeField] private AudioClip turnPage03;
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip hudbuttonSound;
    [SerializeField] private AudioClip nameTagSound;

    [Header("Game Sounds")]
    [SerializeField] private AudioClip rouletteSound;
    [SerializeField] private AudioClip drawCard;
    [SerializeField] private AudioClip rightSlotSound;
    [SerializeField] private AudioClip wrongSlotSound;
    [SerializeField] private AudioClip clickSlotSound;

    [Header("Component Sounds")]
    [SerializeField] private AudioClip componentExplosionSound;
    [SerializeField] private AudioClip componentFinalExplosionSound;
    [SerializeField] private AudioClip componentRepairSound;

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