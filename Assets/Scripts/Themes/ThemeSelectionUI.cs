using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    public class ThemeSelectionUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject themeSelectionCanvas;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Grid")]
        [SerializeField] private Transform themeGrid;
        [SerializeField] private GameObject themeCardPrefab;

        [Header("Pagination")]
        [SerializeField] private Button leftArrow;
        [SerializeField] private Button rightArrow;
        [SerializeField] private TextMeshProUGUI pageText;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;

        [Header("Search")]
        [SerializeField] private TMP_InputField searchInput;

        [Header("Footer")]
        [SerializeField] private TextMeshProUGUI storageInfoText;
        [SerializeField] private TextMeshProUGUI selectedThemeText;

        [Header("Settings")]
        private const int cardsPerPage = 8;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;

        // Estado
        private int currentPage = 1;
        private int totalPages = 1;
        private List<ThemeCardUI> cardInstances = new List<ThemeCardUI>();
        private List<ThemeListItem> allThemes = new List<ThemeListItem>();
        private List<ThemeListItem> filteredThemes = new List<ThemeListItem>();
        private string currentDownloadingThemeId;
        private string selectedThemeId;
        private string currentSearchText = "";

        private static ThemeSelectionUI _instance;
        public SoundEffects soundEffects;
        public static ThemeSelectionUI Instance => _instance;

        public event Action OnPanelClosed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            SetupButtons();

            if (themeSelectionCanvas != null)
                themeSelectionCanvas.SetActive(false);
        }

        private void Start()
        {
            // Inscrever nos eventos de download
            var downloader = ThemeDownloader.Instance;
            if (downloader != null)
            {
                downloader.OnDownloadProgress += OnDownloadProgress;
                downloader.OnDownloadStatus += OnDownloadStatus;
            }

            // Verificar se já há tema selecionado
            if (ThemeManager.Instance != null && ThemeManager.Instance.HasSelectedTheme)
                selectedThemeId = ThemeManager.Instance.SelectedTheme.id;
        }

        private void OnDestroy()
        {
            var downloader = ThemeDownloader.Instance;
            if (downloader != null)
            {
                downloader.OnDownloadProgress -= OnDownloadProgress;
                downloader.OnDownloadStatus -= OnDownloadStatus;
            }
        }

        private void SetupButtons()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);

            if (leftArrow != null)
                leftArrow.onClick.AddListener(OnPrevPage);

            if (rightArrow != null)
                rightArrow.onClick.AddListener(OnNextPage);

            if (searchInput != null)
                searchInput.onValueChanged.AddListener(OnSearchChanged);
        }

        private void OnCloseButtonClicked()
        {
            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            Hide();
        }

        #region Public Methods

        public void Show()
        {
            if (themeSelectionCanvas == null) return;

            themeSelectionCanvas.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = true;

                LeanTween.alphaCanvas(canvasGroup, 1f, fadeInDuration)
                    .setOnComplete(() =>
                    {
                        canvasGroup.interactable = true;
                    });
            }

            currentPage = 1;
            currentSearchText = "";
            if (searchInput != null)
                searchInput.text = "";

            LoadThemesFromAPI();

            DebugHelper.Log("[ThemeSelectionUI] Show");
        }

        public void Hide()
        {
            if (themeSelectionCanvas == null) return;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;

                LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                    .setOnComplete(() =>
                    {
                        themeSelectionCanvas.SetActive(false);
                        gameObject.SetActive(false);
                    });
            }
            else
            {
                themeSelectionCanvas.SetActive(false);
                gameObject.SetActive(false);
            }

            DebugHelper.Log("[ThemeSelectionUI] Hide");

            OnPanelClosed?.Invoke();
        }

        public bool IsVisible()
        {
            return themeSelectionCanvas != null && themeSelectionCanvas.activeSelf;
        }

        #endregion

        #region Loading

        private void ShowLoading(string message = "Carregando...")
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(true);

            if (loadingText != null)
                loadingText.text = message;
        }

        private void HideLoading()
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
        }

        #endregion

        #region API Loading

        private void LoadThemesFromAPI()
        {
            ShowLoading("Carregando temas...");

            ThemeManager.Instance.GetAvailableThemes(1, 100, result =>
            {
                HideLoading();

                if (result.Success && result.Data != null)
                {
                    allThemes = result.Data.items ?? new List<ThemeListItem>();
                    ApplyFilter();

                    DebugHelper.Log($"[ThemeSelectionUI] Loaded {allThemes.Count} themes");

                    RefreshGrid();
                    UpdateFooter();
                }
                else
                {
                    DebugHelper.Log($"[ThemeSelectionUI] Failed to load themes: {result.ErrorMessage}");
                    // Mostrar mensagem de erro na UI
                    if (loadingText != null)
                    {
                        loadingText.text = result.ErrorMessage ?? "Erro ao carregar temas";
                        if (loadingOverlay != null)
                            loadingOverlay.SetActive(true);
                    }
                }
            });
        }

        #endregion

        #region Grid Management

        private void RefreshGrid()
        {
            ClearGrid();

            if (themeCardPrefab == null || themeGrid == null)
            {
                DebugHelper.Log("[ThemeSelectionUI] Missing prefab or grid reference");
                return;
            }

            int startIndex = (currentPage - 1) * cardsPerPage;
            int endIndex = Mathf.Min(startIndex + cardsPerPage, filteredThemes.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var theme = filteredThemes[i];
                var cardGO = Instantiate(themeCardPrefab, themeGrid);
                var cardUI = cardGO.GetComponent<ThemeCardUI>();

                if (cardUI != null)
                {
                    bool isDownloaded = ThemeManager.Instance.IsThemeDownloaded(theme.id);
                    bool isSelected = theme.id == selectedThemeId;

                    cardUI.Setup(theme, isDownloaded, isSelected);
                    cardUI.OnDownloadRequested += OnDownloadRequested;
                    cardUI.OnThemeSelected += OnThemeSelected;

                    cardInstances.Add(cardUI);
                }
            }

            UpdatePagination();
        }

        private void ClearGrid()
        {
            foreach (var card in cardInstances)
            {
                if (card != null)
                {
                    card.OnDownloadRequested -= OnDownloadRequested;
                    card.OnThemeSelected -= OnThemeSelected;
                    Destroy(card.gameObject);
                }
            }
            cardInstances.Clear();
        }

        #endregion

        #region Pagination

        private void UpdatePagination()
        {
            if (pageText != null)
                pageText.text = $"{currentPage}/{totalPages}";

            if (leftArrow != null)
                leftArrow.interactable = currentPage > 1;

            if (rightArrow != null)
                rightArrow.interactable = currentPage < totalPages;
        }

        private void OnPrevPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshGrid();
            }
            soundEffects.PressHudButtonSound();

        }

        private void OnNextPage()
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                RefreshGrid();
            }
            soundEffects.PressHudButtonSound();

        }

        #endregion

        #region Search

        private void OnSearchChanged(string searchText)
        {
            currentSearchText = searchText;
            currentPage = 1;
            ApplyFilter();
            RefreshGrid();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(currentSearchText))
            {
                filteredThemes = new List<ThemeListItem>(allThemes);
            }
            else
            {
                string searchLower = currentSearchText.ToLower().Trim();
                filteredThemes = allThemes.FindAll(theme =>
                    (theme.name != null && theme.name.ToLower().Contains(searchLower)) ||
                    (theme.creatorName != null && theme.creatorName.ToLower().Contains(searchLower))
                );
            }

            totalPages = Mathf.CeilToInt((float)filteredThemes.Count / cardsPerPage);
            if (totalPages < 1) totalPages = 1;
            if (currentPage > totalPages) currentPage = totalPages;

            UpdatePagination();
        }

        #endregion

        #region Theme Actions

        private void OnDownloadRequested(string themeId)
        {
            if (!string.IsNullOrEmpty(currentDownloadingThemeId))
            {
                DebugHelper.Log("[ThemeSelectionUI] Already downloading a theme");
                return;
            }

            currentDownloadingThemeId = themeId;

            // Encontrar o card e iniciar visual de download
            var card = cardInstances.Find(c => c.GetThemeId() == themeId);
            if (card != null)
                card.StartDownload();

            // Iniciar download
            ThemeManager.Instance.DownloadTheme(themeId, (success, error) =>
            {
                var downloadedCard = cardInstances.Find(c => c.GetThemeId() == themeId);
                if (downloadedCard != null)
                    downloadedCard.FinishDownload(success);

                if (!success)
                    DebugHelper.Log($"[ThemeSelectionUI] Download failed: {error}");

                currentDownloadingThemeId = null;
                UpdateFooter();
            });
        }

        private void OnThemeSelected(string themeId)
        {
            // Desmarcar anterior
            foreach (var card in cardInstances)
            {
                card.SetSelected(false);
            }

            // Selecionar novo
            selectedThemeId = themeId;
            ThemeManager.Instance.SelectTheme(themeId);

            // Marcar card selecionado
            var selectedCard = cardInstances.Find(c => c.GetThemeId() == themeId);
            if (selectedCard != null)
                selectedCard.SetSelected(true);

            UpdateFooter();

            DebugHelper.Log($"[ThemeSelectionUI] Theme selected: {themeId}");
        }

        #endregion

        #region Download Progress

        private void OnDownloadProgress(float progress)
        {
            if (string.IsNullOrEmpty(currentDownloadingThemeId)) return;

            var card = cardInstances.Find(c => c.GetThemeId() == currentDownloadingThemeId);
            if (card != null)
                card.UpdateDownloadProgress(progress);
        }

        private void OnDownloadStatus(string status)
        {
            // Pode ser usado para mostrar status na UI se desejado
            DebugHelper.Log($"[ThemeSelectionUI] Download status: {status}");
        }

        #endregion

        #region Footer

        private void UpdateFooter()
        {
            // Info de armazenamento
            if (storageInfoText != null)
            {
                string storageInfo = ThemeManager.Instance.GetStorageInfo();
                storageInfoText.text = storageInfo;
            }

            // Tema selecionado
            if (selectedThemeText != null)
            {
                if (ThemeManager.Instance.HasSelectedTheme)
                {
                    var theme = ThemeManager.Instance.SelectedTheme;
                    selectedThemeText.text = $"Selecionado: {theme.name}";
                }
                else
                {
                    selectedThemeText.text = "Nenhum tema selecionado";
                }
            }
        }

        #endregion
    }
}
