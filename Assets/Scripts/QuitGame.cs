
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    public Animator animator;
    public Camera cam;
    public InputField nameDisplay;

    private void OnMouseDown()
    {
        animator.SetBool("quitGame", true);
        nameDisplay.text = " ";
    }

    void AwaitRedButtonAnimation()
    {
        cam.gameObject.GetComponent<Animator>().SetBool("enterMenu", false);
        cam.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);
        animator.SetBool("quitGame", false);

        Invoke("AfterClickQuitButton", 2.9f);
    }

    private void AfterClickQuitButton()
    {
        //EditorApplication.isPlaying = false;
        Application.Quit();
    }

}
