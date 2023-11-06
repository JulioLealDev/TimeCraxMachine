using UnityEngine;
using UnityEngine.UIElements;

public class Tutorial : MonoBehaviour
{
    public Canvas canvas;
    public Canvas inputName;

    private void OnMouseDown()
    {
        Debug.Log("Clicou no tutorial");
        canvas.gameObject.SetActive(true);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        inputName.gameObject.SetActive(false);

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            canvas.gameObject.SetActive(false);
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            inputName.gameObject.SetActive(true);
        }
    }

   }
