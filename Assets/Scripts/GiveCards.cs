using UnityEngine.EventSystems;
using UnityEngine;
using TimeCrax.Core;

public class GiveCards : MonoBehaviour
{

    public Canvas gameInfo;
    private int sendNumberCards;
    private void OnMouseDown()
    {
        DebugHelper.Log("Clicou no player: " + gameObject.name);

        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetYourTurn())
            {
                sendNumberCards = player.GetNumberOfRepairsCards();
            }
        }


        if (gameObject.tag == "Disabled")
        {

            DebugHelper.Log("Voc� j� realizaou uma a��o neste turno");

            Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
            gameInfo.gameObject.SetActive(true);

            foreach (var info in infos)
            {
                if (info.gameObject.name == "ActionInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }

            this.DelayedCall(1.5f, HideActionInfo);
        }
        else if (sendNumberCards == 0)
        {
            DebugHelper.Log("Voc� n�o possui cartas de reparo");

            Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
            gameInfo.gameObject.SetActive(true);

            foreach (var info in infos)
            {
                if (info.gameObject.name == "CardInfoBackground")
                {
                    info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                }
            }

            this.DelayedCall(1.5f, HideCardInfo);
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
                        DebugHelper.Log("Este jogador j� possui 5 cartas");

                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "FiveInfoBackground")
                            {
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        this.DelayedCall(1.5f, HideFiveInfo);
                    }
                    else
                    {
                        var gameManager = FindFirstObjectByType<GameManager>();
                        gameManager.GiveCard(numberPlayer);
                    }
                }
            }


        }
   
    }

    public void HideFiveInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "FiveInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void HideActionInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ActionInfoBackground")
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
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
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
    }
}
