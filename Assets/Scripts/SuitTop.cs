using UnityEngine;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Managers;

public class SuitTop : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    //[SerializeField] private GameObject inputName;
    //[SerializeField] private GameObject inputNameText;
    [SerializeField] private MenuManager menuManager;

    public void AwaitOpenSuit()
    {
        menuManager.DesablingMenuOptions();
    }

    public void AwaitCloseSuit()
    {


        cameraController.ExitingMatch();

        if (GameStateManager.Is(GamePhase.ExitingGame))
        {
            this.DelayedCall(2f, QuitGame);
        }
        else
        {
            //reset all game components and activate menu options
            menuManager.EnablingMenuOptions();
            //this.DelayedCall(3.7f, ActivateInputName);
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /*private Transform[] GetMenuOptionsChildren()
    {
        Transform menuOptions = transform.Find("MenuOptions");
        if (menuOptions == null) return new Transform[0];
        return menuOptions.GetComponentsInChildren<Transform>();
    }*/

    /*public void DistanceTimelineWhenQuit()
    {
        if (cameraController.GetComponent<Animator>().GetBool("zoomTimeline"))
        {
            cameraController.GetComponent<Animator>().SetBool("zoomTimeline", false);
            cameraController.GetComponent<Animator>().SetBool("distanceZoom", true);
        }

    }*/

    /*public void ActivateInputName()
    {

        if (inputName != null)
            inputName.gameObject.SetActive(true);

        if (inputNameText != null)
            inputNameText.gameObject.SetActive(true);
    }*/

    /*public void DisableMenu()
    {
        foreach (Transform child in GetMenuOptionsChildren())
        {
            //if (child.GetComponent<MeshCollider>() != null && !child.CompareTag("Undestructable"))
            {
                //child.tag = "InRoom";
                child.GetComponent<MeshCollider>().enabled = false;
            }
            if (child.GetComponent<Animator>() != null)
            {
                child.GetComponent<Animator>().enabled = false;
            }
        }
    }*/

    /*public void EnableMenu()
    {
        foreach (Transform child in GetMenuOptionsChildren())
        {
            //if (child.GetComponent<MeshCollider>() != null && !child.CompareTag("Undestructable"))
            {
                //child.tag = "Selectable";
                child.GetComponent<MeshCollider>().enabled = true;
            }
            if (child.GetComponent<Animator>() != null)
            {
                child.GetComponent<Animator>().enabled = true;
            }
        }
    }*/
}
