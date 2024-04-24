
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    public Animator animator;
    public Camera cam;
    public InputField nameDisplay;
    public SoundEffects soundEffects;

    private void OnMouseDown()
    {
        soundEffects.PressButtonSound();
        animator.SetBool("quitGame", true);
        nameDisplay.text = " ";
    }

    void AwaitRedButtonAnimation()
    {
        cam.gameObject.GetComponent<Animator>().SetBool("enterMenu", false);
        cam.gameObject.GetComponent<Animator>().SetBool("quitGame", true);
        animator.SetBool("quitGame", false);

        Invoke("AfterClickQuitButton", 2.9f);
    }

    private void AfterClickQuitButton()
    {
        //EditorApplication.isPlaying = false;
        Application.Quit();
    }

}
