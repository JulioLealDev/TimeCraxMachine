using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    /// <summary>
    /// UI para exibir prompt de download quando o jogador tenta entrar
    /// em uma sala com um tema que ele não possui baixado.
    /// </summary>
    public class ThemeDownloadPromptUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI themeNameText;

        [Header("Progress")]
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Buttons")]
        [SerializeField] private Button downloadButton;
        [SerializeField] private Button cancelButton;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        public SoundEffects soundEffects;

        // Estado
        private string currentThemeId;
        private string currentThemeName;
        private string targetRoomName;
        private bool isDownloading = false;

        private void Awake()
        {
            SetupButtons();

            if (promptPanel != null)
                promptPanel.SetActive(false);
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
            if (downloadButton != null)
                downloadButton.onClick.AddListener(OnDownloadClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
        }

        #region Public Methods

        /// <summary>
        /// Exibe o prompt de download para um tema específico
        /// </summary>
        public void Show(string themeId, string themeName, string roomName)
        {

            currentThemeId = themeId;
            currentThemeName = themeName;
            targetRoomName = roomName;

            if (promptPanel == null)
            {
                return;
            }

            promptPanel.SetActive(true);
            gameObject.SetActive(true);

            // Verificar Canvas
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
            }
            else
            {
            }

            // Configurar textos
            if (titleText != null)
                titleText.text = "Tema Necessário";

            if (messageText != null)
                messageText.text = "Para entrar nesta sala, você precisa baixar o tema:";

            if (themeNameText != null)
                themeNameText.text = themeName;

            // Esconder progresso inicialmente
            if (progressPanel != null)
                progressPanel.SetActive(false);

            // Habilitar botões
            if (downloadButton != null)
                downloadButton.interactable = true;

            if (cancelButton != null)
                cancelButton.interactable = true;

            isDownloading = false;

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
            else
            {
            }

        }

        public void Hide()
        {
            if (promptPanel == null) return;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;

                LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                    .setOnComplete(() =>
                    {
                        promptPanel.SetActive(false);
                        gameObject.SetActive(false);
                    });
            }
            else
            {
                promptPanel.SetActive(false);
                gameObject.SetActive(false);
            }

            // Reativar botões do lobby
            var lobbyOptions = FindFirstObjectByType<LobbyOptions>(FindObjectsInactive.Include);
            if (lobbyOptions != null)
                lobbyOptions.ActivateButtons(true);

        }

        #endregion

        #region Button Handlers

        private void OnDownloadClicked()
        {
            if (isDownloading) return;

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            isDownloading = true;

            // Desabilitar botões durante download
            if (downloadButton != null)
                downloadButton.interactable = false;

            if (cancelButton != null)
                cancelButton.interactable = false;

            // Mostrar progresso
            if (progressPanel != null)
                progressPanel.SetActive(true);

            if (progressSlider != null)
                progressSlider.value = 0;

            if (progressText != null)
                progressText.text = "Iniciando download...";

            // Iniciar download
            ThemeManager.Instance?.DownloadTheme(currentThemeId, OnDownloadComplete);

        }

        private void OnCancelClicked()
        {
            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            Hide();
        }

        #endregion

        #region Download Callbacks

        private void OnDownloadProgress(float progress)
        {
            if (!isDownloading) return;

            if (progressSlider != null)
                progressSlider.value = progress;

            if (progressText != null)
                progressText.text = $"Baixando... {(progress * 100):F0}%";
        }

        private void OnDownloadStatus(string status)
        {
            if (!isDownloading) return;

            if (progressText != null)
                progressText.text = status;
        }

        private void OnDownloadComplete(bool success, string error)
        {
            isDownloading = false;

            if (success)
            {

                if (progressText != null)
                    progressText.text = "Download concluído!";

                // Aguardar um momento e entrar na sala
                this.DelayedCall(0.5f, JoinRoomAfterDownload);
            }
            else
            {

                if (progressText != null)
                    progressText.text = $"Erro: {error}";

                // Reabilitar botões
                if (downloadButton != null)
                    downloadButton.interactable = true;

                if (cancelButton != null)
                    cancelButton.interactable = true;

                if (progressPanel != null)
                    progressPanel.SetActive(false);
            }
        }

        private void JoinRoomAfterDownload()
        {
            Hide();

            // Selecionar o tema baixado
            ThemeManager.Instance?.SelectTheme(currentThemeId);

            // Entrar na sala
            var gameConnection = FindFirstObjectByType<GameConnection>();
            if (gameConnection != null)
            {
                gameConnection.JoinRoomInList(targetRoomName);
            }
        }

        #endregion
    }
}
