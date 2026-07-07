using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TimeCrax.Themes;

/// <summary>
/// Ferramenta de Editor para criar o ThemeDownloadPromptUI Canvas
/// Use: Tools > TimeCrax > Create Theme Download Prompt
/// </summary>
public class ThemeDownloadPromptSetup : EditorWindow
{
    [MenuItem("Tools/TimeCrax/Create Theme Download Prompt")]
    public static void CreateThemeDownloadPrompt()
    {
        // Criar Canvas principal
        GameObject canvasGO = new GameObject("ThemeDownloadPromptCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150; // Acima de outros UI

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // CanvasGroup para fade
        CanvasGroup canvasGroup = canvasGO.AddComponent<CanvasGroup>();

        // Background escuro semi-transparente
        GameObject background = CreateUIElement("Background", canvasGO.transform);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // Painel central
        GameObject panel = CreateUIElement("PromptPanel", canvasGO.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500, 350);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Adicionar Outline ao painel
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.8f, 0.6f, 0.2f, 1f);
        panelOutline.effectDistance = new Vector2(2, 2);

        // Título
        GameObject titleGO = CreateUIElement("TitleText", panel.transform);
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(-40, 50);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Tema Necessário";
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.9f, 0.75f, 0.3f, 1f);

        // Mensagem
        GameObject messageGO = CreateUIElement("MessageText", panel.transform);
        RectTransform messageRect = messageGO.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 1);
        messageRect.anchorMax = new Vector2(1, 1);
        messageRect.pivot = new Vector2(0.5f, 1);
        messageRect.anchoredPosition = new Vector2(0, -80);
        messageRect.sizeDelta = new Vector2(-40, 40);
        TextMeshProUGUI messageText = messageGO.AddComponent<TextMeshProUGUI>();
        messageText.text = "Para entrar nesta sala, você precisa baixar o tema:";
        messageText.fontSize = 18;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = Color.white;

        // Nome do tema
        GameObject themeNameGO = CreateUIElement("ThemeNameText", panel.transform);
        RectTransform themeNameRect = themeNameGO.GetComponent<RectTransform>();
        themeNameRect.anchorMin = new Vector2(0, 1);
        themeNameRect.anchorMax = new Vector2(1, 1);
        themeNameRect.pivot = new Vector2(0.5f, 1);
        themeNameRect.anchoredPosition = new Vector2(0, -130);
        themeNameRect.sizeDelta = new Vector2(-40, 50);
        TextMeshProUGUI themeNameText = themeNameGO.AddComponent<TextMeshProUGUI>();
        themeNameText.text = "Nome do Tema";
        themeNameText.fontSize = 26;
        themeNameText.fontStyle = FontStyles.Bold;
        themeNameText.alignment = TextAlignmentOptions.Center;
        themeNameText.color = new Color(0.5f, 0.8f, 1f, 1f);

        // Painel de progresso (inicialmente oculto)
        GameObject progressPanel = CreateUIElement("ProgressPanel", panel.transform);
        RectTransform progressRect = progressPanel.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0, 1);
        progressRect.anchorMax = new Vector2(1, 1);
        progressRect.pivot = new Vector2(0.5f, 1);
        progressRect.anchoredPosition = new Vector2(0, -190);
        progressRect.sizeDelta = new Vector2(-60, 60);

        // Slider de progresso
        GameObject sliderGO = CreateSlider("ProgressSlider", progressPanel.transform);
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(1, 1);
        sliderRect.pivot = new Vector2(0.5f, 1);
        sliderRect.anchoredPosition = new Vector2(0, 0);
        sliderRect.sizeDelta = new Vector2(0, 20);
        Slider progressSlider = sliderGO.GetComponent<Slider>();
        progressSlider.value = 0;
        progressSlider.interactable = false;

        // Texto de progresso
        GameObject progressTextGO = CreateUIElement("ProgressText", progressPanel.transform);
        RectTransform progressTextRect = progressTextGO.GetComponent<RectTransform>();
        progressTextRect.anchorMin = new Vector2(0, 1);
        progressTextRect.anchorMax = new Vector2(1, 1);
        progressTextRect.pivot = new Vector2(0.5f, 1);
        progressTextRect.anchoredPosition = new Vector2(0, -25);
        progressTextRect.sizeDelta = new Vector2(0, 30);
        TextMeshProUGUI progressText = progressTextGO.AddComponent<TextMeshProUGUI>();
        progressText.text = "Baixando...";
        progressText.fontSize = 16;
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.color = Color.white;

        progressPanel.SetActive(false);

        // Container de botões
        GameObject buttonsContainer = CreateUIElement("ButtonsContainer", panel.transform);
        RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0, 0);
        buttonsRect.anchorMax = new Vector2(1, 0);
        buttonsRect.pivot = new Vector2(0.5f, 0);
        buttonsRect.anchoredPosition = new Vector2(0, 20);
        buttonsRect.sizeDelta = new Vector2(-40, 60);

        HorizontalLayoutGroup buttonsLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
        buttonsLayout.spacing = 20;
        buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonsLayout.childControlWidth = true;
        buttonsLayout.childControlHeight = true;
        buttonsLayout.childForceExpandWidth = true;
        buttonsLayout.childForceExpandHeight = true;

        // Botão Download
        GameObject downloadBtnGO = CreateButton("DownloadButton", buttonsContainer.transform, "Baixar Tema");
        Button downloadBtn = downloadBtnGO.GetComponent<Button>();
        Image downloadBtnImage = downloadBtnGO.GetComponent<Image>();
        downloadBtnImage.color = new Color(0.2f, 0.6f, 0.3f, 1f);

        // Botão Cancelar
        GameObject cancelBtnGO = CreateButton("CancelButton", buttonsContainer.transform, "Cancelar");
        Button cancelBtn = cancelBtnGO.GetComponent<Button>();
        Image cancelBtnImage = cancelBtnGO.GetComponent<Image>();
        cancelBtnImage.color = new Color(0.5f, 0.2f, 0.2f, 1f);

        // Adicionar componente ThemeDownloadPromptUI
        ThemeDownloadPromptUI promptUI = canvasGO.AddComponent<ThemeDownloadPromptUI>();

        // Usar SerializedObject para atribuir campos privados
        SerializedObject serializedPrompt = new SerializedObject(promptUI);
        serializedPrompt.FindProperty("promptPanel").objectReferenceValue = panel;
        serializedPrompt.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedPrompt.FindProperty("titleText").objectReferenceValue = titleText;
        serializedPrompt.FindProperty("messageText").objectReferenceValue = messageText;
        serializedPrompt.FindProperty("themeNameText").objectReferenceValue = themeNameText;
        serializedPrompt.FindProperty("progressPanel").objectReferenceValue = progressPanel;
        serializedPrompt.FindProperty("progressSlider").objectReferenceValue = progressSlider;
        serializedPrompt.FindProperty("progressText").objectReferenceValue = progressText;
        serializedPrompt.FindProperty("downloadButton").objectReferenceValue = downloadBtn;
        serializedPrompt.FindProperty("cancelButton").objectReferenceValue = cancelBtn;
        serializedPrompt.ApplyModifiedProperties();

        // Desativar por padrão
        canvasGO.SetActive(false);

        // Selecionar o objeto criado
        Selection.activeGameObject = canvasGO;

        // Marcar cena como modificada
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[ThemeDownloadPromptSetup] ThemeDownloadPromptCanvas criado com sucesso!");
        Debug.Log("[ThemeDownloadPromptSetup] Lembre-se de atribuir a referência de SoundEffects no Inspector.");

        EditorUtility.DisplayDialog("Sucesso",
            "ThemeDownloadPromptCanvas criado!\n\n" +
            "Lembre-se de:\n" +
            "1. Atribuir SoundEffects no Inspector\n" +
            "2. Salvar a cena (Ctrl+S)",
            "OK");
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 50);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.35f, 1f);

        Button button = buttonGO.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.45f, 1f);
        colors.pressedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        button.colors = colors;

        // Texto do botão
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 20;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        return buttonGO;
    }

    private static GameObject CreateSlider(string name, Transform parent)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;

        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(0, 0);
        fillAreaRect.offsetMax = new Vector2(0, 0);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.7f, 0.4f, 1f);

        slider.fillRect = fillRect;

        return sliderGO;
    }
}
