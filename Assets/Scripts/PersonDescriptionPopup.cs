using UnityEngine;
using TMPro;

public class PersonDescriptionPopup : MonoBehaviour
{
    public static PersonDescriptionPopup Instance { get; private set; }

    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text descriptionText;

    void Awake()
    {
        Instance = this;
        descriptionPanel.SetActive(false);
    }

    public void Open(string text)
    {
        descriptionText.text = text;
        descriptionPanel.SetActive(true);
        InputBlocker.Block();
        Cursor.visible = true;
    }

    public void Close()
    {
        descriptionPanel.SetActive(false);
        InputBlocker.Unblock();
    }
}
