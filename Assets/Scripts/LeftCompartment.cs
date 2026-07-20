using UnityEngine;

public class LeftCompartmentAnimation : MonoBehaviour
{
    [SerializeField] private DeckBonus deckBonus;

    public void ActivateDeckBonusCollider()
    {
        if (deckBonus == null) return;

        var players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.photonView.IsMine && player.GetYourTurn())
            {
                deckBonus.ActivateCollider();
                break;
            }
        }
    }
}