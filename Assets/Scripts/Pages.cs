using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pages : MonoBehaviour
{
    public Image image;
    public Sprite page01;
    public Sprite page02;
    public Sprite page03;
    public Sprite page04;
    public Button nextPageButton;
    public Button previousPageButton;
    public Button backToMenuButton;
    public Canvas inputName;
    public SoundEffects soundEffects;
    public Menu menu;

    public void NextButton()
    {

        soundEffects.TurnPageSound(2);

        Debug.Log("noma da imagem" + image.GetComponent<Image>().sprite.name);

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

        Invoke("Back", 0.3f);

    }

    public void Back()
    {
        Debug.Log("Reset Tutorial");

        inputName.gameObject.SetActive(true);
        menu.EnableMenu();

        image.GetComponent<Image>().sprite = page01;

        backToMenuButton.gameObject.SetActive(false);
        nextPageButton.gameObject.SetActive(true);
        previousPageButton.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

}
