using UnityEngine;
using UnityEngine.UIElements;

public class Tutorial : MonoBehaviour
{
    public Canvas canvas;
    public Canvas inputName;
    public SoundEffects soundEffects;
    public Menu menu;

    private void OnMouseDown()
    {
        Debug.Log("Clicou no tutorial");
        soundEffects.TurnPageSound(1);


        canvas.gameObject.SetActive(true);
        inputName.gameObject.SetActive(false);
        menu.DisableMenu();

    }
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        canvas.gameObject.SetActive(false);
    //        UnityEngine.Cursor.lockState = CursorLockMode.None;

    //        inputName.gameObject.SetActive(true);
    //    }
    //}

   }
