using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TimeCrax.Themes;

public class PersonsCarousel : MonoBehaviour
{
    private static PersonsCarousel _instance;
    public static PersonsCarousel Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<PersonsCarousel>(FindObjectsInactive.Include);
            return _instance;
        }
    }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject challengeCanvas;
    [SerializeField] private Image currentImage;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button confirmButton;

    private Renderer targetRenderer;
    private TMP_Text targetNameText;
    private List<PersonEntry> shuffledEntries;
    private Sprite[] loadedSprites;
    private int currentIndex;
    private int currentSlotIndex;

    void Awake()
    {
        _instance = this;
        panel.SetActive(false);
        prevButton.onClick.AddListener(OnPrev);
        nextButton.onClick.AddListener(OnNext);
        confirmButton.onClick.AddListener(OnConfirm);
    }

    public void Open(Renderer target, TMP_Text nameText, int slotIndex)
    {
        var themeCard = GameManager.CurrentPersonsThemeCard;
        if (themeCard?.persons?.entries == null || themeCard.persons.entries.Count == 0) return;

        targetRenderer = target;
        targetNameText = nameText;
        currentSlotIndex = slotIndex;
        currentIndex = 0;

        shuffledEntries = themeCard.persons.entries
            .OrderBy(_ => Random.value)
            .ToList();

        loadedSprites = new Sprite[shuffledEntries.Count];

        for (int i = 0; i < shuffledEntries.Count; i++)
        {
            var texture = ThemeStorage.LoadLocalImage(shuffledEntries[i].localImagePath);
            if (texture != null)
                loadedSprites[i] = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.one * 0.5f
                );
        }

        ShowCurrent();
        panel.SetActive(true);
        if (challengeCanvas != null) challengeCanvas.SetActive(false);
        InputBlocker.Block();
        Cursor.visible = true;
        OutlineAction.RequestHandCursor();
    }

    private void OnPrev()
    {
        currentIndex = (currentIndex - 1 + loadedSprites.Length) % loadedSprites.Length;
        ShowCurrent();
    }

    private void OnNext()
    {
        currentIndex = (currentIndex + 1) % loadedSprites.Length;
        ShowCurrent();
    }

    private void ShowCurrent()
    {
        var sprite = loadedSprites[currentIndex];
        currentImage.sprite = sprite;
        currentImage.enabled = sprite != null;
    }

    public void Close()
    {
        if (!panel.activeSelf) return;
        panel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
        InputBlocker.Unblock();
        OutlineAction.ReleaseHandCursor();
    }

    private void OnConfirm()
    {
        if (targetRenderer == null || loadedSprites == null) return;

        var texture = loadedSprites[currentIndex]?.texture;
        if (texture != null)
            targetRenderer.material.mainTexture = texture;

        if (targetNameText != null)
            targetNameText.text = shuffledEntries[currentIndex].name;

        panel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
        InputBlocker.Unblock();
        OutlineAction.ReleaseHandCursor();
        PersonsAnswerChecker.Instance?.OnSlotAssigned(currentSlotIndex);
        targetRenderer = null;
        targetNameText = null;
    }
}
