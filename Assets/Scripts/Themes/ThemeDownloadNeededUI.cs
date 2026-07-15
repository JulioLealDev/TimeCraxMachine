using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    /// <summary>
    /// UI exibida quando o jogador tenta entrar em uma sala com um tema que não possui.
    /// Permite baixar o tema e entra automaticamente na sala após o download.
    /// </summary>
    public class ThemeDownloadNeededUI : MonoBehaviour
    {
        [Header("Screen")]
        [SerializeField] private GameObject themeNeededScreen;
        [SerializeField] private GameObject themeNeededContent;

        [Header("Theme Info")]
        [SerializeField] private RawImage coverImage;
        [SerializeField] private TextMeshProUGUI themeNameText;
        [SerializeField] private TextMeshProUGUI creatorNameText;
        [SerializeField] private TextMeshProUGUI cardCountText;

        [Header("Buttons")]
        [SerializeField] private Button downloadButton;
        [SerializeField] private Button cancelButton;

        [Header("Progress")]
        [SerializeField] private GameObject downloadProgress;
        [SerializeField] private Slider progressSlider;

        [Header("Background")]
        [SerializeField] private GameObject background;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        [Header("Audio")]
        [SerializeField] private SoundEffects soundEffects;

        // Estado
        private string currentThemeId;
        private string currentThemeName;
        private string targetRoomName;
        private bool isRoomLocked;
        private bool isDownloading = false;
        private CanvasGroup canvasGroup;

        private static ThemeDownloadNeededUI _instance;
        public static ThemeDownloadNeededUI Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (themeNeededScreen != null)
            {
                canvasGroup = themeNeededScreen.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = themeNeededScreen.AddComponent<CanvasGroup>();

                themeNeededScreen.SetActive(false);
            }

            SetupButtons();
        }

        private void Start()
        {
            SubscribeToDownloader();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            UnsubscribeFromDownloader();
        }

        private void Update()
        {
            // ESC fecha a tela (apenas se não estiver baixando)
            if (IsOpen && !isDownloading && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void SetupButtons()
        {
            if (downloadButton != null)
                downloadButton.onClick.AddListener(OnDownloadClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
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

        #region Public Methods

        /// <summary>
        /// Exibe a tela de download necessário para um tema específico
        /// </summary>
        /// <param name="themeId">ID do tema</param>
        /// <param name="themeName">Nome do tema</param>
        /// <param name="roomName">Nome da sala para entrar após download</param>
        /// <param name="isLocked">Se a sala requer senha</param>
        /// <param name="creatorName">Nome do criador (opcional)</param>
        /// <param name="cardCount">Número de cartas (opcional)</param>
        /// <param name="coverUrl">URL da imagem de capa (opcional)</param>
        public void Show(string themeId, string themeName, string roomName, bool isLocked,
                        string creatorName = null, int cardCount = 0, string coverUrl = null)
        {

            currentThemeId = themeId;
            currentThemeName = themeName;
            targetRoomName = roomName;
            isRoomLocked = isLocked;
            isDownloading = false;

            if (themeNeededScreen == null)
            {
                return;
            }

            // Ativar tela e background
            themeNeededScreen.SetActive(true);
            gameObject.SetActive(true);

            if (background != null)
                background.SetActive(true);

            // Preencher informações básicas do tema
            if (themeNameText != null)
                themeNameText.text = themeName ?? "Tema Desconhecido";

            if (creatorNameText != null)
                creatorNameText.text = creatorName ?? "";

            if (cardCountText != null)
                cardCountText.text = cardCount > 0 ? $"{cardCount} cards" : "";

            // Carregar imagem de capa se tiver URL
            if (coverImage != null && !string.IsNullOrEmpty(coverUrl))
            {
                StartCoroutine(LoadCoverImage(coverUrl));
            }

            // Se não tiver dados completos, buscar da API
            bool needsExtraInfo = string.IsNullOrEmpty(creatorName) || cardCount == 0 || string.IsNullOrEmpty(coverUrl);
            if (needsExtraInfo && !string.IsNullOrEmpty(themeId))
            {
                FetchThemeInfoFromApi(themeId);
            }

            // Resetar estado dos botões
            if (downloadButton != null)
                downloadButton.gameObject.SetActive(true);

            if (cancelButton != null)
                cancelButton.interactable = true;

            if (downloadProgress != null)
                downloadProgress.SetActive(false);

            if (progressSlider != null)
                progressSlider.value = 0;

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
        }

        /// <summary>
        /// Busca informações extras do tema da API para preencher a UI
        /// </summary>
        private void FetchThemeInfoFromApi(string themeId)
        {
            var downloader = ThemeDownloader.Instance;
            if (downloader == null) return;


            downloader.GetThemeInfo(themeId, (themeInfo) =>
            {
                if (themeInfo == null)
                {
                    return;
                }


                // Atualizar UI com informações da API
                if (themeNameText != null && !string.IsNullOrEmpty(themeInfo.name))
                    themeNameText.text = themeInfo.name;

                if (creatorNameText != null && !string.IsNullOrEmpty(themeInfo.creatorName))
                    creatorNameText.text = themeInfo.creatorName;

                if (cardCountText != null && themeInfo.numberOfCards > 0)
                    cardCountText.text = $"{themeInfo.numberOfCards} cards";

                // Carregar imagem de capa da API
                if (coverImage != null && !string.IsNullOrEmpty(themeInfo.image))
                    StartCoroutine(LoadCoverImage(themeInfo.image));
            });
        }

        public void Close()
        {
            if (themeNeededScreen == null) return;

            // Não fechar se estiver baixando
            if (isDownloading)
            {
                return;
            }

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;

                LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                    .setOnComplete(() =>
                    {
                        themeNeededScreen.SetActive(false);
                        gameObject.SetActive(false);
                        if (background != null)
                            background.SetActive(false);
                        ReactivateLobbyButtons();
                    });
            }
            else
            {
                themeNeededScreen.SetActive(false);
                gameObject.SetActive(false);
                if (background != null)
                    background.SetActive(false);
                ReactivateLobbyButtons();
            }

        }

        public bool IsOpen => themeNeededScreen != null && themeNeededScreen.activeSelf;

        #endregion

        #region Button Handlers

        private void OnDownloadClicked()
        {
            if (isDownloading) return;

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            isDownloading = true;

            // Desabilitar botão de cancelar durante download (apenas desativa, não esconde)
            if (cancelButton != null)
                cancelButton.interactable = false;

            // Esconder botão de download
            if (downloadButton != null)
                downloadButton.gameObject.SetActive(false);

            // Mostrar progresso
            if (downloadProgress != null)
                downloadProgress.SetActive(true);

            if (progressSlider != null)
                progressSlider.value = 0;

            // Iniciar download
            ThemeManager.Instance?.DownloadTheme(currentThemeId, OnDownloadComplete);
        }

        private void OnCancelClicked()
        {
            Close();
        }

        #endregion

        #region Download Callbacks

        private void OnDownloadProgress(float progress)
        {
            if (!isDownloading) return;

            if (progressSlider != null)
                progressSlider.value = progress;
        }

        private void OnDownloadStatus(string status)
        {
            if (!isDownloading) return;
        }

        private void OnDownloadComplete(bool success, string error)
        {
            isDownloading = false;

            if (success)
            {

                if (progressSlider != null)
                    progressSlider.value = 1f;

                // Entrar na sala imediatamente após download
                this.DelayedCall(0.3f, JoinRoomAfterDownload);
            }
            else
            {

                // Reativar botões em caso de erro
                if (downloadButton != null)
                    downloadButton.gameObject.SetActive(true);

                if (cancelButton != null)
                    cancelButton.interactable = true;

                if (downloadProgress != null)
                    downloadProgress.SetActive(false);
            }
        }

        private void JoinRoomAfterDownload()
        {
            // Selecionar o tema baixado
            ThemeManager.Instance?.SelectTheme(currentThemeId);

            // Fechar tela (sem chamar Close para evitar reativação dos botões do lobby)
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                    .setOnComplete(() =>
                    {
                        themeNeededScreen.SetActive(false);
                        gameObject.SetActive(false);
                        if (background != null)
                            background.SetActive(false);

                        // Após fechar a tela, verificar se precisa de senha
                        ProcessRoomEntry();
                    });
            }
            else
            {
                themeNeededScreen.SetActive(false);
                gameObject.SetActive(false);
                if (background != null)
                    background.SetActive(false);

                // Verificar se precisa de senha
                ProcessRoomEntry();
            }
        }

        private void ProcessRoomEntry()
        {
            if (isRoomLocked)
            {
                // Sala requer senha - mostrar tela de senha

                var passwordScreen = FindFirstObjectByType<PasswordScreen>(FindObjectsInactive.Include);
                var lobbyOptions = FindFirstObjectByType<LobbyOptions>(FindObjectsInactive.Include);

                if (passwordScreen != null)
                {
                    passwordScreen.gameObject.SetActive(true);
                    passwordScreen.ActivateBackground(true);
                    passwordScreen.SetRoomName(targetRoomName);

                    if (lobbyOptions != null)
                        lobbyOptions.ActivateButtons(false);
                }
                else
                {
                }
            }
            else
            {
                // Sala não requer senha - entrar diretamente
                var gameConnection = FindFirstObjectByType<GameConnection>();
                if (gameConnection != null)
                {
                    gameConnection.JoinRoomInList(targetRoomName);
                }
            }
        }

        #endregion

        #region Helper Methods

        private void ReactivateLobbyButtons()
        {
            var lobbyOptions = FindFirstObjectByType<LobbyOptions>(FindObjectsInactive.Include);
            if (lobbyOptions != null)
                lobbyOptions.ActivateButtons(true);
        }

        private System.Collections.IEnumerator LoadCoverImage(string url)
        {
            using (var www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                    if (coverImage != null)
                        coverImage.texture = texture;
                }
                else
                {
                }
            }
        }

        #endregion
    }
}
