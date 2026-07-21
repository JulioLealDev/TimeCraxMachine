using UnityEngine;
using TMPro;

public class PersonDescriptionPopup : MonoBehaviour
{
    public static PersonDescriptionPopup Instance { get; private set; }

    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject challengeCanvas;

    void Awake()
    {
        Instance = this;
        descriptionPanel.SetActive(false);
    }

    public void Open(string text)
    {
        descriptionText.text = text;
        descriptionPanel.SetActive(true);
        if (challengeCanvas != null) challengeCanvas.SetActive(false);
        InputBlocker.Block();
        Cursor.visible = true;
    }

    public void Close()
    {
        descriptionPanel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
        InputBlocker.Unblock();
    }
}
