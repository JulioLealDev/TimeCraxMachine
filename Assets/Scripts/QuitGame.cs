
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;

public class QuitGame : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cam;
    [SerializeField] private InputField nameDisplay;
    [SerializeField] private SoundEffects soundEffects;

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

        this.DelayedCall(2.9f, AfterClickQuitButton);
    }

    private void AfterClickQuitButton()
    {
        //EditorApplication.isPlaying = false;
        Application.Quit();
    }

}
