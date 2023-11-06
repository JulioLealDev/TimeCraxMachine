using UnityEngine.EventSystems;
using UnityEngine;

public class GiveCards : MonoBehaviour
{

    public Canvas gameInfo;
    private int sendNumberCards;
    private void OnMouseDown()
    {
        Debug.Log("Clicou no player: " + gameObject.name);

        var players = FindObjectsOfType<PlayerScript>();
        foreach (var player in players)
        {
            if (player.GetYourTurn())
            {
                sendNumberCards = player.GetNumberOfRepairsCards();
            }
        }


        if (gameObject.tag == "Disabled")
        {

            Debug.Log("Você já realizaou uma ação neste turno");

            Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
            gameInfo.gameObject.SetActive(true);

            foreach (var info in infos)
            {
                if (info.gameObject.name == "ActionInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }

            Invoke("HideActionInfo", 1.5f);
        }
        else if (sendNumberCards == 0)
        {
            Debug.Log("Você não possui cartas de reparo");

            Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
            gameInfo.gameObject.SetActive(true);

            foreach (var info in infos)
            {
                if (info.gameObject.name == "CardInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }

            Invoke("HideCardInfo", 1.5f);
        }
        else
        {
            int numberPlayer = 0;

            if (gameObject.name == "plateName01")
            {
                numberPlayer = 1;
            }
            else if (gameObject.name == "plateName02")
            {
                numberPlayer = 2;
            }
            else if (gameObject.name == "plateName03")
            {
                numberPlayer = 3;
            }
            else if (gameObject.name == "plateName04")
            {
                numberPlayer = 4;
            }

            foreach (var player in players)
            {
                if (player.index == numberPlayer - 1)
                {
                    int receiverNumberCards = player.GetNumberOfRepairsCards();
                    if(receiverNumberCards == 5)
                    {
                        Debug.Log("Este jogador já possui 5 cartas");

                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "FiveInfoBackground")
                            {
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        Invoke("HideFiveInfo", 1.5f);
                    }
                    else
                    {
                        var gameManager = FindObjectOfType<GameManager>();
                        gameManager.GiveCard(numberPlayer);
                    }
                }
            }


        }
   
    }

    public void HideFiveInfo()
    {
        //Debug.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "FiveInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("DisableGameInfo", 0.5f);
    }

    public void HideActionInfo()
    {
        //Debug.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ActionInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("DisableGameInfo", 0.5f);
    }

    public void HideCardInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "CardInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        Invoke("DisableGameInfo", 0.5f);
    }

    public void DisableGameInfo()
    {
        //Debug.Log("DisableGameInfo()");
        gameInfo.gameObject.SetActive(false);
    }
}
