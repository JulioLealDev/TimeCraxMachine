using UnityEngine;

public class TimelineColliderArea : MonoBehaviour
{
    private Timeline parentTimeline;
    private OutlineComponent parentOutline;

    private void Start()
    {
        parentTimeline = GetComponentInParent<Timeline>(true);

        if (parentTimeline != null)
            parentOutline = parentTimeline.GetComponent<OutlineComponent>();

    }

    private void OnMouseDown()
    {
        parentTimeline?.TriggerZoom();
    }

    private void OnMouseEnter()
    {
        if (parentOutline != null)
            parentOutline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (parentOutline != null)
            parentOutline.enabled = false;
    }

    public void SetUpTimelineCollider(bool activate)
    {
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            Debug.Log("[TimelineColliderArea] SetUpTimelineCollider(" +activate+ ")");
            collider.enabled = activate;
        }
    }
}
