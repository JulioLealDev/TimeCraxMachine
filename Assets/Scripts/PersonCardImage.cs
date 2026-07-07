using UnityEngine;
using TMPro;

public class PersonCardImage : MonoBehaviour
{
    [SerializeField] private TMP_Text personNameText;
    [SerializeField] private int slotIndex;
    [SerializeField] private Texture defaultTexture;

    public void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;

        var carousel = PersonsCarousel.Instance;
        if (carousel == null) return;

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            carousel.Open(renderer, personNameText, slotIndex);
    }

    public void ResetToDefault()
    {
        var r = GetComponent<Renderer>();
        if (r != null) r.material.mainTexture = defaultTexture;
        if (personNameText != null) personNameText.text = string.Empty;
    }
}
