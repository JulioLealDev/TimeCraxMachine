using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using TimeCrax.Core;

public class OutlineAction : MonoBehaviour
{
    public Material originalMaterial;
    public Material selectionMaterial;
    public GameObject menuStart;
    public GameObject timeline;
    public GameObject deckEvent;
    public GameObject deckBonus;

    [Header("Cursors")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D malfunctionCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    private Transform highlight;
    private RaycastHit raycastHit;
    private bool isShowingMalfunctionCursor = false;

    void Start()
    {
        // Definir cursor padrão no início
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
        }
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

        // Resetar cursor para padrão quando não está sobre componente com malfunction
        bool shouldShowMalfunctionCursor = false;

        Transform[] opcoes = menuStart.GetComponentsInChildren<Transform>();
        for (int i = 0; i < opcoes.Length; i++)
        {
            opcoes[i].GetComponentInChildren<TextMeshPro>().alpha = 0;
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

                    // Verificar se é um componente com malfunction = 1
                    var machineComponent = highlight.gameObject.GetComponent<MachineComponent>();
                    if (machineComponent != null && machineComponent.malfunctions == 1)
                    {
                        shouldShowMalfunctionCursor = true;
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

        // Atualizar cursor baseado no estado
        UpdateCursor(shouldShowMalfunctionCursor);
    }

    /// <summary>
    /// Atualiza o cursor baseado no estado atual
    /// </summary>
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
        // deckBonus só fica selecionável após acertar quiz
    }

    /// <summary>
    /// Reseta o cursor para o padrão
    /// </summary>
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
        // Resetar cursor ao desativar
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        isShowingMalfunctionCursor = false;
    }
}
