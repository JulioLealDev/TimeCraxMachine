using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;

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
    [SerializeField] private Canvas inputName;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private Menu menu;

    public void NextButton()
    {

        soundEffects.TurnPageSound(2);

        DebugHelper.Log("noma da imagem" + image.GetComponent<Image>().sprite.name);

        if (image.GetComponent<Image>().sprite == page01) 
        {
            image.GetComponent<Image>().sprite = page02;

            previousPageButton.gameObject.SetActive(true);
        }
        else if (image.GetComponent<Image>().sprite == page02)
        {
            image.GetComponent<Image>().sprite = page03;
        }
        else if (image.GetComponent<Image>().sprite == page03)
        {
            image.GetComponent<Image>().sprite = page04;
            nextPageButton.gameObject.SetActive(false);
            backToMenuButton.gameObject.SetActive(true);
        }
        else
        {

        }
    }

    public void PreviousButton()
    {

        soundEffects.TurnPageSound(2);

        if (image.GetComponent<Image>().sprite == page02)
        {
            image.GetComponent<Image>().sprite = page01;

            previousPageButton.gameObject.SetActive(false);
        }
        else if (image.GetComponent<Image>().sprite == page03)
        {
            image.GetComponent<Image>().sprite = page02;
        }
        else if (image.GetComponent<Image>().sprite == page04)
        {
            image.GetComponent<Image>().sprite = page03;
            backToMenuButton.gameObject.SetActive(false);
            nextPageButton.gameObject.SetActive(true);
        }
        else
        {
            
        }
    }

    public void BackToMenu()
    {

        soundEffects.TurnPageSound(1);

        this.DelayedCall(0.3f, Back);

    }

    public void Back()
    {
        DebugHelper.Log("Reset Tutorial");

        inputName.gameObject.SetActive(true);
        menu.EnableMenu();

        image.GetComponent<Image>().sprite = page01;

        backToMenuButton.gameObject.SetActive(false);
        nextPageButton.gameObject.SetActive(true);
        previousPageButton.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

}
