using UnityEngine;
using TMPro;
using Photon.Pun;
using TimeCrax.Managers;

public class PersonDescriptionPopup : MonoBehaviour
{
    public static PersonDescriptionPopup Instance { get; private set; }

    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject challengeCanvas;

    private bool _isObserverMode = false;

    void Awake()
    {
        Instance = this;
        descriptionPanel.SetActive(false);
    }

    public void Open(string text)
    {
        _isObserverMode = false;
        descriptionText.text = text;
        descriptionPanel.SetActive(true);
        if (challengeCanvas != null) challengeCanvas.SetActive(false);
        InputBlocker.Block();
        Cursor.visible = true;
        OutlineAction.RequestHandCursor();
    }

    public void OpenForObserver(string text)
    {
        _isObserverMode = true;
        descriptionText.text = text;
        descriptionPanel.SetActive(true);
        if (challengeCanvas != null) challengeCanvas.SetActive(false);
        Cursor.visible = true;

        var cg = descriptionPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = descriptionPanel.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Close()
    {
        if (!descriptionPanel.activeSelf) return;
        if (_isObserverMode)
        {
            CloseForObserver();
            return;
        }
        descriptionPanel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
        InputBlocker.Unblock();
        OutlineAction.ReleaseHandCursor();

        var local = PlayerManager.Instance?.GetLocalPlayer();
        if (local != null && local.GetYourTurn())
        {
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null && PhotonNetwork.InRoom)
                gm.photonView.RPC("RPC_ClosePersonDescription", RpcTarget.Others);
        }
    }

    public void CloseForObserver()
    {
        if (!descriptionPanel.activeSelf) return;
        _isObserverMode = false;
        var cg = descriptionPanel.GetComponent<CanvasGroup>();
        if (cg != null) { cg.interactable = true; cg.blocksRaycasts = true; cg.alpha = 1f; }
        descriptionPanel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
    }
}
