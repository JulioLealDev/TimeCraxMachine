using UnityEngine;
using UnityEngine.UI;

public class Configurations : MonoBehaviour
{
    public SoundEffects soundEffects;
    public BackgroundMusic backgroundMusic;
    public GameObject soundEffectsSlider;
    public GameObject backgroundMusicSlider;
    public Menu menu;
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
        menu.EnableMenu();
        gameObject.SetActive(false);
    }

    public void ApplyButtton()
    {
        soundEffects.PressHudButtonSound();

        soundEffects.GetComponent<AudioSource>().volume = soundEffectsSlider.GetComponent<Slider>().value;
        backgroundMusic.GetComponent<AudioSource>().volume = backgroundMusicSlider.GetComponent<Slider>().value;
        menu.EnableMenu();
        gameObject.SetActive(false);
    }
}
