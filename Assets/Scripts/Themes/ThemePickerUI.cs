using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    /// <summary>
    /// Painel simples para selecionar temas disponíveis (baixados + legados).
    /// Não inclui funcionalidade de download - apenas seleção.
    /// </summary>
    public class ThemePickerUI : MonoBehaviour
    {
        [Header("Canvas References")]
        [SerializeField] private GameObject pickerCanvas;
        [SerializeField] private Transform gridContainer;
        [SerializeField] private GameObject themePickerCardPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image backgroundOverlay;

        [Header("Legacy Themes")]
        [SerializeField] private bool includeLegacyThemes = true;
        [SerializeField] private string[] legacyThemeNames = { "World History", "World War 2" };
        [SerializeField] private Sprite defaultLegacySprite;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        [Header("Audio")]
        public SoundEffects soundEffects;

        // Eventos
        public event Action<string, string> OnThemeSelected; // (themeId, themeName)
        public event Action OnPanelClosed;

        // Estado
        private bool isOpen = false;
        private List<GameObject> themeCards = new List<GameObject>();
        private CanvasGroup canvasGroup;

        // Singleton
        private static ThemePickerUI _instance;
        public static ThemePickerUI Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (pickerCanvas != null)
            {
                canvasGroup = pickerCanvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = pickerCanvas.AddComponent<CanvasGroup>();

                pickerCanvas.SetActive(false);
            }

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            // Fechar ao clicar no background
            if (backgroundOverlay != null)
            {
                var bgButton = backgroundOverlay.GetComponent<Button>();
                if (bgButton == null)
                    bgButton = backgroundOverlay.gameObject.AddComponent<Button>();
                bgButton.onClick.AddListener(Close);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #region Public Methods

        /// <summary>
        /// Abre o painel de seleção de temas
        /// </summary>
        public void Show()
        {
            if (pickerCanvas == null) return;

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            isOpen = true;
            pickerCanvas.SetActive(true);

            // Popular cards
            PopulateThemeCards();

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

            DebugHelper.Log($"[ThemePickerUI] Painel aberto com {themeCards.Count} cards");
        }

        /// <summary>
        /// Fecha o painel de seleção de temas
        /// </summary>
        public void Close()
        {
            if (pickerCanvas == null || !isOpen) return;

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            isOpen = false;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;

                LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                    .setOnComplete(() =>
                    {
                        pickerCanvas.SetActive(false);
                        ClearThemeCards();
                        OnPanelClosed?.Invoke();
                    });
            }
            else
            {
                pickerCanvas.SetActive(false);
                ClearThemeCards();
                OnPanelClosed?.Invoke();
            }

            DebugHelper.Log("[ThemePickerUI] Painel fechado");
        }

        public bool IsOpen => isOpen;

        #endregion

        #region Theme Cards Management

        private void PopulateThemeCards()
        {
            ClearThemeCards();

            if (themePickerCardPrefab == null || gridContainer == null)
            {
                DebugHelper.Log("[ThemePickerUI] Prefab ou container não configurado");
                return;
            }

            // Adicionar temas baixados da API
            if (ThemeManager.Instance != null)
            {
                var downloadedThemes = ThemeManager.Instance.DownloadedThemes;
                foreach (var theme in downloadedThemes)
                {
                    CreateThemeCard(theme.id, theme.name, theme.creatorName, false, theme);
                }
            }

            // Adicionar temas legados
            if (includeLegacyThemes)
            {
                foreach (var legacyName in legacyThemeNames)
                {
                    CreateThemeCard("", legacyName, "TimeCrax", true, null);
                }
            }

            // Se não houver temas, mostrar mensagem
            if (themeCards.Count == 0)
            {
                CreateEmptyMessage();
            }

            DebugHelper.Log($"[ThemePickerUI] {themeCards.Count} cards criados");
        }

        private void CreateThemeCard(string themeId, string themeName, string creatorName, bool isLegacy, ThemeData themeData)
        {
            GameObject cardGO = Instantiate(themePickerCardPrefab, gridContainer);
            cardGO.SetActive(true);
            cardGO.name = $"PickerCard_{themeName}";

            // Procurar componentes no card
            RawImage coverImage = null;
            TextMeshProUGUI nameText = null;
            TextMeshProUGUI creatorText = null;
            Button cardButton = null;
            Image cardBackground = null;

            foreach (Transform child in cardGO.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "CoverImage" || child.name == "ThemeImage")
                    coverImage = child.GetComponent<RawImage>();
                else if (child.name == "ThemeName" || child.name == "NameText")
                    nameText = child.GetComponent<TextMeshProUGUI>();
                else if (child.name == "CreatorName" || child.name == "CreatorText")
                    creatorText = child.GetComponent<TextMeshProUGUI>();
                else if (child.name == "CardBackground" || child.name == "Background")
                    cardBackground = child.GetComponent<Image>();
            }

            // Fallback para componentes diretos
            if (nameText == null) nameText = cardGO.GetComponentInChildren<TextMeshProUGUI>();
            if (coverImage == null) coverImage = cardGO.GetComponentInChildren<RawImage>();

            // Configurar textos
            if (nameText != null)
            {
                nameText.text = themeName;
                if (isLegacy)
                    nameText.fontStyle = FontStyles.Italic;
            }

            if (creatorText != null)
                creatorText.text = creatorName;

            // Configurar imagem
            if (coverImage != null)
            {
                if (!isLegacy && themeData != null)
                {
                    // Carregar imagem do tema baixado
                    Texture2D coverTexture = ThemeManager.Instance?.LoadThemeCover(themeData);
                    if (coverTexture != null)
                        coverImage.texture = coverTexture;
                }
                else if (isLegacy && defaultLegacySprite != null)
                {
                    // Usar sprite padrão para temas legados
                    coverImage.texture = defaultLegacySprite.texture;
                }
            }

            // Configurar botão
            cardButton = cardGO.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = cardGO.AddComponent<Button>();
                DebugHelper.Log($"[ThemePickerUI] Button adicionado ao card: {themeName}");
            }

            // Garantir que o botão está interativo
            cardButton.interactable = true;

            string capturedId = themeId;
            string capturedName = themeName;
            bool capturedIsLegacy = isLegacy;

            cardButton.onClick.RemoveAllListeners(); // Limpar listeners anteriores
            cardButton.onClick.AddListener(() => OnCardClicked(capturedId, capturedName, capturedIsLegacy));

            DebugHelper.Log($"[ThemePickerUI] Listener configurado para: {themeName}");

            themeCards.Add(cardGO);
        }

        private void CreateEmptyMessage()
        {
            GameObject cardGO = Instantiate(themePickerCardPrefab, gridContainer);
            cardGO.SetActive(true);
            cardGO.name = "PickerCard_Empty";

            var nameText = cardGO.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = "Nenhum tema disponível";
                nameText.fontStyle = FontStyles.Italic;
                nameText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }

            var button = cardGO.GetComponent<Button>();
            if (button != null)
                button.interactable = false;

            themeCards.Add(cardGO);
        }

        private void ClearThemeCards()
        {
            foreach (var card in themeCards)
            {
                if (card != null)
                    Destroy(card);
            }
            themeCards.Clear();
        }

        #endregion

        #region Selection

        private void OnCardClicked(string themeId, string themeName, bool isLegacy)
        {
            DebugHelper.Log($"[ThemePickerUI] >>> CLIQUE DETECTADO: {themeName}");

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            DebugHelper.Log($"[ThemePickerUI] Tema selecionado: {themeName} (ID: {themeId}, Legacy: {isLegacy})");

            // Selecionar tema no ThemeManager
            if (!isLegacy && !string.IsNullOrEmpty(themeId))
            {
                ThemeManager.Instance?.SelectTheme(themeId);
            }
            else
            {
                // Tema legado - limpar seleção do ThemeManager
                ThemeManager.Instance?.SelectTheme((ThemeData)null);
            }

            // Disparar evento
            OnThemeSelected?.Invoke(themeId, themeName);

            // Fechar painel
            Close();
        }

        #endregion
    }
}
