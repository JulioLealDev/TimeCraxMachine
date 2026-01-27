using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    public class ThemeInfoUI : MonoBehaviour
    {
        [Header("Screen")]
        [SerializeField] private GameObject themeInfoScreen;
        [SerializeField] private Transform infoContainer;
        [SerializeField] private GameObject themeInfoPrefab;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        [Header("Audio")]
        [SerializeField] private SoundEffects soundEffects;

        private CanvasGroup canvasGroup;
        private GameObject currentInfoInstance;
        private ThemeListItem currentTheme;
        private bool isDownloaded;
        private bool isDownloading;
        private GameObject backgroundCloseButton;

        // Referências do prefab instanciado
        private Button downloadButton;
        private Button playButton;
        private Slider downloadProgress;
        private TextMeshProUGUI readyToPlayText;

        public event Action OnClose;
        public event Action<string> OnDownloadRequested;
        public event Action<string> OnPlayRequested;

        private static ThemeInfoUI _instance;
        private bool isInitialized = false;

        public static ThemeInfoUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Tentar encontrar na cena (incluindo objetos inativos)
                    _instance = FindFirstObjectByType<ThemeInfoUI>(FindObjectsInactive.Include);

                    if (_instance == null)
                    {
                        DebugHelper.Log("[ThemeInfoUI] Instance not found in scene!");
                    }
                    else
                    {
                        // Garantir inicialização
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Initialize();
        }

        /// <summary>
        /// Inicializa o componente. Pode ser chamado múltiplas vezes com segurança.
        /// </summary>
        private void Initialize()
        {
            if (isInitialized) return;

            if (themeInfoScreen != null)
            {
                canvasGroup = themeInfoScreen.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = themeInfoScreen.AddComponent<CanvasGroup>();

                themeInfoScreen.SetActive(false);
            }

            isInitialized = true;
            DebugHelper.Log("[ThemeInfoUI] Initialized");
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            UnsubscribeFromDownloader();
        }

        private void SubscribeToDownloader()
        {
            var downloader = ThemeDownloader.Instance;
            if (downloader != null)
            {
                downloader.OnDownloadProgress += OnDownloadProgress;
                downloader.OnDownloadStatus += OnDownloadStatus;
            }
        }

        private void UnsubscribeFromDownloader()
        {
            var downloader = ThemeDownloader.Instance;
            if (downloader != null)
            {
                downloader.OnDownloadProgress -= OnDownloadProgress;
                downloader.OnDownloadStatus -= OnDownloadStatus;
            }
        }

        private void OnDownloadProgress(float progress)
        {
            if (!isDownloading || downloadProgress == null) return;
            downloadProgress.value = progress;
        }

        private void OnDownloadStatus(string status)
        {
            DebugHelper.Log($"[ThemeInfoUI] Download status: {status}");
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        public void Show(ThemeListItem theme, bool downloaded)
        {
            if (themeInfoScreen == null || themeInfoPrefab == null) return;

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            currentTheme = theme;
            isDownloaded = downloaded;
            isDownloading = false;

            // Limpar referências
            downloadButton = null;
            playButton = null;
            downloadProgress = null;
            readyToPlayText = null;

            // Limpar instância anterior
            if (currentInfoInstance != null)
                Destroy(currentInfoInstance);

            // Ativar tela
            themeInfoScreen.SetActive(true);

            // Instanciar prefab
            currentInfoInstance = Instantiate(themeInfoPrefab, infoContainer);
            currentInfoInstance.SetActive(true);

            // Criar botão de fundo para fechar ao clicar fora (dentro do prefab instanciado)
            CreateBackgroundCloseButton();

            // Preencher dados
            PopulateThemeInfo(theme, downloaded);

            // Inscrever nos eventos de download
            SubscribeToDownloader();

            // Fade in
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

            DebugHelper.Log($"[ThemeInfoUI] Showing info for: {theme.name}");
        }

        public void Close()
        {
            if (themeInfoScreen == null) return;

            // Não fechar se estiver baixando
            if (isDownloading)
            {
                DebugHelper.Log("[ThemeInfoUI] Cannot close while downloading");
                return;
            }

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            UnsubscribeFromDownloader();

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;

                LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                    .setOnComplete(() =>
                    {
                        themeInfoScreen.SetActive(false);
                        if (currentInfoInstance != null)
                        {
                            Destroy(currentInfoInstance);
                            currentInfoInstance = null;
                        }
                        if (backgroundCloseButton != null)
                        {
                            Destroy(backgroundCloseButton);
                            backgroundCloseButton = null;
                        }
                        if (downloadButton != null)
                        {
                            Destroy(downloadButton.gameObject);
                            downloadButton = null;
                        }
                        if (downloadProgress != null)
                        {
                            Destroy(downloadProgress.gameObject);
                            downloadProgress = null;
                        }
                        playButton = null;
                        readyToPlayText = null;
                        OnClose?.Invoke();
                    });
            }
            else
            {
                themeInfoScreen.SetActive(false);
                if (currentInfoInstance != null)
                {
                    Destroy(currentInfoInstance);
                    currentInfoInstance = null;
                }
                if (backgroundCloseButton != null)
                {
                    Destroy(backgroundCloseButton);
                    backgroundCloseButton = null;
                }
                if (downloadButton != null)
                {
                    Destroy(downloadButton.gameObject);
                    downloadButton = null;
                }
                if (downloadProgress != null)
                {
                    Destroy(downloadProgress.gameObject);
                    downloadProgress = null;
                }
                playButton = null;
                readyToPlayText = null;
                OnClose?.Invoke();
            }

            DebugHelper.Log("[ThemeInfoUI] Closed");
        }

        private void PopulateThemeInfo(ThemeListItem theme, bool downloaded)
        {
            if (currentInfoInstance == null) return;

            // Buscar componentes no prefab por nome
            foreach (Transform child in currentInfoInstance.GetComponentsInChildren<Transform>(true))
            {
                switch (child.name)
                {
                    case "ThemeName":
                    case "NameText":
                        var nameText = child.GetComponent<TextMeshProUGUI>();
                        if (nameText != null)
                            nameText.text = theme.name ?? "Sem nome";
                        break;

                    case "ThemeCreator":
                    case "CreatorName":
                    case "CreatorText":
                        var creatorText = child.GetComponent<TextMeshProUGUI>();
                        if (creatorText != null)
                            creatorText.text = theme.creatorName ?? "Autor desconhecido";
                        break;

                    case "ThemeNumberOfCards":
                    case "CardCount":
                    case "CardCountText":
                        var cardCountText = child.GetComponent<TextMeshProUGUI>();
                        if (cardCountText != null)
                            cardCountText.text = theme.numberOfCards > 0 ? theme.numberOfCards.ToString() : "";
                        break;

                    case "ThemeResume":
                    case "Resume":
                    case "ResumeText":
                    case "Description":
                        var resumeText = child.GetComponent<TextMeshProUGUI>();
                        if (resumeText != null)
                            resumeText.text = theme.resume ?? "";
                        break;

                    case "ThemeRecommendation":
                    case "Recommendation":
                    case "RecommendationText":
                        var recommendationText = child.GetComponent<TextMeshProUGUI>();
                        if (recommendationText != null)
                            recommendationText.text = theme.recommendation ?? "";
                        break;

                    case "ThemeCreatedAt":
                    case "CreatedAt":
                    case "DateText":
                        var createdAtText = child.GetComponent<TextMeshProUGUI>();
                        if (createdAtText != null)
                            createdAtText.text = FormatDate(theme.createdAt);
                        break;

                    case "ReadyToPlayText":
                        var readyText = child.GetComponent<TextMeshProUGUI>();
                        if (readyText != null)
                        {
                            readyToPlayText = readyText;
                            readyToPlayText.text = downloaded ? "Ready" : "";
                            DebugHelper.Log($"[ThemeInfoUI] ReadyToPlayText found, downloaded={downloaded}");
                        }
                        break;

                    case "ThemeCover":
                    case "CoverImage":
                    case "ThemeImage":
                        var coverImage = child.GetComponent<RawImage>();
                        if (coverImage != null)
                            LoadCoverImage(theme, coverImage, downloaded);
                        break;

                    case "DownloadProgress":
                    case "ProgressBar":
                        var progress = child.GetComponent<Slider>();
                        if (progress != null)
                        {
                            downloadProgress = progress;
                            downloadProgress.gameObject.SetActive(false);
                            downloadProgress.value = 0;
                            // Mover para frente do backgroundCloseButton
                            downloadProgress.transform.SetParent(themeInfoScreen.transform, true);
                            downloadProgress.transform.SetAsLastSibling();
                        }
                        break;

                    case "DownloadButton":
                        var downloadBtn = child.GetComponent<Button>();
                        if (downloadBtn != null)
                        {
                            downloadButton = downloadBtn;
                            downloadButton.onClick.RemoveAllListeners();
                            downloadButton.onClick.AddListener(OnDownloadClicked);
                            // Mover para frente do backgroundCloseButton
                            downloadButton.transform.SetParent(themeInfoScreen.transform, true);
                            downloadButton.transform.SetAsLastSibling();
                            // Mostrar apenas se não foi baixado e está pronto para jogar
                            downloadButton.gameObject.SetActive(!downloaded && theme.readyToPlay);
                        }
                        break;

                    case "PlayButton":
                    case "SelectButton":
                        var playBtn = child.GetComponent<Button>();
                        if (playBtn != null)
                        {
                            playButton = playBtn;
                            playButton.gameObject.SetActive(downloaded);
                            playButton.onClick.RemoveAllListeners();
                            playButton.onClick.AddListener(OnPlayClicked);
                        }
                        break;

                    case "CloseButton":
                    case "BackButton":
                        var closeBtn = child.GetComponent<Button>();
                        if (closeBtn != null)
                        {
                            closeBtn.onClick.RemoveAllListeners();
                            closeBtn.onClick.AddListener(Close);
                        }
                        break;

                    case "Background":
                    case "BackgroundButton":
                    case "Overlay":
                        var bgBtn = child.GetComponent<Button>();
                        if (bgBtn != null)
                        {
                            bgBtn.onClick.RemoveAllListeners();
                            bgBtn.onClick.AddListener(Close);
                        }
                        break;
                }
            }
        }

        private string FormatDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return "";

            if (DateTime.TryParse(dateString, out DateTime date))
                return date.ToString("dd/MM/yyyy");

            return dateString;
        }

        private void LoadCoverImage(ThemeListItem theme, RawImage coverImage, bool downloaded)
        {
            if (downloaded)
            {
                var localTheme = ThemeStorage.GetTheme(theme.id);
                if (localTheme != null && !string.IsNullOrEmpty(localTheme.localCoverPath))
                {
                    var texture = ThemeStorage.LoadLocalImage(localTheme.localCoverPath);
                    if (texture != null)
                    {
                        coverImage.texture = texture;
                        return;
                    }
                }
            }

            if (!string.IsNullOrEmpty(theme.image))
            {
                StartCoroutine(LoadImageFromUrl(theme.image, coverImage));
            }
        }

        private System.Collections.IEnumerator LoadImageFromUrl(string url, RawImage targetImage)
        {
            using (var www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                    if (targetImage != null)
                        targetImage.texture = texture;
                }
            }
        }

        private void OnDownloadClicked()
        {
            if (currentTheme == null || isDownloaded || isDownloading) return;

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            isDownloading = true;

            // Esconder botão de download e mostrar barra de progresso
            if (downloadButton != null)
                downloadButton.gameObject.SetActive(false);

            if (downloadProgress != null)
            {
                downloadProgress.gameObject.SetActive(true);
                downloadProgress.value = 0;
            }

            DebugHelper.Log($"[ThemeInfoUI] Starting download: {currentTheme.id}");

            // Iniciar download
            ThemeManager.Instance.DownloadTheme(currentTheme.id, (success, error) =>
            {
                isDownloading = false;

                if (downloadProgress != null)
                    downloadProgress.gameObject.SetActive(false);

                if (success)
                {
                    isDownloaded = true;

                    // Mostrar botão de Play
                    if (playButton != null)
                        playButton.gameObject.SetActive(true);

                    // Atualizar texto ReadyToPlay
                    if (readyToPlayText != null)
                        readyToPlayText.text = "Ready";

                    DebugHelper.Log($"[ThemeInfoUI] Download completed: {currentTheme.id}");

                    // Notificar ThemeSelectionUI para atualizar
                    OnDownloadRequested?.Invoke(currentTheme.id);
                }
                else
                {
                    // Falhou - mostrar botão de download novamente
                    if (downloadButton != null)
                        downloadButton.gameObject.SetActive(true);

                    DebugHelper.Log($"[ThemeInfoUI] Download failed: {error}");
                }
            });
        }

        private void OnPlayClicked()
        {
            if (currentTheme != null && isDownloaded)
            {
                if (soundEffects != null)
                    soundEffects.PressHudButtonSound();

                DebugHelper.Log($"[ThemeInfoUI] Play requested: {currentTheme.id}");
                OnPlayRequested?.Invoke(currentTheme.id);
                Close();
            }
        }

        public bool IsOpen => themeInfoScreen != null && themeInfoScreen.activeSelf;

        private void CreateBackgroundCloseButton()
        {
            DebugHelper.Log("[ThemeInfoUI] CreateBackgroundCloseButton called");

            // Remover botão anterior se existir
            if (backgroundCloseButton != null)
            {
                Destroy(backgroundCloseButton);
                backgroundCloseButton = null;
            }

            if (themeInfoScreen == null)
            {
                DebugHelper.Log("[ThemeInfoUI] themeInfoScreen is null!");
                return;
            }

            // Criar GameObject para o botão de fundo dentro do themeInfoScreen (cobre toda a tela)
            backgroundCloseButton = new GameObject("BackgroundCloseButton");
            backgroundCloseButton.transform.SetParent(themeInfoScreen.transform, false);
            backgroundCloseButton.transform.SetAsLastSibling(); // Ficar na frente de tudo

            // Adicionar RectTransform que cobre toda a tela
            var rectTransform = backgroundCloseButton.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Adicionar Image transparente para receber raycasts
            var image = backgroundCloseButton.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0); // Totalmente transparente
            image.raycastTarget = true;

            // Adicionar Button e configurar onClick
            var button = backgroundCloseButton.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(Close);

            DebugHelper.Log("[ThemeInfoUI] BackgroundCloseButton created successfully");
        }
    }
}
