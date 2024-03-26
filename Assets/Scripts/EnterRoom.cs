
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class EnterRoom : MonoBehaviour
{
    public Animator animator;
    public Camera cam;
    public GameObject suit;
    public InputField nameDisplay;
    public TextMeshPro warning;
    public GameConnection gameConnection;
    public Canvas lobby;
    public GameObject blueButton;

    void Start()
    {
        PlayerPrefs.SetString("nickname", null);
        nameDisplay.text = PlayerPrefs.GetString("nickname");
    }
    void Update()
    {

    }

    public void OnMouseDown()
    {
        PlayerPrefs.SetString("nickname", nameDisplay.text);
        var nickname =  PlayerPrefs.GetString("nickname");

        if (nickname == null || nickname.Equals(""))
        {
            warning.gameObject.SetActive(true);
            warning.gameObject.GetComponent<Animator>().SetBool("nameIsEmpty", true);
            gameObject.GetComponent<MeshCollider>().enabled = false;
            blueButton.gameObject.GetComponent<MeshCollider>().enabled = false;
            Invoke("AfterClickStart", 1.5f);
        }
        else
        {
            //if (gameConnection.gameObject.activeInHierarchy)
            //{
            var connection = FindObjectOfType<GameConnection>();
            connection.Lobby();
            //}
            //else
            //{
            //    gameConnection.gameObject.SetActive(true);
            //}
            var menu = FindObjectOfType<Menu>();
            menu.DisableMenu();
            nameDisplay.gameObject.SetActive(false);

        }

    }
    
    private void AfterClickStart()
    {
        warning.gameObject.SetActive(false);
        warning.gameObject.GetComponent<Animator>().SetBool("nameIsEmpty", false);
        gameObject.GetComponent<MeshCollider>().enabled = true;
        blueButton.gameObject.GetComponent<MeshCollider>().enabled = true;

    }

    //public void ValidateNickname()
    //{

    //    if (nameDisplay.text.Contains(" ") || nameDisplay.text.Contains("-") || nameDisplay.text.Contains("´"))
    //    {
    //        nameDisplay.text = nameDisplay.text.Remove(nameDisplay.text.Length - 1);
    //        PlayerPrefs.SetString("nickname", nameDisplay.text);
    //    }
    //    else
    //    {
    //        PlayerPrefs.SetString("nickname", nameDisplay.text);
    //    }
    //}

    void AwaitGreenButtonAnimation()
    {
        cam.gameObject.GetComponent<Animator>().SetBool("enterMenu", false);
        cam.gameObject.GetComponent<Animator>().SetBool("enterMatch", true);
        animator.SetBool("startGame", false);

    }

}
