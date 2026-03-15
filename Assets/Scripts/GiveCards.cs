using UnityEngine;
using TimeCrax.Core;

public class GiveCards : MonoBehaviour
{
    [SerializeField] private Canvas gameInfo;
    private int sendNumberCards;

    // Proteção contra clique duplo
    private bool isProcessingClick = false;

    // Nome do background atual para esconder
    private string currentInfoBackground;

    private void OnMouseDown()
    {
        // Bloquear clique durante animações de câmera
        if (CameraController.IsAnimating) return;

        // Proteção contra clique duplo
        if (isProcessingClick) return;
        isProcessingClick = true;

        DebugHelper.Log("Clicou no player: " + gameObject.name);

        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetYourTurn())
            {
                sendNumberCards = player.GetNumberOfRepairsCards();
            }
        }

        if (gameObject.CompareTag("Disabled"))
        {
            DebugHelper.Log("Você já realizou uma ação neste turno");
            ShowInfo("ActionInfoBackground");
        }
        else if (sendNumberCards == 0)
        {
            DebugHelper.Log("Você não possui cartas de reparo");
            ShowInfo("CardInfoBackground");
        }
        else
        {
            // Extrair número do plateName (plateName01 -> 1, plateName02 -> 2, etc.)
            int numberPlayer = GetPlayerNumberFromPlateName();
            if (numberPlayer == 0) return;

            foreach (var player in players)
            {
                if (player.index == numberPlayer - 1)
                {
                    int receiverNumberCards = player.GetNumberOfRepairsCards();
                    if (receiverNumberCards == 5)
                    {
                        DebugHelper.Log("Este jogador já possui 5 cartas");
                        ShowInfo("FiveInfoBackground");
                    }
                    else
                    {
                        var gameManager = FindFirstObjectByType<GameManager>();
                        if (gameManager != null)
                        {
                            gameManager.GiveCard(numberPlayer);
                        }
                        else
                        {
                            DebugHelper.Log("[GiveCards] GameManager não encontrado");
                        }
                        isProcessingClick = false;
                    }
                }
            }
        }
    }

    private int GetPlayerNumberFromPlateName()
    {
        string name = gameObject.name;
        if (name.StartsWith("plateName") && name.Length == 11)
        {
            if (int.TryParse(name.Substring(9, 2), out int num))
            {
                return num;
            }
        }
        return 0;
    }

    private void ShowInfo(string backgroundName)
    {
        currentInfoBackground = backgroundName;
        gameInfo.gameObject.SetActive(true);

        foreach (Transform info in gameInfo.GetComponentsInChildren<Transform>())
        {
            if (info.gameObject.name == backgroundName)
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
            }
        }

        this.DelayedCall(1.5f, HideCurrentInfo);
    }

    private void HideCurrentInfo()
    {
        foreach (Transform info in gameInfo.GetComponentsInChildren<Transform>())
        {
            if (info.gameObject.name == currentInfoBackground)
            {
                info.GetComponent<CanvasGroup>().LeanAlpha(0f, 0.5f);
            }
        }
        this.DelayedCall(0.5f, DisableGameInfo);
    }

    public void DisableGameInfo()
    {
        gameInfo.gameObject.SetActive(false);
        isProcessingClick = false;
    }

    public void ResetClickProtection()
    {
        isProcessingClick = false;
    }
}
