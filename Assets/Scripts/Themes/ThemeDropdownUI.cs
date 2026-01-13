using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    /// <summary>
    /// Gerencia o dropdown de seleção de temas na tela de criação de sala.
    /// </summary>
    public class ThemeDropdownUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button dropdownArrowButton;
        [SerializeField] private TextMeshProUGUI selectedThemeText;
        [SerializeField] private GameObject dropdownPanel;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject themeItemPrefab;

        [Header("Legacy Themes")]
        [SerializeField] private bool includeLegacyThemes = true;
        [SerializeField] private string[] legacyThemeNames = { "World History", "World War 2" };

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.15f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        [SerializeField] private Vector2 panelOffset = new Vector2(0, -10);
        [SerializeField] private int dropdownSortingOrder = 100;

        public SoundEffects soundEffects;

        // Estado
        private bool isOpen = false;
        private List<GameObject> themeItems = new List<GameObject>();
        private CanvasGroup panelCanvasGroup;
        private Canvas dropdownCanvas;

        public event Action<string, string> OnThemeSelected; // (themeId, themeName)

        private void Update()
        {
            // Fechar dropdown ao clicar fora
            if (isOpen && Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverDropdown())
                {
                    CloseDropdown();
                }
            }
        }

        private bool IsPointerOverDropdown()
        {
            if (dropdownPanel == null || dropdownArrowButton == null) return false;

            RectTransform panelRect = dropdownPanel.GetComponent<RectTransform>();
            RectTransform arrowRect = dropdownArrowButton.GetComponent<RectTransform>();

            Vector2 mousePos = Input.mousePosition;

            // Verificar se está sobre o painel
            if (panelRect != null && RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos))
                return true;

            // Verificar se está sobre o botão arrow
            if (arrowRect != null && RectTransformUtility.RectangleContainsScreenPoint(arrowRect, mousePos))
                return true;

            return false;
        }

        private void Awake()
        {
            if (dropdownArrowButton != null)
                dropdownArrowButton.onClick.AddListener(ToggleDropdown);

            if (dropdownPanel != null)
            {
                panelCanvasGroup = dropdownPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                    panelCanvasGroup = dropdownPanel.AddComponent<CanvasGroup>();

                // Adicionar Canvas para garantir que o dropdown renderize por cima
                dropdownCanvas = dropdownPanel.GetComponent<Canvas>();
                if (dropdownCanvas == null)
                    dropdownCanvas = dropdownPanel.AddComponent<Canvas>();

                dropdownCanvas.overrideSorting = true;
                dropdownCanvas.sortingOrder = dropdownSortingOrder;

                // Adicionar GraphicRaycaster para interação
                if (dropdownPanel.GetComponent<GraphicRaycaster>() == null)
                    dropdownPanel.AddComponent<GraphicRaycaster>();

                dropdownPanel.SetActive(false);
            }
        }

        private void Start()
        {
            // Se já houver tema selecionado, mostrar no texto
            if (ThemeManager.Instance != null && ThemeManager.Instance.HasSelectedTheme)
            {
                var theme = ThemeManager.Instance.SelectedTheme;
                UpdateSelectedText(theme.name);
            }
        }

        #region Public Methods

        public void ToggleDropdown()
        {
            if (isOpen)
                CloseDropdown();
            else
                OpenDropdown();
        }

        public void OpenDropdown()
        {
            if (dropdownPanel == null) return;

            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            isOpen = true;
            dropdownPanel.SetActive(true);

            // Popular lista de temas
            PopulateThemeList();

            // Forçar rebuild do layout após um frame
            StartCoroutine(ForceLayoutRebuildDelayed());

            // Fade in
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = true;

                LeanTween.alphaCanvas(panelCanvasGroup, 1f, fadeInDuration)
                    .setOnComplete(() =>
                    {
                        panelCanvasGroup.interactable = true;
                    });
            }

            DebugHelper.Log($"[ThemeDropdownUI] Dropdown aberto, {themeItems.Count} itens");
        }

        private System.Collections.IEnumerator ForceLayoutRebuildDelayed()
        {
            yield return null; // Aguardar um frame

            if (contentContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer as RectTransform);
                DebugHelper.Log($"[ThemeDropdownUI] Layout rebuild - Content size: {(contentContainer as RectTransform).sizeDelta}");
            }

            Canvas.ForceUpdateCanvases();
        }

        public void CloseDropdown()
        {
            if (dropdownPanel == null) return;

            isOpen = false;

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.interactable = false;

                LeanTween.alphaCanvas(panelCanvasGroup, 0f, fadeOutDuration)
                    .setOnComplete(() =>
                    {
                        dropdownPanel.SetActive(false);
                        ClearThemeList();
                    });
            }
            else
            {
                dropdownPanel.SetActive(false);
                ClearThemeList();
            }

            DebugHelper.Log("[ThemeDropdownUI] Dropdown fechado");
        }

        #endregion

        #region Theme List Management

        private void PopulateThemeList()
        {
            ClearThemeList();

            if (themeItemPrefab == null || contentContainer == null)
            {
                DebugHelper.Log("[ThemeDropdownUI] Prefab ou container não configurado");
                return;
            }

            // Adicionar temas baixados da API
            if (ThemeManager.Instance != null)
            {
                var downloadedThemes = ThemeManager.Instance.DownloadedThemes;
                foreach (var theme in downloadedThemes)
                {
                    CreateThemeItem(theme.id, theme.name, false);
                }
            }

            // Adicionar temas legados se configurado
            if (includeLegacyThemes)
            {
                foreach (var legacyName in legacyThemeNames)
                {
                    CreateThemeItem("", legacyName, true);
                }
            }

            // Se não houver temas, mostrar mensagem
            if (themeItems.Count == 0)
            {
                CreateNoThemesMessage();
            }

            DebugHelper.Log($"[ThemeDropdownUI] {themeItems.Count} temas na lista");
        }

        private void CreateThemeItem(string themeId, string themeName, bool isLegacy)
        {
            if (themeItemPrefab == null)
            {
                DebugHelper.Log("[ThemeDropdownUI] ERRO: themeItemPrefab é null!");
                return;
            }

            GameObject itemGO = Instantiate(themeItemPrefab, contentContainer);
            itemGO.SetActive(true);
            itemGO.name = $"ThemeItem_{themeName}";

            // Garantir que o item está na layer correta
            itemGO.layer = contentContainer.gameObject.layer;

            // Configurar RectTransform
            RectTransform itemRect = itemGO.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.localScale = Vector3.one;
                DebugHelper.Log($"[ThemeDropdownUI] Item criado: {themeName}, size: {itemRect.sizeDelta}, pos: {itemRect.anchoredPosition}");
            }

            // Configurar texto
            var textComponent = itemGO.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = themeName;
                if (isLegacy)
                    textComponent.fontStyle = FontStyles.Italic;
            }
            else
            {
                DebugHelper.Log($"[ThemeDropdownUI] AVISO: TextMeshProUGUI não encontrado no item {themeName}");
            }

            // Configurar botão
            var button = itemGO.GetComponent<Button>();
            if (button == null)
                button = itemGO.AddComponent<Button>();

            string capturedId = themeId;
            string capturedName = themeName;
            bool capturedIsLegacy = isLegacy;

            button.onClick.AddListener(() => OnThemeItemClicked(capturedId, capturedName, capturedIsLegacy));

            themeItems.Add(itemGO);
        }

        private void CreateNoThemesMessage()
        {
            GameObject itemGO = Instantiate(themeItemPrefab, contentContainer);
            itemGO.SetActive(true);

            var textComponent = itemGO.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "Nenhum tema disponível";
                textComponent.fontStyle = FontStyles.Italic;
                textComponent.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }

            // Desabilitar clique
            var button = itemGO.GetComponent<Button>();
            if (button != null)
                button.interactable = false;

            themeItems.Add(itemGO);
        }

        private void ClearThemeList()
        {
            foreach (var item in themeItems)
            {
                if (item != null)
                    Destroy(item);
            }
            themeItems.Clear();
        }

        #endregion

        #region Selection

        private void OnThemeItemClicked(string themeId, string themeName, bool isLegacy)
        {
            if (soundEffects != null)
                soundEffects.PressHudButtonSound();

            // Atualizar texto do dropdown
            UpdateSelectedText(themeName);

            // Selecionar tema no ThemeManager
            if (!isLegacy && !string.IsNullOrEmpty(themeId))
            {
                ThemeManager.Instance?.SelectTheme(themeId);
                DebugHelper.Log($"[ThemeDropdownUI] Tema API selecionado: {themeName} (ID: {themeId})");
            }
            else
            {
                // Tema legado - limpar seleção do ThemeManager
                ThemeManager.Instance?.SelectTheme((ThemeData)null);
                DebugHelper.Log($"[ThemeDropdownUI] Tema legado selecionado: {themeName}");
            }

            // Disparar evento
            OnThemeSelected?.Invoke(themeId, themeName);

            // Fechar dropdown
            CloseDropdown();
        }

        private void UpdateSelectedText(string themeName)
        {
            if (selectedThemeText != null)
                selectedThemeText.text = themeName;
        }

        #endregion

        #region Public Accessors

        public bool IsOpen => isOpen;

        public string GetSelectedThemeName()
        {
            return selectedThemeText != null ? selectedThemeText.text : "";
        }

        #endregion
    }
}
