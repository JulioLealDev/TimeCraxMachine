using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using TimeCrax.Core;

public class MachineComponent : MonoBehaviourPunCallbacks
{
    public int componentId;
    public int malfunctions = 0;
    public GameObject gameInfo;
    private SoundEffects soundEffects;
    private GameOver gameOver;
    private Transform sparks;
    private Transform smoke;
    private Transform componentWithAnimator = null;
    private Transform[] childrenWithanimator = new Transform[4] {null,null,null,null};
    private int count;
    void Start()
    {
        soundEffects = FindFirstObjectByType<SoundEffects>();
        gameOver = FindFirstObjectByType<GameOver>();
        count = 0;

        var parent = gameObject.transform.parent;
        if (parent.name != "Enviroment")
        {
            Transform[] opcoes = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < opcoes.Length; i++)
            {
                if (opcoes[i].GetComponent<Animator>() != null)
                {
                    childrenWithanimator[count] = opcoes[i];
                    count++;
                }
                else if (opcoes[i].CompareTag("Sparks"))
                {
                    sparks = opcoes[i];
                }
                else if (opcoes[i].CompareTag("Smoke"))
                {
                    smoke = opcoes[i];
                }
            }
        }
        else
        {
            componentWithAnimator = gameObject.GetComponent<Transform>();

            Transform[] childs = gameObject.GetComponentsInChildren<Transform>(true);
            foreach(var child in childs)
            {
                if (child.CompareTag("Sparks"))
                {
                    sparks = child;
                }
                else if (child.CompareTag("Smoke"))
                {
                    smoke = child;
                }
            }
        }

    }

    public void OnMouseDown()
    {

        if (gameObject.CompareTag("Selectable")) 
        {
            var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                DebugHelper.Log("Vez de " + player.nickname + " : " + player.GetYourTurn());
                if (player.GetYourTurn())
                {

                    DebugHelper.Log("Number od cards: " + player.GetNumberOfRepairsCards());

                    if(player.GetNumberOfRepairsCards() >= players.Length)
                    {

                        photonView.RPC("RemoveMalfunction", RpcTarget.All);
                        player.RepairComponent(players.Length);
                        DebugHelper.Log("component: " + componentId);

                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "RepairInfoBackground")
                            {
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        this.DelayedCall(1.5f, HideRepairInfo);
                    }
                    else
                    {
                        DebugHelper.Log("You need " + players.Length + " Repair Cards to repair one component!");


                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "ComponentInfoBackground")
                            {
                                info.GetComponentInChildren<TextMeshProUGUI>().text = "You need " + players.Length + " Repair Cards to repair one component!";
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        this.DelayedCall(1.5f, HideComponentInfo);
                    }
                }
            }
        }
        else
        {
            DebugHelper.Log("Voc� j� realizou uma a��o nesse turno");

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

    }
    public void HideRepairInfo()
    {
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "RepairInfoBackground")
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
    public void HideComponentInfo()
    {

        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "ComponentInfoBackground")
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

    public void AddMalfunction()
    {
        malfunctions++;

        if (malfunctions > 1)
        {
            DebugHelper.Log("----> EndGame");
            soundEffects.PlayFinalComponentExplosionSound();
            //sparks.gameObject.SetActive(true);
            smoke.gameObject.SetActive(true);

            gameOver.gameIsOver = true;

            this.DelayedCall(3f, EndGame);

        }
        else
        {
            DebugHelper.Log("----> NOT EndGame");
            if (componentWithAnimator != null)
            {
                componentWithAnimator.gameObject.GetComponent<Animator>().SetBool("malfunction", true);
            }
            else
            {
                foreach (var child in childrenWithanimator)
                {
                    if(child != null)
                    {
                        child.gameObject.GetComponent<Animator>().SetBool("malfunction", true);
                    }
                }
                
            }

            sparks.gameObject.SetActive(true);
            soundEffects.PlayComponentExplosionSound();
        }

    }

    public void EndGame()
    {
        BackgroundMusic backgroundMusic = FindFirstObjectByType<BackgroundMusic>();
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        gameManager.DeactivateAll();
        gameManager.ResetAllComponents();
        gameManager.ResetAllPlatenames();

        backgroundMusic.PlayGameOverSound();
        gameOver.transform.GetChild(0).gameObject.SetActive(true);
        gameManager.hud.SetActive(false);
        DebugHelper.Log("name ---> " + gameOver.name);
    }

    [PunRPC]
    public void RemoveMalfunction()
    {
        if (componentWithAnimator != null)
        {
            componentWithAnimator.gameObject.GetComponent<Animator>().SetBool("malfunction", false);
        }
        else
        {
            foreach (var child in childrenWithanimator)
            {
                if (child != null)
                {
                    child.gameObject.GetComponent<Animator>().SetBool("malfunction", false);
                }
            }

        }


        sparks.gameObject.SetActive(false);
        soundEffects.PlayComponentRepairSound();

        malfunctions--;
        gameObject.GetComponent<MeshCollider>().enabled = false;

        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager.BlockActions();
    }


}
