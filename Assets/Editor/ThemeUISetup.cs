using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TimeCrax.Themes;

public class ThemeUISetup : Editor
{
    [MenuItem("Tools/TimeCrax/Create Theme Card Prefab")]
    public static void CreateThemeCardPrefab()
    {
        // Criar root do card
        GameObject cardRoot = new GameObject("ThemeCard");
        RectTransform cardRect = cardRoot.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(280, 220);

        // Adicionar Image de fundo
        Image cardBg = cardRoot.AddComponent<Image>();
        cardBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Adicionar Button
        Button cardButton = cardRoot.AddComponent<Button>();
        cardButton.transition = Selectable.Transition.ColorTint;

        // Adicionar ThemeCardUI
        ThemeCardUI cardUI = cardRoot.AddComponent<ThemeCardUI>();

        // Cover Image (RawImage para texturas dinamicas)
        GameObject coverGO = new GameObject("CoverImage");
        coverGO.transform.SetParent(cardRoot.transform, false);
        RectTransform coverRect = coverGO.AddComponent<RectTransform>();
        coverRect.anchorMin = new Vector2(0, 0.35f);
        coverRect.anchorMax = new Vector2(1, 1);
        coverRect.offsetMin = new Vector2(5, 0);
        coverRect.offsetMax = new Vector2(-5, -5);
        RawImage coverImage = coverGO.AddComponent<RawImage>();
        coverImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Info Panel
        GameObject infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(cardRoot.transform, false);
        RectTransform infoRect = infoPanel.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(1, 0.35f);
        infoRect.offsetMin = new Vector2(8, 5);
        infoRect.offsetMax = new Vector2(-8, 0);

        // Theme Name
        GameObject nameGO = CreateTMPText("ThemeName", infoPanel.transform, "Nome do Tema", 14, FontStyles.Bold);
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // Creator Name
        GameObject creatorGO = CreateTMPText("CreatorName", infoPanel.transform, "por Autor", 11, FontStyles.Italic);
        RectTransform creatorRect = creatorGO.GetComponent<RectTransform>();
        creatorRect.anchorMin = new Vector2(0, 0);
        creatorRect.anchorMax = new Vector2(0.7f, 0.5f);
        creatorRect.offsetMin = Vector2.zero;
        creatorRect.offsetMax = Vector2.zero;
        creatorGO.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Card Count
        GameObject countGO = CreateTMPText("CardCount", infoPanel.transform, "12 cartas", 11, FontStyles.Normal);
        RectTransform countRect = countGO.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.7f, 0);
        countRect.anchorMax = new Vector2(1, 0.5f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;
        var countTMP = countGO.GetComponent<TextMeshProUGUI>();
        countTMP.alignment = TextAlignmentOptions.Right;
        countTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Download Button
        GameObject downloadBtnGO = new GameObject("DownloadButton");
        downloadBtnGO.transform.SetParent(cardRoot.transform, false);
        RectTransform downloadRect = downloadBtnGO.AddComponent<RectTransform>();
        downloadRect.anchorMin = new Vector2(0.5f, 0.5f);
        downloadRect.anchorMax = new Vector2(0.5f, 0.5f);
        downloadRect.sizeDelta = new Vector2(100, 35);
        downloadRect.anchoredPosition = new Vector2(0, 20);
        Image downloadBg = downloadBtnGO.AddComponent<Image>();
        downloadBg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        Button downloadBtn = downloadBtnGO.AddComponent<Button>();
        downloadBtn.targetGraphic = downloadBg;

        GameObject downloadTextGO = CreateTMPText("Text", downloadBtnGO.transform, "Baixar", 12, FontStyles.Bold);
        RectTransform downloadTextRect = downloadTextGO.GetComponent<RectTransform>();
        downloadTextRect.anchorMin = Vector2.zero;
        downloadTextRect.anchorMax = Vector2.one;
        downloadTextRect.offsetMin = Vector2.zero;
        downloadTextRect.offsetMax = Vector2.zero;
        downloadTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Download Progress (Slider)
        GameObject progressGO = new GameObject("DownloadProgress");
        progressGO.transform.SetParent(cardRoot.transform, false);
        RectTransform progressRect = progressGO.AddComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.1f, 0.45f);
        progressRect.anchorMax = new Vector2(0.9f, 0.55f);
        progressRect.offsetMin = Vector2.zero;
        progressRect.offsetMax = Vector2.zero;
        Slider progressSlider = progressGO.AddComponent<Slider>();
        progressSlider.minValue = 0;
        progressSlider.maxValue = 1;
        progressSlider.interactable = false;

        // Slider Background
        GameObject sliderBgGO = new GameObject("Background");
        sliderBgGO.transform.SetParent(progressGO.transform, false);
        RectTransform sliderBgRect = sliderBgGO.AddComponent<RectTransform>();
        sliderBgRect.anchorMin = Vector2.zero;
        sliderBgRect.anchorMax = Vector2.one;
        sliderBgRect.offsetMin = Vector2.zero;
        sliderBgRect.offsetMax = Vector2.zero;
        Image sliderBgImg = sliderBgGO.AddComponent<Image>();
        sliderBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Slider Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(progressGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 0.3f, 1f);

        progressSlider.fillRect = fillRect;
        progressGO.SetActive(false);

        // Selected Icon (checkmark)
        GameObject selectedGO = new GameObject("SelectedIcon");
        selectedGO.transform.SetParent(cardRoot.transform, false);
        RectTransform selectedRect = selectedGO.AddComponent<RectTransform>();
        selectedRect.anchorMin = new Vector2(1, 1);
        selectedRect.anchorMax = new Vector2(1, 1);
        selectedRect.pivot = new Vector2(1, 1);
        selectedRect.sizeDelta = new Vector2(30, 30);
        selectedRect.anchoredPosition = new Vector2(-5, -5);
        Image selectedImg = selectedGO.AddComponent<Image>();
        selectedImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        selectedGO.SetActive(false);

        // Downloaded Badge
        GameObject badgeGO = new GameObject("DownloadedBadge");
        badgeGO.transform.SetParent(cardRoot.transform, false);
        RectTransform badgeRect = badgeGO.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0, 1);
        badgeRect.anchorMax = new Vector2(0, 1);
        badgeRect.pivot = new Vector2(0, 1);
        badgeRect.sizeDelta = new Vector2(20, 20);
        badgeRect.anchoredPosition = new Vector2(5, -5);
        Image badgeImg = badgeGO.AddComponent<Image>();
        badgeImg.color = new Color(0.3f, 0.6f, 1f, 1f);
        badgeGO.SetActive(false);

        // Not Ready Overlay
        GameObject notReadyGO = new GameObject("NotReadyOverlay");
        notReadyGO.transform.SetParent(cardRoot.transform, false);
        RectTransform notReadyRect = notReadyGO.AddComponent<RectTransform>();
        notReadyRect.anchorMin = Vector2.zero;
        notReadyRect.anchorMax = Vector2.one;
        notReadyRect.offsetMin = Vector2.zero;
        notReadyRect.offsetMax = Vector2.zero;
        Image notReadyImg = notReadyGO.AddComponent<Image>();
        notReadyImg.color = new Color(0, 0, 0, 0.7f);
        notReadyGO.SetActive(false);

        // Configurar referencias no ThemeCardUI via SerializedObject
        SerializedObject so = new SerializedObject(cardUI);
        so.FindProperty("coverImage").objectReferenceValue = coverImage;
        so.FindProperty("nameText").objectReferenceValue = nameGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("creatorText").objectReferenceValue = creatorGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("cardCountText").objectReferenceValue = countGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("cardButton").objectReferenceValue = cardButton;
        so.FindProperty("downloadButton").objectReferenceValue = downloadBtn;
        so.FindProperty("downloadButtonText").objectReferenceValue = downloadTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("downloadProgress").objectReferenceValue = progressSlider;
        so.FindProperty("selectedIcon").objectReferenceValue = selectedGO;
        so.FindProperty("downloadedBadge").objectReferenceValue = badgeGO;
        so.FindProperty("notReadyOverlay").objectReferenceValue = notReadyGO;
        so.ApplyModifiedProperties();

        // Salvar como prefab
        string prefabPath = "Assets/Prefabs/ThemeCard.prefab";
        PrefabUtility.SaveAsPrefabAsset(cardRoot, prefabPath);
        DestroyImmediate(cardRoot);

        Debug.Log($"ThemeCard prefab created at: {prefabPath}");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    [MenuItem("Tools/TimeCrax/Create Theme Selection Canvas")]
    public static void CreateThemeSelectionCanvas()
    {
        // Criar Canvas
        GameObject canvasGO = new GameObject("ThemeSelectionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        CanvasGroup canvasGroup = canvasGO.AddComponent<CanvasGroup>();

        // Background semi-transparente
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.8f);
        bgImg.raycastTarget = true;

        // Main Panel
        GameObject panelGO = new GameObject("ThemeSelectionPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0.12f, 0.12f, 0.12f, 1f);

        // Header
        GameObject headerGO = new GameObject("Header");
        headerGO.transform.SetParent(panelGO.transform, false);
        RectTransform headerRect = headerGO.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.9f);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.offsetMin = Vector2.zero;
        headerRect.offsetMax = Vector2.zero;

        // Title
        GameObject titleGO = CreateTMPText("TitleText", headerGO.transform, "Selecionar Tema", 24, FontStyles.Bold);
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0);
        titleRect.anchorMax = new Vector2(0.9f, 1);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = Vector2.zero;
        titleGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

        // Close Button
        GameObject closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(headerGO.transform, false);
        RectTransform closeRect = closeBtnGO.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 0.5f);
        closeRect.anchorMax = new Vector2(1, 0.5f);
        closeRect.pivot = new Vector2(1, 0.5f);
        closeRect.sizeDelta = new Vector2(40, 40);
        closeRect.anchoredPosition = new Vector2(-10, 0);
        Image closeBg = closeBtnGO.AddComponent<Image>();
        closeBg.color = new Color(0.6f, 0.2f, 0.2f, 1f);
        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;

        GameObject closeTextGO = CreateTMPText("Text", closeBtnGO.transform, "X", 18, FontStyles.Bold);
        RectTransform closeTextRect = closeTextGO.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        closeTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Grid Area
        GameObject gridGO = new GameObject("ThemeGrid");
        gridGO.transform.SetParent(panelGO.transform, false);
        RectTransform gridRect = gridGO.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0, 0.15f);
        gridRect.anchorMax = new Vector2(1, 0.85f);
        gridRect.offsetMin = new Vector2(20, 10);
        gridRect.offsetMax = new Vector2(-20, -10);
        GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(280, 220);
        grid.spacing = new Vector2(20, 20);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        // Pagination
        GameObject paginationGO = new GameObject("Pagination");
        paginationGO.transform.SetParent(panelGO.transform, false);
        RectTransform pagRect = paginationGO.AddComponent<RectTransform>();
        pagRect.anchorMin = new Vector2(0.3f, 0.05f);
        pagRect.anchorMax = new Vector2(0.7f, 0.12f);
        pagRect.offsetMin = Vector2.zero;
        pagRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup pagLayout = paginationGO.AddComponent<HorizontalLayoutGroup>();
        pagLayout.childAlignment = TextAnchor.MiddleCenter;
        pagLayout.spacing = 20;
        pagLayout.childControlWidth = false;
        pagLayout.childControlHeight = false;

        // Prev Button
        GameObject prevBtnGO = CreateButton("PrevButton", paginationGO.transform, "<", 50, 35);
        // Page Text
        GameObject pageTextGO = CreateTMPText("PageText", paginationGO.transform, "1/1", 16, FontStyles.Normal);
        pageTextGO.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 35);
        pageTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        // Next Button
        GameObject nextBtnGO = CreateButton("NextButton", paginationGO.transform, ">", 50, 35);

        // Footer
        GameObject footerGO = new GameObject("Footer");
        footerGO.transform.SetParent(panelGO.transform, false);
        RectTransform footerRect = footerGO.AddComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0, 0);
        footerRect.anchorMax = new Vector2(1, 0.05f);
        footerRect.offsetMin = new Vector2(20, 0);
        footerRect.offsetMax = new Vector2(-20, 0);

        // Storage Info
        GameObject storageGO = CreateTMPText("StorageInfo", footerGO.transform, "0 tema(s) - 0 MB", 12, FontStyles.Normal);
        RectTransform storageRect = storageGO.GetComponent<RectTransform>();
        storageRect.anchorMin = new Vector2(0, 0);
        storageRect.anchorMax = new Vector2(0.5f, 1);
        storageRect.offsetMin = Vector2.zero;
        storageRect.offsetMax = Vector2.zero;
        storageGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        storageGO.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.6f, 1f);

        // Selected Theme
        GameObject selectedGO = CreateTMPText("SelectedTheme", footerGO.transform, "Nenhum tema selecionado", 12, FontStyles.Normal);
        RectTransform selectedRect = selectedGO.GetComponent<RectTransform>();
        selectedRect.anchorMin = new Vector2(0.5f, 0);
        selectedRect.anchorMax = new Vector2(1, 1);
        selectedRect.offsetMin = Vector2.zero;
        selectedRect.offsetMax = Vector2.zero;
        selectedGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineRight;
        selectedGO.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.6f, 1f);

        // Loading Overlay
        GameObject loadingGO = new GameObject("LoadingOverlay");
        loadingGO.transform.SetParent(panelGO.transform, false);
        RectTransform loadingRect = loadingGO.AddComponent<RectTransform>();
        loadingRect.anchorMin = Vector2.zero;
        loadingRect.anchorMax = Vector2.one;
        loadingRect.offsetMin = Vector2.zero;
        loadingRect.offsetMax = Vector2.zero;
        Image loadingBg = loadingGO.AddComponent<Image>();
        loadingBg.color = new Color(0, 0, 0, 0.7f);
        CanvasGroup loadingCG = loadingGO.AddComponent<CanvasGroup>();

        GameObject loadingTextGO = CreateTMPText("LoadingText", loadingGO.transform, "Carregando...", 18, FontStyles.Normal);
        RectTransform loadingTextRect = loadingTextGO.GetComponent<RectTransform>();
        loadingTextRect.anchorMin = Vector2.zero;
        loadingTextRect.anchorMax = Vector2.one;
        loadingTextRect.offsetMin = Vector2.zero;
        loadingTextRect.offsetMax = Vector2.zero;
        loadingTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Adicionar ThemeSelectionUI
        ThemeSelectionUI selectionUI = canvasGO.AddComponent<ThemeSelectionUI>();

        // Configurar referencias
        SerializedObject so = new SerializedObject(selectionUI);
        so.FindProperty("themeSelectionCanvas").objectReferenceValue = canvasGO;
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.FindProperty("loadingOverlay").objectReferenceValue = loadingGO;
        so.FindProperty("loadingText").objectReferenceValue = loadingTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("themeGrid").objectReferenceValue = gridGO.transform;
        so.FindProperty("prevButton").objectReferenceValue = prevBtnGO.GetComponent<Button>();
        so.FindProperty("nextButton").objectReferenceValue = nextBtnGO.GetComponent<Button>();
        so.FindProperty("pageText").objectReferenceValue = pageTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("titleText").objectReferenceValue = titleGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("storageInfoText").objectReferenceValue = storageGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("selectedThemeText").objectReferenceValue = selectedGO.GetComponent<TextMeshProUGUI>();

        // Carregar prefab do ThemeCard se existir
        GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ThemeCard.prefab");
        if (cardPrefab != null)
            so.FindProperty("themeCardPrefab").objectReferenceValue = cardPrefab;

        so.ApplyModifiedProperties();

        canvasGO.SetActive(false);

        Debug.Log("ThemeSelectionCanvas created in scene. Don't forget to assign ThemeCard prefab if not auto-assigned.");
        Selection.activeGameObject = canvasGO;
    }

    private static GameObject CreateTMPText(string name, Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string text, float width, float height)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        RectTransform rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        Image bg = btnGO.AddComponent<Image>();
        bg.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;

        GameObject textGO = CreateTMPText("Text", btnGO.transform, text, 14, FontStyles.Bold);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        return btnGO;
    }
}
