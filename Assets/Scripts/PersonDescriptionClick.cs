using UnityEngine;
using TMPro;

public class PersonDescriptionClick : MonoBehaviour
{
    [SerializeField] private TMP_Text sourceText;

    void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (PersonDescriptionPopup.Instance == null) return;
        if (sourceText == null) return;

        PersonDescriptionPopup.Instance.Open(sourceText.text);
    }
}
