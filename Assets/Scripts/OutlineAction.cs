using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using TimeCrax.Core;

public class OutlineAction : MonoBehaviour
{
    public GameObject menuStart;
    public GameObject timeline;
    public GameObject deckEvent;
    public GameObject deckBonus;

    [Header("Hover Settings")]
    [SerializeField] private float hoverSmoothness = 1f;
    [SerializeField] private float hoverMetallic = 0.5f;

    [Header("Cursors")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D malfunctionCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    private Transform highlight;
    private RaycastHit raycastHit;
    private bool isShowingMalfunctionCursor = false;

    // Valores originais do material para restaurar apos hover
    private float originalSmoothness;
    private float originalMetallic;
    private Material highlightedMaterial;


    void Start()
    {
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
        }
    }

    void Update()
    {
        // Restaurar material/outline anterior quando mouse sair
        if (highlight != null)
        {
            if (highlight.gameObject.GetComponent<OutlineComponent>() != null)
            {
                highlight.gameObject.GetComponent<OutlineComponent>().enabled = false;
                highlight = null;
            }
            else
            {
                if (highlightedMaterial != null)
                {
                    highlightedMaterial.SetFloat("_Glossiness", originalSmoothness);
                    highlightedMaterial.SetFloat("_Metallic", originalMetallic);
                    highlightedMaterial = null;
                }
                highlight = null;
            }
        }

        if (InputBlocker.IsBlocked) return;

        bool shouldShowMalfunctionCursor = false;

        Transform[] opcoes = menuStart.GetComponentsInChildren<Transform>();
        for (int i = 0; i < opcoes.Length; i++)
        {
            var tmp = opcoes[i].GetComponentInChildren<TextMeshPro>();
            if (tmp != null) tmp.alpha = 0;
        }

        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            highlight = raycastHit.transform;
            if (highlight.CompareTag("Selectable"))
            {
                if (highlight.gameObject.GetComponent<OutlineComponent>() != null)
                {
                    highlight.gameObject.GetComponent<OutlineComponent>().enabled = true;

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

                    var machineComponent = highlight.gameObject.GetComponent<MachineComponent>();
                    if (machineComponent != null && machineComponent.malfunctions == 1)
                    {
                        shouldShowMalfunctionCursor = true;
                    }
                }
                else
                {
                    var renderer = highlight.gameObject.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        if (highlightedMaterial != renderer.material)
                        {
                            highlightedMaterial = renderer.material;
                            originalSmoothness = highlightedMaterial.GetFloat("_Glossiness");
                            originalMetallic = highlightedMaterial.GetFloat("_Metallic");

                            highlightedMaterial.SetFloat("_Glossiness", hoverSmoothness);
                            highlightedMaterial.SetFloat("_Metallic", hoverMetallic);
                        }
                    }
                }
            }
            else
            {
                highlight = null;
            }
        }

        UpdateCursor(shouldShowMalfunctionCursor);
    }

    private void UpdateCursor(bool showMalfunctionCursor)
    {
        if (showMalfunctionCursor && !isShowingMalfunctionCursor)
        {
            if (malfunctionCursor != null)
            {
                Cursor.SetCursor(malfunctionCursor, cursorHotspot, CursorMode.Auto);
                isShowingMalfunctionCursor = true;
            }
        }
        else if (!showMalfunctionCursor && isShowingMalfunctionCursor)
        {
            if (defaultCursor != null)
            {
                Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            isShowingMalfunctionCursor = false;
        }
    }

    public void MakeObjectsSelectable()
    {
        timeline.tag = "Selectable";
        deckEvent.tag = "Selectable";
    }

    public void ResetCursor()
    {
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        isShowingMalfunctionCursor = false;
    }

    private void OnDisable()
    {
        if (highlightedMaterial != null)
        {
            highlightedMaterial.SetFloat("_Glossiness", originalSmoothness);
            highlightedMaterial.SetFloat("_Metallic", originalMetallic);
            highlightedMaterial = null;
        }

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        isShowingMalfunctionCursor = false;
    }
}
