using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    public class ThemeCardUI : MonoBehaviour
    {
        [Header("Cover")]
        [SerializeField] private RawImage coverImage;
        [SerializeField] private Texture2D placeholderTexture;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI creatorText;
        [SerializeField] private TextMeshProUGUI cardCountText;

        [Header("Buttons")]
        [SerializeField] private Button cardButton;
        [SerializeField] private Button downloadButton;
        [SerializeField] private TextMeshProUGUI downloadButtonText;

        [Header("Status")]
        [SerializeField] private Slider downloadProgress;
        [SerializeField] private TextMeshProUGUI readyToPlayText;

        private ThemeListItem themeData;
        private bool isDownloaded;
        private bool isDownloading;
        private string themeId;

        public event Action<string> OnDownloadRequested;
        public event Action<string> OnThemeSelected;

        private void Awake()
        {
            if (cardButton != null)
                cardButton.onClick.AddListener(OnCardClicked);

            if (downloadButton != null)
                downloadButton.onClick.AddListener(OnDownloadClicked);
        }

        private void OnDestroy()
        {
            if (cardButton != null)
                cardButton.onClick.RemoveListener(OnCardClicked);

            if (downloadButton != null)
                downloadButton.onClick.RemoveListener(OnDownloadClicked);
        }

        public void Setup(ThemeListItem theme, bool downloaded, bool selected)
        {
            themeData = theme;
            themeId = theme.id;
            isDownloaded = downloaded;
            isDownloading = false;

            // Preencher informações
            if (nameText != null)
                nameText.text = theme.name ?? "Sem nome";

            if (creatorText != null)
            {
                string firstName = theme.creatorName?.Split(' ')[0] ?? "Autor desconhecido";
                creatorText.text = firstName;
            }

            if (cardCountText != null)
                cardCountText.text = theme.numberOfCards > 0 ? theme.numberOfCards.ToString() : "";

            if (readyToPlayText != null)
                readyToPlayText.text = isDownloaded ? "Ready" : "";

            // Atualizar estado visual
            UpdateVisualState(selected);

            // Carregar imagem de capa
            LoadCoverImage(theme);
        }

        private void LoadCoverImage(ThemeListItem theme)
        {
            // Se já está baixado, carregar imagem local
            if (isDownloaded)
            {
                var localTheme = ThemeStorage.GetTheme(theme.id);
                if (localTheme != null && !string.IsNullOrEmpty(localTheme.localCoverPath))
                {
                    var texture = ThemeStorage.LoadLocalImage(localTheme.localCoverPath);
                    if (texture != null && coverImage != null)
                    {
                        coverImage.texture = texture;
                        return;
                    }
                }
            }

            // Caso contrário, carregar da URL (ou usar placeholder)
            if (!string.IsNullOrEmpty(theme.image))
            {
                StartCoroutine(LoadImageFromUrl(theme.image));
            }
            else if (placeholderTexture != null && coverImage != null)
            {
                coverImage.texture = placeholderTexture;
            }
        }

        private System.Collections.IEnumerator LoadImageFromUrl(string url)
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
                    if (placeholderTexture != null && coverImage != null)
                        coverImage.texture = placeholderTexture;
                }
            }
        }

        private void UpdateVisualState(bool selected)
        {
            // Botão de download
            if (downloadButton != null)
            {
                downloadButton.gameObject.SetActive(!isDownloaded && !isDownloading);
                if (downloadButtonText != null)
                    downloadButtonText.text = "Baixar";
            }

            // Barra de progresso
            if (downloadProgress != null)
            {
                downloadProgress.gameObject.SetActive(isDownloading);
                downloadProgress.value = 0;
            }

            // Texto Ready to Play
            if (readyToPlayText != null)
                readyToPlayText.text = isDownloaded ? "Ready" : "";
        }

        public void SetSelected(bool selected)
        {
            // No visual change for selection
        }

        public void StartDownload()
        {
            isDownloading = true;

            if (downloadButton != null)
                downloadButton.gameObject.SetActive(false);

            if (downloadProgress != null)
            {
                downloadProgress.gameObject.SetActive(true);
                downloadProgress.value = 0;
            }
        }

        public void UpdateDownloadProgress(float progress)
        {
            if (downloadProgress != null)
                downloadProgress.value = progress;
        }

        public void FinishDownload(bool success)
        {
            isDownloading = false;
            isDownloaded = success;

            if (downloadProgress != null)
                downloadProgress.gameObject.SetActive(false);

            if (success)
            {
                // Atualizar texto Ready
                if (readyToPlayText != null)
                    readyToPlayText.text = "Ready";

                // Recarregar imagem local
                if (themeData != null)
                    LoadCoverImage(themeData);
            }
            else
            {
                // Falhou, mostrar botão de download novamente
                if (downloadButton != null)
                    downloadButton.gameObject.SetActive(true);
            }
        }

        private void OnCardClicked()
        {
            if (!isDownloading)
            {
                OnThemeSelected?.Invoke(themeId);
            }
        }

        private void OnDownloadClicked()
        {
            if (themeData == null)
            {
                return;
            }

            if (!isDownloaded && !isDownloading && themeData.readyToPlay)
            {
                OnDownloadRequested?.Invoke(themeId);
            }
        }

        public string GetThemeId() => themeId;
        public bool IsDownloaded() => isDownloaded;
        public bool IsDownloading() => isDownloading;
    }
}
