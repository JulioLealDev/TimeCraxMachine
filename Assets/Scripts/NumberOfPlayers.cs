using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class NumberOfPlayers : MonoBehaviour
{

    public Animator animator;
    private bool click = true;
    public TextMeshPro warningRoomCreated;

    void Start()
    {
        PlayerPrefs.SetInt("numberOfPlayers", 1);
    }

    private void OnMouseDown()
    {
        if (gameObject.CompareTag("InRoom"))
        {
            animator.enabled = false;
            gameObject.GetComponent<MeshCollider>().enabled = false;
        }
        else if (gameObject.CompareTag("Disabled"))
        {
            animator.enabled = false;
            warningRoomCreated.GetComponent<Animator>().SetBool("roomCreated", true);
            Invoke("AfterClickRoulette", 1.0f);
        }
        else
        {
            animator.SetBool("rouletteClick", click);
            click = !click;
            var numberOfPlayers = PlayerPrefs.GetInt("numberOfPlayers");
            if (numberOfPlayers <= 3)
            {
                PlayerPrefs.SetInt("numberOfPlayers", numberOfPlayers + 1);
                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.SetInt("numberOfPlayers", 1);
            }
            gameObject.GetComponent<MeshCollider>().enabled = false;
            Invoke("AfterClickRoulette", 0.8f);
        }

    }

    private void AfterClickRoulette()
    {
        if (gameObject.CompareTag("Disabled"))
        {
            warningRoomCreated.GetComponent<Animator>().SetBool("roomCreated", false);
        }
        else
        {
            gameObject.GetComponent<MeshCollider>().enabled = true;
        }
    }
}

