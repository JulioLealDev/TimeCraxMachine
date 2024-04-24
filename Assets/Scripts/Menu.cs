using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    //public GameObject roulette;
    //public TextMeshPro warningRoomCreated;
    public Camera camera;
    public GameObject hudGame;
    public GameObject inputName;
    public GameObject inputNameText;
    public BackgroundMusic backgroundMusic;

    public void AwaitOpenSuit()
    {
        Debug.Log("Entrou no AwaitOpenSuit");
        Transform[] suitTop = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < suitTop.Length; i++)
        {
            if (suitTop[i].CompareTag("Selectable"))
            {
                Debug.Log("Retirando Mesh dos objeto: " + suitTop[i].name);
                suitTop[i].GetComponent<MeshCollider>().enabled = false;
            }
        }

    }

    public void AwaitCloseSuit()
    {
        GameOver gameOver = FindObjectOfType<GameOver>();

        Debug.Log("Entrou no AwaitCloseSuit");
        hudGame.SetActive(false);
        camera.GetComponent<Animator>().SetBool("enterMatch", false);

        Debug.Log("exit? : "+gameOver.exitGame);
        if (gameOver.exitGame)
        {
            Debug.Log("Saindo do jogo");

            Invoke("QuitGame", 2f);
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
                        //Debug.Log("Ativando Mesh dos objeto: " + suitTop[i].name);
                        suitTop[i].tag = "Selectable";
                        suitTop[i].GetComponent<MeshCollider>().enabled = true;

                        if (suitTop[i].GetComponent<Animator>() != null)
                        {
                            suitTop[i].GetComponent<Animator>().enabled = true;
                        }
                    }

                }
            }
            Invoke("ActivateInputName", 3.7f);
        }



    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DistanceTimelineWhenQuit()
    {
        if (camera.GetComponent<Animator>().GetBool("zoomTimeline"))
        {
            camera.GetComponent<Animator>().SetBool("zoomTimeline", false);
            camera.GetComponent<Animator>().SetBool("distanceZoom", true);
        }

    }

    public void ActivateInputName()
    {
        Debug.Log("ativando input");
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