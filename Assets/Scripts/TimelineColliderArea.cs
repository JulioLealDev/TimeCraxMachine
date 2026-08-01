using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Managers;

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

        GameStateManager.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        GameStateManager.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(GamePhase previous, GamePhase next)
    {
        if (_selfCollider == null) return;

        if (next == GamePhase.IM_UnlockBonusDeck || next == GamePhase.IM_DrewBonusCard || next == GamePhase.Menu)
        {
            _selfCollider.enabled = false;
        }
        else if (next == GamePhase.IM_Turn || next == GamePhase.IM_FirstTurn)
        {
            // Re-enable only for the turn player when returning from bonus-deck phases
            if (!GameManager.IsMalfunctionPending)
            {
                var local = PlayerManager.Instance?.GetLocalPlayer();
                if (local != null && local.GetYourTurn())
                    _selfCollider.enabled = true;
            }
        }
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
        if (InputBlocker.IsBlocked) return;
        if (!IsMyTurn()) return;
        if (parentOutline != null)
            parentOutline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (parentOutline != null)
            parentOutline.enabled = false;
    }

    private bool IsMyTurn()
    {
        var local = PlayerManager.Instance?.GetLocalPlayer();
        return local != null && local.GetYourTurn();
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
