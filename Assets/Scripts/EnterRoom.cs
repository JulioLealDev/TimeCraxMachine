
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using TimeCrax.Core;

public class EnterRoom : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cam;
    [SerializeField] private GameObject suit;
    [SerializeField] private InputField nameDisplay;
    [SerializeField] private TextMeshPro warning;
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private Canvas lobby;
    [SerializeField] private GameObject blueButton;
    [SerializeField] private SoundEffects soundEffects;

    void Start()
    {
        SessionData.Nickname = string.Empty;
        nameDisplay.text = SessionData.Nickname;
    }
    public void OnMouseDown()
    {
        soundEffects.PressButtonSound();

        SessionData.Nickname = nameDisplay.text;
        var nickname = SessionData.Nickname;

        if (nickname == null || nickname.Equals(""))
        {
            warning.gameObject.SetActive(true);
            warning.gameObject.GetComponent<Animator>().SetBool("nameIsEmpty", true);
            gameObject.GetComponent<MeshCollider>().enabled = false;
            blueButton.gameObject.GetComponent<MeshCollider>().enabled = false;
            this.DelayedCall(1.5f, AfterClickStart);
        }
        else
        {
            //if (gameConnection.gameObject.activeInHierarchy)
            //{
            var connection = FindFirstObjectByType<GameConnection>();
            connection.Lobby();
            //}
            //else
            //{
            //    gameConnection.gameObject.SetActive(true);
            //}
            var menu = FindFirstObjectByType<Menu>();
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

    //    if (nameDisplay.text.Contains(" ") || nameDisplay.text.Contains("-") || nameDisplay.text.Contains("�"))
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
