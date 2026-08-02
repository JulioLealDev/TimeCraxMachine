using UnityEngine;
using TimeCrax.Managers;


public class MapPinClick : MonoBehaviour
{
    public int PinIndex { get; set; }

    private HoverMaterialAdder _hover;
    [SerializeField] private SoundEffects soundEffects;

    private void Awake()
    {
        _hover = GetComponent<HoverMaterialAdder>();
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (!IsMyTurn()) return;
        soundEffects.PlayClickSlotSound();
        _hover?.LockHover();
        MapAnswerChecker.Instance?.OnPinClicked(PinIndex);
    }

    public void ResetHoverLock()
    {
        _hover?.UnlockHover();
    }

    private bool IsMyTurn()
    {
        var local = PlayerManager.Instance?.GetLocalPlayer();
        return local != null && local.GetYourTurn();
    }
}
