using UnityEngine;
using TimeCrax.Core;

/// <summary>
/// Script para filhos da NewTimeline.
/// Delega hover (outline) e clique para o objeto pai.
/// </summary>
public class TimelineChild : MonoBehaviour
{
    [Tooltip("Referência ao objeto pai NewTimeline (será buscado automaticamente se não configurado)")]
    [SerializeField] private Timeline parentTimeline;
    [SerializeField] private OutlineComponent parentOutline;

    void Start()
    {
        if (parentTimeline == null)
            parentTimeline = GetComponentInParent<Timeline>(true);

        // Buscar outline no mesmo objeto que tem o Timeline, independente da profundidade
        // na hierarquia — evita pegar OutlineComponent de um objeto intermediário (ex: LeftDoor)
        if (parentOutline == null)
        {
            if (parentTimeline != null)
                parentOutline = parentTimeline.GetComponent<OutlineComponent>();
        }
        if (parentOutline == null)
            parentOutline = GetComponentInParent<OutlineComponent>(true);

        // Garantir que este objeto está marcado como Selectable para o OutlineAction detectar hover
        if (!gameObject.CompareTag("Selectable"))
            gameObject.tag = "Selectable";

        var parent = transform.parent;
        if (parent != null
            && parent.GetComponent<Collider>() != null
            && parent.GetComponent<TimelineChild>() == null
            && parent.GetComponent<TimelineClickForwarder>() == null)
        {
            var forwarder = parent.gameObject.AddComponent<TimelineClickForwarder>();
            forwarder.Initialize(this);
            DebugHelper.Log($"[TimelineChild] Adicionou TimelineClickForwarder em '{parent.name}' → '{name}'");
        }
        DebugHelper.Log($"[TimelineChild] '{name}': parentTimeline={parentTimeline?.name ?? "NULL"}, parentOutline={parentOutline?.name ?? "NULL"}");
    }

    /// <summary>
    /// Retorna o OutlineComponent do pai para ativação pelo OutlineAction
    /// </summary>
    public OutlineComponent GetParentOutline()
    {
        return parentOutline;
    }

    private void OnMouseOver()
    {
        if (parentOutline != null)
            parentOutline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (parentOutline != null)
            parentOutline.enabled = false;
    }

    /// <summary>
    /// Delega o clique para o Timeline do pai
    /// </summary>
    public void OnMouseDown()
    {
        if (parentTimeline != null)
        {
            parentTimeline.OnMouseDown();
        }
    }
}
