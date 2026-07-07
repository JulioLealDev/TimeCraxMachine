using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;

public class PlayerCountSelector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Image playerCountImage;
    [SerializeField] private Sprite[] playerCountSprites; // 0=1Player, 1=2Players, 2=3Players, 3=4Players
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    public SoundEffects soundEffects;

    private int currentCount = 4;
    private const int MinPlayers = 1;
    private const int MaxPlayers = 4;

    void Start()
    {
        currentCount = 4;
        UpdateDisplay();

        leftButton.onClick.AddListener(OnLeftClick);
        rightButton.onClick.AddListener(OnRightClick);
    }

    private void OnLeftClick()
    {
        currentCount--;
        if (currentCount < MinPlayers)
            currentCount = MaxPlayers;

        UpdateDisplay();
        soundEffects.PressHudButtonSound();
    }

    private void OnRightClick()
    {
        currentCount++;
        if (currentCount > MaxPlayers)
            currentCount = MinPlayers;

        UpdateDisplay();
        soundEffects.PressHudButtonSound();
    }

    private void UpdateDisplay()
    {
        string playerWord = currentCount == 1 ? "Player" : "Players";
        playerCountText.text = $"{currentCount} {playerWord}";
        SessionData.NumberOfPlayers = currentCount;

        int spriteIndex = currentCount - 1; // 1-4 → 0-3
        if (playerCountSprites != null && spriteIndex < playerCountSprites.Length)
            playerCountImage.sprite = playerCountSprites[spriteIndex];
    }

    void OnDestroy()
    {
        leftButton.onClick.RemoveListener(OnLeftClick);
        rightButton.onClick.RemoveListener(OnRightClick);
    }
}
