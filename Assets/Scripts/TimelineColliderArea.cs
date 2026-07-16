using UnityEngine;

public class TimelineColliderArea : MonoBehaviour
{
    private Timeline parentTimeline;
    private OutlineComponent parentOutline;
    private Collider _selfCollider;
    private bool _prevColliderEnabled;

    private void Start()
    {
        parentTimeline = GetComponentInParent<Timeline>(true);

        if (parentTimeline != null)
            parentOutline = parentTimeline.GetComponent<OutlineComponent>();

        _selfCollider = GetComponent<Collider>();
        _prevColliderEnabled = _selfCollider != null && _selfCollider.enabled;
    }

    private void LateUpdate()
    {
        if (_selfCollider == null) return;
        bool current = _selfCollider.enabled;
        if (current && !_prevColliderEnabled)
            Debug.Log($"[TimelineColliderArea] *** COLLIDER ATIVADO *** — IsMalfunctionPending={GameManager.IsMalfunctionPending}, frame={Time.frameCount}", this);
        else if (!current && _prevColliderEnabled)
            Debug.Log($"[TimelineColliderArea] collider desativado — frame={Time.frameCount}", this);
        _prevColliderEnabled = current;
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
