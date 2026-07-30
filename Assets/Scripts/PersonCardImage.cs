using UnityEngine;
using TMPro;
using Photon.Pun;
using TimeCrax.Managers;

public class PersonCardImage : MonoBehaviour
{
    [SerializeField] private TMP_Text personNameText;
    [SerializeField] private int slotIndex;
    [SerializeField] private Texture defaultTexture;

    public void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (!IsMyTurn()) return;

        var carousel = PersonsCarousel.Instance;
        if (carousel == null) return;

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            carousel.Open(renderer, personNameText, slotIndex);

            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null && PhotonNetwork.InRoom)
            {
                string imagePath = carousel.GetCurrentImagePath() ?? string.Empty;
                gm.photonView.RPC("RPC_OpenPersonsCarousel", RpcTarget.Others, imagePath);
            }
        }
    }

    private bool IsMyTurn()
    {
        var local = PlayerManager.Instance?.GetLocalPlayer();
        return local != null && local.GetYourTurn();
    }

    public void ResetToDefault()
    {
        var r = GetComponent<Renderer>();
        if (r != null) r.material.mainTexture = defaultTexture;
        if (personNameText != null) personNameText.text = string.Empty;
    }
}
