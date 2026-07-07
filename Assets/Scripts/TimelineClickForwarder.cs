using UnityEngine;
using TimeCrax.Core;

/// <summary>
/// Auto-adicionado ao pai de um TimelineChild quando o pai tem Collider mas
/// não tem TimelineChild nem este forwarder. Delega cliques e hover do pai para o filho.
/// </summary>
public class TimelineClickForwarder : MonoBehaviour
{
    private TimelineChild target;
    private OutlineComponent outline;

    public void Initialize(TimelineChild tc)
    {
        target = tc;
        outline = tc.GetParentOutline();
        DebugHelper.Log($"[TimelineClickForwarder] Inicializado em '{name}' → target='{tc.name}', outline={outline?.name ?? "NULL"}");
    }

    private void OnMouseDown()
    {
        if (target != null && target.enabled)
            target.OnMouseDown();
    }

    private void OnMouseOver()
    {
        if (target == null || !target.enabled) return;

        // Buscar outline se ainda não foi resolvido
        if (outline == null)
            outline = target.GetParentOutline();

        if (outline != null)
            outline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (outline != null)
            outline.enabled = false;
    }
}
