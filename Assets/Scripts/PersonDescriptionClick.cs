using UnityEngine;
using TMPro;
using Photon.Pun;
using TimeCrax.Managers;

public class PersonDescriptionClick : MonoBehaviour
{
    [SerializeField] private TMP_Text sourceText;

    void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (!IsMyTurn()) return;
        if (PersonDescriptionPopup.Instance == null) return;
        if (sourceText == null) return;

        string text = sourceText.text;
        PersonDescriptionPopup.Instance.Open(text);

        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null && PhotonNetwork.InRoom)
            gm.photonView.RPC("RPC_OpenPersonDescription", RpcTarget.Others, text);
    }

    private bool IsMyTurn()
    {
        var local = PlayerManager.Instance?.GetLocalPlayer();
        return local != null && local.GetYourTurn();
    }
}
