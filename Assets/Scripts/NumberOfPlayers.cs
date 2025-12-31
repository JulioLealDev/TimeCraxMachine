using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using TimeCrax.Core;

public class NumberOfPlayers : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshPro warningRoomCreated;

    private bool click = true;

    void Start()
    {
        SessionData.NumberOfPlayers = 1;
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
            this.DelayedCall(1.0f, AfterClickRoulette);
        }
        else
        {
            animator.SetBool("rouletteClick", click);
            click = !click;
            var numberOfPlayers = SessionData.NumberOfPlayers;
            if (numberOfPlayers <= 3)
            {
                SessionData.NumberOfPlayers = numberOfPlayers + 1;
            }
            else
            {
                SessionData.NumberOfPlayers = 1;
            }
            gameObject.GetComponent<MeshCollider>().enabled = false;
            this.DelayedCall(0.8f, AfterClickRoulette);
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

