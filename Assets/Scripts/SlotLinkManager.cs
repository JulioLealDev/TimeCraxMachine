using UnityEngine;
using TimeCrax.Core;

public class SlotLinkManager : MonoBehaviour
{
    private static SlotLinkManager _instance;
    public static SlotLinkManager Instance => _instance;

    [SerializeField] private MeshRenderer firstLink01;
    [SerializeField] private MeshRenderer firstLink02;
    [SerializeField] private MeshRenderer secondLink01;
    [SerializeField] private MeshRenderer secondLink02;
    [SerializeField] private MeshRenderer thirdLink01;
    [SerializeField] private MeshRenderer thirdLink02;
    [SerializeField] private MeshRenderer fourthLink01;
    [SerializeField] private MeshRenderer fourthLink02;
    [SerializeField] private MeshRenderer fifthLink01;
    [SerializeField] private MeshRenderer fifthLink02;

    private void Awake() => _instance = this;

    // Verifica os slots adjacentes ao slot respondido corretamente e ativa os links correspondentes.
    public void CheckAndActivateLinks(int answeredSlotIndex)
    {
        var slots = FindObjectsByType<EventSlot>(FindObjectsSortMode.None);
        var filledSlots = new System.Collections.Generic.HashSet<int>();
        foreach (var slot in slots)
            if (slot.CompareTag("Disabled"))
                filledSlots.Add(slot.SlotNumber);

        int lo = answeredSlotIndex - 1;
        int hi = answeredSlotIndex + 1;

        if (lo >= 1 && filledSlots.Contains(lo))
            this.DelayedCall(2f, () => ActivateLinksForPair(lo));

        if (hi <= 6 && filledSlots.Contains(hi))
            this.DelayedCall(2f, () => ActivateLinksForPair(answeredSlotIndex));
    }

    // lowerSlotIndex é o menor dos dois índices adjacentes preenchidos (1-based).
    // Mapeamento: (1,2)→first | (2,3)→second | (3,4)→third | (4,5)→fourth | (5,6)→fifth
    private void ActivateLinksForPair(int lowerSlotIndex)
    {
        switch (lowerSlotIndex)
        {
            case 1: Enable(firstLink01);  Enable(firstLink02);  break;
            case 2: Enable(secondLink01); Enable(secondLink02); break;
            case 3: Enable(thirdLink01);  Enable(thirdLink02);  break;
            case 4: Enable(fourthLink01); Enable(fourthLink02); break;
            case 5: Enable(fifthLink01);  Enable(fifthLink02);  break;
        }
    }

    private void Enable(MeshRenderer mr)
    {
        if (mr != null) mr.enabled = true;
    }
}
