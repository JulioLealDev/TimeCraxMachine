
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;
using TMPro;
using TimeCrax.Managers;

public class QuitGame : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private TMP_Text nameDisplay;
    [SerializeField] private SoundEffects soundEffects;
    [SerializeField] private MenuManager menuManager;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        if (!GameManager.TryBeginClick(this)) return;

        soundEffects.PressButtonSound();
        animator.SetBool("quitGame", true);
        nameDisplay.text = " ";
        menuManager.DesablingMenuOptions();
        GameStateManager.TransitionTo(GamePhase.ExitingGame);
    }

    void AwaitRedButtonAnimation()
    {
        cameraController.DistanceFromMenu();
        animator.SetBool("quitGame", false);

        this.DelayedCall(2.9f, AfterClickQuitButton);
    }

    private void AfterClickQuitButton()
    {
        //EditorApplication.isPlaying = false;
        Application.Quit();
    }

}
