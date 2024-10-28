using UnityEngine;
using Photon.Pun;
using TMPro;
using System;

public class Component : MonoBehaviourPunCallbacks
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
        soundEffects = FindObjectOfType<SoundEffects>();
        gameOver = FindObjectOfType<GameOver>();
        count = 0;

        var parent = gameObject.transform.parent;
        if (parent.name != "Enviroment")
        {
            //Debug.Log("parent: " + parent.name);
            Transform[] opcoes = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < opcoes.Length; i++) 
            {
                //Debug.Log("opc: " + opc.name);
                if (opcoes[i].GetComponent<Animator>() != null)
                {
                    childrenWithanimator[count] = opcoes[i];
                    count++;
                }
                else if (opcoes[i].CompareTag("Sparks"))
                {
                    //Debug.Log("sparks: " + opc.name);
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
                //Debug.Log("child: " + child.name);
                if (child.CompareTag("Sparks"))
                {
                    //Debug.Log("sparks: " + child.name);
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
            var players = FindObjectsOfType<PlayerScript>();
            foreach (var player in players)
            {
                Debug.Log("Vez de " + player.nickname + " : " + player.GetYourTurn());
                if (player.GetYourTurn())
                {

                    Debug.Log("Number od cards: " + player.GetNumberOfRepairsCards());

                    if(player.GetNumberOfRepairsCards() >= players.Length)
                    {

                        photonView.RPC("RemoveMalfunction", RpcTarget.All);
                        player.RepairComponent(players.Length);
                        Debug.Log("component: " + componentId);

                        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
                        gameInfo.gameObject.SetActive(true);

                        foreach (var info in infos)
                        {
                            if (info.gameObject.name == "RepairInfoBackground")
                            {
                                info.GetComponent<CanvasGroup>().LeanAlpha(1f, 0.5f);
                            }
                        }

                        Invoke("HideRepairInfo", 1.5f);
                    }
                    else
                    {
                        Debug.Log("You need " + players.Length + " Repair Cards to repair one component!");


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

                        Invoke("HideComponentInfo", 1.5f);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Você já realizou uma ação nesse turno");

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

    }
    public void HideRepairInfo()
    {
        //Debug.Log("HideRoundInfo()");
        Transform[] infos = gameInfo.GetComponentsInChildren<Transform>();
        foreach (var info in infos)
        {
            if (info.gameObject.name == "RepairInfoBackground")
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
        Invoke("DisableGameInfo", 0.5f);
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
            Debug.Log("----> EndGame");
            soundEffects.PlayFinalComponentExplosionSound();
            //sparks.gameObject.SetActive(true);
            smoke.gameObject.SetActive(true);

            gameOver.gameIsOver = true;

            Invoke("EndGame", 3f);

        }
        else
        {
            Debug.Log("----> NOT EndGame");
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

            //var parent = gameObject.transform.parent;
            //if (parent.name != "Enviroment")
            //{
            //    //Debug.Log("parent: " + parent.name);
            //    Transform[] opcoes = parent.GetComponentsInChildren<Transform>();
            //    foreach (var opc in opcoes)
            //    {
            //        //Debug.Log("opc: " + opc.name);
            //        if (opc.GetComponent<Animator>() != null)
            //        {
            //            opc.GetComponent<Animator>().SetBool("malfunction", true);

            //            //ParticleSystem effect = opc.GetComponentInChildren<ParticleSystem>(true);
            //            sparks.gameObject.SetActive(true);
            //            soundEffects.PlayComponentExplosionSound();
            //        }
            //    }
            //}
            //else
            //{
            //    gameObject.GetComponent<Animator>().SetBool("malfunction", true);

            //    //ParticleSystem effect = gameObject.GetComponentInChildren<ParticleSystem>(true);
            //    sparks.gameObject.SetActive(true);
            //    soundEffects.PlayComponentExplosionSound();
            //}
        }

    }

    public void EndGame()
    {
        BackgroundMusic backgroundMusic = FindObjectOfType<BackgroundMusic>();
        GameManager gameManager = FindObjectOfType<GameManager>();

        gameManager.DeactivateAll();
        gameManager.ResetAllComponents();
        gameManager.ResetAllPlatenames();

        backgroundMusic.PlayGameOverSound();
        gameOver.transform.GetChild(0).gameObject.SetActive(true);
        gameManager.hud.SetActive(false);
        Debug.Log("name ---> " + gameOver.name);


        //Invoke("ReturningToMenu", 2f);

    }

    //public void ReturningToMenu()
    //{
    //    gameOver.transform.GetChild(0).gameObject.SetActive(false);
    //    var gameManager = FindObjectOfType<GameManager>();
    //    gameManager.QuitGame();
    //}

    [PunRPC]
    public void RemoveMalfunction()
    {
        ////ativar animação
        //var parent = gameObject.transform.parent;
        //if (parent.name != "Enviroment")
        //{
        //    //Debug.Log("parent: " + parent.name);
        //    Transform[] opcoes = parent.GetComponentsInChildren<Transform>();
        //    foreach (var opc in opcoes)
        //    {
        //       // Debug.Log("opc: " + opc.name);
        //        if (opc.GetComponent<Animator>() != null)
        //        {
        //            opc.GetComponent<Animator>().SetBool("malfunction", false);

        //            soundEffects.PlayComponentRepairSound();

        //            //ParticleSystem effect = opc.GetComponentInChildren<ParticleSystem>(true);
        //            sparks.gameObject.SetActive(false);

        //        }
        //    }
        //}
        //else
        //{
        //    gameObject.GetComponent<Animator>().SetBool("malfunction", false);

        //    soundEffects.PlayComponentRepairSound();

        //    //ParticleSystem effect = gameObject.GetComponentInChildren<ParticleSystem>(true);
        //    sparks.gameObject.SetActive(false);
        //}

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

        var gameManager = FindObjectOfType<GameManager>();
        gameManager.BlockActions();
    }


}
