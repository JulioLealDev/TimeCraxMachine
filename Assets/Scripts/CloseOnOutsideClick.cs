using UnityEngine;
using UnityEngine.EventSystems;

public class CloseOnOutsideClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        PersonDescriptionPopup.Instance.Close();
    }
}
