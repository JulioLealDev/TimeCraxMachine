using UnityEngine;
using TMPro;
using TimeCrax.Core;

public class Menu : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject hudGame;
    [SerializeField] private GameObject inputName;
    [SerializeField] private GameObject inputNameText;
    [SerializeField] private BackgroundMusic backgroundMusic;

    private Transform[] GetMenuOptionsChildren()
    {
        Transform menuOptions = transform.Find("MenuOptions");
        if (menuOptions == null) return new Transform[0];
        return menuOptions.GetComponentsInChildren<Transform>();
    }

    public void AwaitOpenSuit()
    {
        DebugHelper.Log("Entrou no AwaitOpenSuit");
        foreach (Transform child in GetMenuOptionsChildren())
        {
            if (child.CompareTag("Selectable"))
            {
                DebugHelper.Log("Retirando Mesh dos objeto: " + child.name);
                var col = child.GetComponent<MeshCollider>();
                if (col != null) col.enabled = false;
            }
        }
    }

    public void AwaitCloseSuit()
    {
        GameOver gameOver = FindFirstObjectByType<GameOver>();

        DebugHelper.Log("Entrou no AwaitCloseSuit");
        cameraController.GetComponent<Animator>().SetBool("enterMatch", false);

        DebugHelper.Log("exit? : "+gameOver.exitGame);
        if (gameOver.exitGame)
        {
            DebugHelper.Log("Saindo do jogo");

            this.DelayedCall(2f, QuitGame);
        }
        else
        {
            foreach (Transform child in GetMenuOptionsChildren())
            {
                if (child.CompareTag("InRoom") || child.name == "timeline")
                {
                    if (child.name == "timeline")
                    {
                        child.tag = "Undestructable";
                        if (child.GetComponent<MeshCollider>() != null)
                            child.GetComponent<MeshCollider>().enabled = false;
                    }
                    else
                    {
                        child.tag = "Selectable";
                        if (child.GetComponent<MeshCollider>() != null)
                            child.GetComponent<MeshCollider>().enabled = true;
                    }
                }

                if (child.GetComponent<Animator>() != null && !child.CompareTag("Undestructable"))
                {
                    child.GetComponent<Animator>().enabled = true;
                }
            }
            this.DelayedCall(3.7f, ActivateInputName);
        }



    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DistanceTimelineWhenQuit()
    {
        if (cameraController.GetComponent<Animator>().GetBool("zoomTimeline"))
        {
            cameraController.GetComponent<Animator>().SetBool("zoomTimeline", false);
            cameraController.GetComponent<Animator>().SetBool("distanceZoom", true);
        }

    }

    public void ActivateInputName()
    {
        DebugHelper.Log("ativando input");

        if (inputName != null)
            inputName.gameObject.SetActive(true);

        if (inputNameText != null)
            inputNameText.gameObject.SetActive(true);
    } 
    public void DisableMenu()
    {
        foreach (Transform child in GetMenuOptionsChildren())
        {
            if (child.GetComponent<MeshCollider>() != null && !child.CompareTag("Undestructable"))
            {
                child.tag = "InRoom";
                child.GetComponent<MeshCollider>().enabled = false;
            }
            if (child.GetComponent<Animator>() != null)
            {
                child.GetComponent<Animator>().enabled = false;
            }
        }
    }

    public void EnableMenu()
    {
        foreach (Transform child in GetMenuOptionsChildren())
        {
            if (child.GetComponent<MeshCollider>() != null && !child.CompareTag("Undestructable"))
            {
                child.tag = "Selectable";
                child.GetComponent<MeshCollider>().enabled = true;
            }
            if (child.GetComponent<Animator>() != null)
            {
                child.GetComponent<Animator>().enabled = true;
            }
        }
    }
}