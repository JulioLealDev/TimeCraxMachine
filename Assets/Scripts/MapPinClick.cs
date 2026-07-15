using UnityEngine;

public class MapPinClick : MonoBehaviour
{
    public int PinIndex { get; set; }

    private OutlineComponent outline;

    private void Awake()
    {
        outline = GetComponent<OutlineComponent>();
    }

    private void OnMouseEnter()
    {
        if (InputBlocker.IsBlocked) return;
        if (outline != null) outline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (outline != null) outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        MapAnswerChecker.Instance?.OnPinClicked(PinIndex);
    }
}
