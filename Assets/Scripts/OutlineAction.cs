using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OutlineAction : MonoBehaviour
{
    public Material originalMaterial;
    public Material selectionMaterial; 
    public GameObject menuStart;
    public GameObject timeline;
    public GameObject deckEvent;
    public GameObject deckRepair;
    private Transform highlight;    
    private RaycastHit raycastHit;

    void Start()
    {
        
    }

    void Update()
    {

        // Highlight
        if (highlight != null)
        {
            if (highlight.gameObject.GetComponent<OutlineComponent>() != null)
            {
                highlight.gameObject.GetComponent<OutlineComponent>().enabled = false;
                highlight = null;
            }
            else
            {
                highlight.gameObject.GetComponent<MeshRenderer>().material = originalMaterial;
                highlight = null;

            }

        }

        Transform[] opcoes = menuStart.GetComponentsInChildren<Transform>();
        for (int i = 0; i < opcoes.Length; i++)
        {
            opcoes[i].GetComponentInChildren<TextMeshPro>().alpha = 0;
        }


        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit)) //Make sure you have EventSystem in the hierarchy before using EventSystem
        {
            highlight = raycastHit.transform;
            if (highlight.CompareTag("Selectable"))
            {
                //Debug.Log("Selectable");

                if (highlight.gameObject.GetComponent<OutlineComponent>() != null)
                {
                    highlight.gameObject.GetComponent<OutlineComponent>().enabled = true;
                    //Debug.Log("raycast hitting: " +gameObject.name);

                    for (int i = 0; i < opcoes.Length; i++)
                    {
                        if (opcoes[i].name == highlight.name)
                        {
                            if (opcoes[i].GetComponentInChildren<TextMeshPro>() != null)
                            {
                                opcoes[i].GetComponentInChildren<TextMeshPro>().alpha = 1;
                            }

                        }

                    }

                }
                else
                {

                    if (highlight.gameObject.GetComponent<MeshRenderer>().material != selectionMaterial)
                    {
                        originalMaterial = highlight.gameObject.GetComponent<MeshRenderer>().material;
                        highlight.gameObject.GetComponent<MeshRenderer>().material = selectionMaterial;
                    }
                }
            }
            else
            {
                highlight = null;
            }

        }

    }
    public void MakeObjectsSelectable()
    {
        timeline.tag = "Selectable";
        deckEvent.tag = "Selectable";
        deckRepair.tag = "Selectable";
    }

}
