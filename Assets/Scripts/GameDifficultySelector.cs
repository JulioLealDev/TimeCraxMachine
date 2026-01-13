using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TimeCrax.Core;

public class GameDifficultySelector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameDifficultyText;
    [SerializeField] private Image gameDifficultyImage;
    [SerializeField] private Sprite[] difficultySprites; // 0=Easy, 1=Normal, 2=Hard
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    public SoundEffects soundEffects;

    private static readonly string[] Difficulties = { "Easy", "Normal", "Hard" };
    private int currentIndex = 1;

    void Start()
    {
        UpdateDisplay();

        leftButton.onClick.AddListener(OnLeftClick);
        rightButton.onClick.AddListener(OnRightClick);
    }

    private void OnLeftClick()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = Difficulties.Length - 1;

        UpdateDisplay();
        soundEffects.PressHudButtonSound();
    }

    private void OnRightClick()
    {
        currentIndex++;
        if (currentIndex >= Difficulties.Length)
            currentIndex = 0;

        UpdateDisplay();
        soundEffects.PressHudButtonSound();
    }

    private void UpdateDisplay()
    {
        gameDifficultyText.text = Difficulties[currentIndex];
        SessionData.GameDifficulty = Difficulties[currentIndex];

        if (difficultySprites != null && currentIndex < difficultySprites.Length)
            gameDifficultyImage.sprite = difficultySprites[currentIndex];
    }

    void OnDestroy()
    {
        leftButton.onClick.RemoveListener(OnLeftClick);
        rightButton.onClick.RemoveListener(OnRightClick);
    }
}
