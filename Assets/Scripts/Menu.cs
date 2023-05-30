using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    public GameObject roulette;
    public TextMeshPro warningRoomCreated;

    public void AwaitOpenSuit()
    {
        //Destroy Menu Objects
        Transform[] suitTop = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < suitTop.Length; i++)
        {
            if (!suitTop[i].CompareTag("Undestructable"))
            {
                Destroy(suitTop[i].gameObject);
            }
        }

    }
    public void DisableMenu()
    {
        Transform[] opcoes = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < opcoes.Length; i++)
        {

            if (opcoes[i].GetComponent<MeshCollider>() != null && !opcoes[i].CompareTag("Undestructable"))
            {
                opcoes[i].tag = "InRoom";
                opcoes[i].GetComponent<MeshCollider>().enabled = false;
            }
            if (opcoes[i].GetComponent<Animator>() != null)
            {
                opcoes[i].GetComponent<Animator>().enabled = false;
            }

        }
    }
    public void EnableMenu()
    {
        Transform[] opcoes = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 0; i < opcoes.Length; i++)
        {

            if (opcoes[i].GetComponent<MeshCollider>() != null && !opcoes[i].CompareTag("Undestructable"))
            {
                opcoes[i].tag = "Selectable";
                opcoes[i].GetComponent<MeshCollider>().enabled = true;
            }
            if (opcoes[i].GetComponent<Animator>() != null)
            {
                opcoes[i].GetComponent<Animator>().enabled = true;
            }

        }
    }
    public void DisableRoulette()
    {
        if (roulette.GetComponent<MeshCollider>().enabled == true)
        {
            roulette.tag = "Disabled";
        }
        if (warningRoomCreated != null)
        {
            warningRoomCreated.GetComponent<TextMeshPro>().enabled = true;
        }
    }
    public void EnableRoulette()
    {
        warningRoomCreated.GetComponent<TextMeshPro>().enabled = false;
        roulette.tag = "Selectable";
    }


}