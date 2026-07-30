using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Photon.Pun;
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
    private bool _isObserverMode = false;

    void Awake()
    {
        _instance = this;
        panel.SetActive(false);
        prevButton.onClick.AddListener(OnPrev);
        nextButton.onClick.AddListener(OnNext);
        confirmButton.onClick.AddListener(OnConfirm);
    }

    public string GetCurrentImagePath()
    {
        if (shuffledEntries == null || shuffledEntries.Count == 0) return null;
        return shuffledEntries[currentIndex]?.localImagePath;
    }

    public void OpenForObserver(string imagePath)
    {
        _isObserverMode = true;
        prevButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(imagePath))
        {
            var texture = ThemeStorage.LoadLocalImage(imagePath);
            if (texture != null)
            {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                currentImage.sprite = sprite;
                currentImage.enabled = true;
            }
            else
            {
                currentImage.sprite = null;
                currentImage.enabled = false;
            }
        }
        else
        {
            currentImage.sprite = null;
            currentImage.enabled = false;
        }

        panel.SetActive(true);
        if (challengeCanvas != null) challengeCanvas.SetActive(false);
        Cursor.visible = true;
    }

    public void Open(Renderer target, TMP_Text nameText, int slotIndex)
    {
        _isObserverMode = false;
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
        BroadcastCurrentImage();
    }

    private void OnNext()
    {
        currentIndex = (currentIndex + 1) % loadedSprites.Length;
        ShowCurrent();
        BroadcastCurrentImage();
    }

    private void BroadcastCurrentImage()
    {
        if (!PhotonNetwork.InRoom) return;
        string imagePath = shuffledEntries?[currentIndex]?.localImagePath ?? string.Empty;
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
            gm.photonView.RPC("RPC_UpdatePersonsCarouselImage", RpcTarget.Others, imagePath);
    }

    public void UpdateObserverImage(string imagePath)
    {
        if (!_isObserverMode || string.IsNullOrEmpty(imagePath)) return;
        var texture = ThemeStorage.LoadLocalImage(imagePath);
        if (texture == null) return;
        currentImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        currentImage.enabled = true;
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
        if (_isObserverMode)
        {
            CloseForObserver();
            return;
        }
        panel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
        InputBlocker.Unblock();
        OutlineAction.ReleaseHandCursor();
    }

    public void CloseForObserver()
    {
        if (!panel.activeSelf) return;
        _isObserverMode = false;
        prevButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        currentImage.enabled = true;
        panel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
    }

    private void OnConfirm()
    {
        if (targetRenderer == null || loadedSprites == null) return;

        var texture = loadedSprites[currentIndex]?.texture;
        if (texture != null)
            targetRenderer.material.mainTexture = texture;

        string personName = shuffledEntries[currentIndex].name;
        if (targetNameText != null)
            targetNameText.text = personName;

        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null && PhotonNetwork.InRoom)
            gm.photonView.RPC("RPC_PersonsSlotAssigned", RpcTarget.Others, currentSlotIndex, personName);

        panel.SetActive(false);
        if (challengeCanvas != null) challengeCanvas.SetActive(true);
        InputBlocker.Unblock();
        OutlineAction.ReleaseHandCursor();
        PersonsAnswerChecker.Instance?.OnSlotAssigned(currentSlotIndex);
        targetRenderer = null;
        targetNameText = null;
    }
}
