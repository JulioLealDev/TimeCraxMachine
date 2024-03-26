using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateRoom : MonoBehaviour
{

    public TextMeshPro warning;
    public InputField nameDisplay;
    public GameConnection gameConnection;
    public GameObject greenButton;

    void Start()
    {
        PlayerPrefs.SetString("nickname", null);
        nameDisplay.text = PlayerPrefs.GetString("nickname");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        PlayerPrefs.SetString("nickname", nameDisplay.text);
        var nickname = PlayerPrefs.GetString("nickname");

        if (nickname == null || nickname.Equals(""))
        {
            warning.gameObject.SetActive(true);
            warning.gameObject.GetComponent<Animator>().SetBool("nameIsEmpty", true);
            gameObject.GetComponent<MeshCollider>().enabled = false;
            greenButton.gameObject.GetComponent<MeshCollider>().enabled = false;
            Invoke("AfterClickStart", 1.5f);
        }
        else
        {
            //if (gameConnection.gameObject.activeInHierarchy)
            //{
            var connection = FindObjectOfType<GameConnection>();
            connection.CreateRoom();
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
        greenButton.gameObject.GetComponent<MeshCollider>().enabled = true;
        gameObject.GetComponent<MeshCollider>().enabled = true;
    }
}
