using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Adiciona troca de cursor em elementos UI (Canvas).
/// Usa IPointerEnterHandler/IPointerExitHandler do EventSystem.
/// Requer que o componente Graphic do objeto tenha Raycast Target ativado.
/// </summary>
public class UICursorHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private OutlineAction outlineAction;

    private void Start()
    {
        outlineAction = FindFirstObjectByType<OutlineAction>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InputBlocker.IsBlocked) return;
        if (hoverCursor != null)
            Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outlineAction != null)
            outlineAction.ResetCursor();
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
