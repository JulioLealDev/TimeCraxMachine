using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Managers;

public class Configurations : MonoBehaviour
{
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private BackgroundMusic backgroundMusic;
    [SerializeField] private GameObject soundEffectsSlider;
    [SerializeField] private GameObject backgroundMusicSlider;
    [SerializeField] private SuitTop suitTop;
    [SerializeField] private MenuManager menuManager;


    private float soundEffectsSliderDefault;
    private float backgroundMusicSliderDefault;

    public void SetDefaultSlidersValues()
    {
        soundEffectsSliderDefault = soundEffectsSlider.GetComponent<Slider>().value;
        backgroundMusicSliderDefault = backgroundMusicSlider.GetComponent<Slider>().value;
    }
    public void CancelButtton()
    {
        soundEffects.PressHudButtonSound();

        soundEffectsSlider.GetComponent<Slider>().value = soundEffectsSliderDefault;
        backgroundMusicSlider.GetComponent<Slider>().value = backgroundMusicSliderDefault;
        menuManager.EnablingMenuOptions();
        gameObject.SetActive(false);
    }

    public void ApplyButtton()
    {
        soundEffects.PressHudButtonSound();

        soundEffects.GetComponent<AudioSource>().volume = soundEffectsSlider.GetComponent<Slider>().value;
        backgroundMusic.GetComponent<AudioSource>().volume = backgroundMusicSlider.GetComponent<Slider>().value;
        menuManager.EnablingMenuOptions();
        gameObject.SetActive(false);
    }
}
