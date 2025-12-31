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

    public void AwaitOpenSuit()
    {
        DebugHelper.Log("Entrou no AwaitOpenSuit");
        Transform[] suitTop = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < suitTop.Length; i++)
        {
            if (suitTop[i].CompareTag("Selectable"))
            {
                DebugHelper.Log("Retirando Mesh dos objeto: " + suitTop[i].name);
                suitTop[i].GetComponent<MeshCollider>().enabled = false;
            }
        }

    }

    public void AwaitCloseSuit()
    {
        GameOver gameOver = FindFirstObjectByType<GameOver>();

        DebugHelper.Log("Entrou no AwaitCloseSuit");
        //hudGame.SetActive(false);
        cameraController.GetComponent<Animator>().SetBool("enterMatch", false);

        DebugHelper.Log("exit? : "+gameOver.exitGame);
        if (gameOver.exitGame)
        {
            DebugHelper.Log("Saindo do jogo");

            this.DelayedCall(2f, QuitGame);
        }
        else
        {
            Transform[] suitTop = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < suitTop.Length; i++)
            {
                if (suitTop[i].CompareTag("InRoom") || suitTop[i].name == "timeline")
                {
                    if (suitTop[i].name == "timeline")
                    {
                        suitTop[i].tag = "Undestructable";
                        suitTop[i].GetComponent<MeshCollider>().enabled = false;
                    }
                    else
                    {
                        //DebugHelper.Log("Ativando Mesh dos objeto: " + suitTop[i].name);
                        suitTop[i].tag = "Selectable";
                        suitTop[i].GetComponent<MeshCollider>().enabled = true;

                        if (suitTop[i].GetComponent<Animator>() != null)
                        {
                            suitTop[i].GetComponent<Animator>().enabled = true;
                        }
                    }

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
        inputName.gameObject.SetActive(true);
        inputNameText.gameObject.SetActive(true);
    } 
    public void DisableMenu()
    {
        Transform[] opcoes = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < opcoes.Length; i++)
        {

            if (opcoes[i].GetComponent<MeshCollider>() != null && !opcoes[i].CompareTag("Undestructable"))
            {
                opcoes[i].tag = "InRoom";
                opcoes[i].GetComponent<MeshCollider>().enabled = false;
            }
            if (opcoes[i].GetComponent<Animator>() != null)
            {
                opcoes[i].GetComponent<Animator>().enabled = false;
            }

        }
    }
    public void EnableMenu()
    {
        Transform[] opcoes = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < opcoes.Length; i++)
        {

            if (opcoes[i].GetComponent<MeshCollider>() != null && !opcoes[i].CompareTag("Undestructable"))
            {
                opcoes[i].tag = "Selectable";
                opcoes[i].GetComponent<MeshCollider>().enabled = true;
            }
            if (opcoes[i].GetComponent<Animator>() != null)
            {
                opcoes[i].GetComponent<Animator>().enabled = true;
            }

        }
    }
}