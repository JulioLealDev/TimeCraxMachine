using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;
using TimeCrax.Managers;

public class Pages : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite page01;
    [SerializeField] private Sprite page02;
    [SerializeField] private Sprite page03;
    [SerializeField] private Sprite page04;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private SuitTop suitTop;
    [SerializeField] private MenuManager menuManager;


    public void NextButton()
    {
        soundEffects.TurnPageSound(2);

        if (image.sprite == page01)
        {
            image.sprite = page02;
            previousPageButton.gameObject.SetActive(true);
        }
        else if (image.sprite == page02)
        {
            image.sprite = page03;
        }
        else if (image.sprite == page03)
        {
            image.sprite = page04;
            nextPageButton.gameObject.SetActive(false);
            backToMenuButton.gameObject.SetActive(true);
        }
    }

    public void PreviousButton()
    {
        soundEffects.TurnPageSound(2);

        if (image.sprite == page02)
        {
            image.sprite = page01;
            previousPageButton.gameObject.SetActive(false);
        }
        else if (image.sprite == page03)
        {
            image.sprite = page02;
        }
        else if (image.sprite == page04)
        {
            image.sprite = page03;
            backToMenuButton.gameObject.SetActive(false);
            nextPageButton.gameObject.SetActive(true);
        }
    }

    public void BackToMenu()
    {

        soundEffects.TurnPageSound(1);

        this.DelayedCall(0.3f, Back);

    }

    public void Back()
    {
        menuManager.EnablingMenuOptions();

        image.sprite = page01;

        backToMenuButton.gameObject.SetActive(false);
        nextPageButton.gameObject.SetActive(true);
        previousPageButton.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

}
