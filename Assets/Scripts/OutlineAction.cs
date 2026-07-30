using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using TimeCrax.Core;
using TimeCrax.Managers;

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
    [SerializeField] private Texture2D inspectCursor;
    [SerializeField] private Texture2D handCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    private Transform highlight;
    private RaycastHit raycastHit;
    private bool isShowingMalfunctionCursor = false;
    private bool isShowingInspectCursor = false;
    private bool isShowingHandCursor = false;

    // Cursor forçado por painéis UI (BonusCardCanvas, PersonsImagesPanel, PersonsDescriptionPanel)
    private static int s_panelHandCursorCount = 0;
    public static void RequestHandCursor() => s_panelHandCursorCount++;
    public static void ReleaseHandCursor() { if (s_panelHandCursorCount > 0) s_panelHandCursorCount--; }

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

        // Cursor forçado por painéis UI — ignora detecção 3D
        if (s_panelHandCursorCount > 0)
        {
            if (!isShowingHandCursor)
            {
                if (handCursor != null)
                    Cursor.SetCursor(handCursor, cursorHotspot, CursorMode.Auto);
                isShowingHandCursor = true;
                isShowingInspectCursor = false;
                isShowingMalfunctionCursor = false;
            }
            return;
        }

        if (InputBlocker.IsBlocked) return;

        bool shouldShowMalfunctionCursor = false;
        bool shouldShowInspectCursor = false;
        bool shouldShowHandCursor = false;

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
            if (!IsMyTurn() && IsChallengeObject(highlight))
            {
                highlight = null;
            }
            else if (IsInspectTarget(highlight))
            {
                shouldShowInspectCursor = true;
                var oc = highlight.gameObject.GetComponent<OutlineComponent>();
                if (oc != null)
                    oc.enabled = true;
                else
                    highlight = null;
            }
            else
            {
                if (IsHandCursorTarget(highlight))
                    shouldShowHandCursor = true;

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
        }

        UpdateCursor(shouldShowMalfunctionCursor, shouldShowInspectCursor, shouldShowHandCursor);
    }

    private bool IsChallengeObject(Transform t) =>
        t.GetComponent<MapPinClick>()            != null ||
        t.GetComponent<PersonCardImage>()        != null ||
        t.GetComponent<PersonDescriptionClick>() != null;

    private bool IsMyTurn()
    {
        var local = PlayerManager.Instance?.GetLocalPlayer();
        return local != null && local.GetYourTurn();
    }

    private bool IsInspectTarget(Transform t) =>
        t.GetComponent<TimelineColliderArea>() != null ||
        t.GetComponent<PersonDescriptionClick>() != null ||
        t.GetComponent<BonusCard>()              != null;

    private bool IsHandCursorTarget(Transform t) =>
        t.GetComponent<MapPinClick>()    != null ||
        t.GetComponent<PersonCardImage>() != null ||
        t.GetComponent<DeckBonus>()      != null ||
        t.GetComponent<DeckEvent>()      != null ||
        t.GetComponent<QuitInGaming>()   != null ||
        t.GetComponent<FinishTurn>()     != null ||
        t.GetComponent<EventSlot>()      != null;

    private void UpdateCursor(bool showMalfunctionCursor, bool showInspectCursor, bool showHandCursor)
    {
        if (showInspectCursor)
        {
            if (!isShowingInspectCursor)
            {
                Cursor.SetCursor(inspectCursor, cursorHotspot, CursorMode.Auto);
                isShowingInspectCursor = true;
                isShowingMalfunctionCursor = false;
                isShowingHandCursor = false;
            }
        }
        else if (showMalfunctionCursor)
        {
            if (!isShowingMalfunctionCursor)
            {
                if (malfunctionCursor != null)
                    Cursor.SetCursor(malfunctionCursor, cursorHotspot, CursorMode.Auto);
                isShowingMalfunctionCursor = true;
                isShowingInspectCursor = false;
                isShowingHandCursor = false;
            }
        }
        else if (showHandCursor)
        {
            if (!isShowingHandCursor)
            {
                if (handCursor != null)
                    Cursor.SetCursor(handCursor, cursorHotspot, CursorMode.Auto);
                isShowingHandCursor = true;
                isShowingInspectCursor = false;
                isShowingMalfunctionCursor = false;
            }
        }
        else
        {
            if (isShowingMalfunctionCursor || isShowingInspectCursor || isShowingHandCursor)
            {
                Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
                isShowingMalfunctionCursor = false;
                isShowingInspectCursor = false;
                isShowingHandCursor = false;
            }
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
        isShowingInspectCursor = false;
        isShowingHandCursor = false;
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
        isShowingInspectCursor = false;
        isShowingHandCursor = false;
    }
}
