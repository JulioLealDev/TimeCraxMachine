using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

public class CreateRoom : MonoBehaviour
{
    [SerializeField] private TextMeshPro warning;
    [SerializeField] private InputField nameDisplay;
    [SerializeField] private GameConnection gameConnection;
    [SerializeField] private GameObject greenButton;
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
            greenButton.gameObject.GetComponent<MeshCollider>().enabled = false;
            this.DelayedCall(1.5f, AfterClickStart);
        }
        else
        {
            //if (gameConnection.gameObject.activeInHierarchy)
            //{
            var connection = FindFirstObjectByType<GameConnection>();
            connection.CreateRoom();
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
        greenButton.gameObject.GetComponent<MeshCollider>().enabled = true;
        gameObject.GetComponent<MeshCollider>().enabled = true;
    }
}
