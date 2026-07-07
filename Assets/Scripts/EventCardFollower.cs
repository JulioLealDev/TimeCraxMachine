using UnityEngine;

/// <summary>
/// Mantém o eventCard na raiz da cena mas faz com que ele acompanhe
/// o movimento do slot ao qual foi fixado (ex: zoom da timeline).
/// Chame AttachToSlot() ao final da animação draw_eventCard_to_slot0X.
/// </summary>
public class EventCardFollower : MonoBehaviour
{
    private Transform anchorSlot;
    private Vector3 localOffset;

    /// <summary>
    /// Registra o slot e calcula o offset da carta em relação a ele.
    /// Deve ser chamado quando a carta já está na posição final (pós-animação).
    /// </summary>
    public void AttachToSlot(Transform slot)
    {
        anchorSlot = slot;
        localOffset = slot.InverseTransformPoint(transform.position);
    }

    public void Detach()
    {
        anchorSlot = null;
    }

    void LateUpdate()
    {
        if (anchorSlot == null) return;
        transform.position = anchorSlot.TransformPoint(localOffset);
    }
}
