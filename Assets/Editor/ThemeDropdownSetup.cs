using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TimeCrax.Themes;

/// <summary>
/// Ferramentas de Editor para criar o ThemeDropdownUI
/// </summary>
public class ThemeDropdownSetup : EditorWindow
{
    /// <summary>
    /// Cria o painel dropdown e o item prefab
    /// </summary>
    [MenuItem("Tools/TimeCrax/Create Theme Dropdown Panel")]
    public static void CreateThemeDropdownPanel()
    {
        // Verificar se HUDCreateRoom existe
        GameObject hudCreateRoom = GameObject.Find("HUDCreateRoom");
        if (hudCreateRoom == null)
        {
            EditorUtility.DisplayDialog("Erro",
                "HUDCreateRoom não encontrado na cena.\n" +
                "Certifique-se de que a cena correta está aberta.",
                "OK");
            return;
        }

        // Criar item prefab primeiro
        GameObject itemPrefab = CreateThemeItemPrefab();

        // Procurar ThemeDropdownArrow
        Transform arrowTransform = FindDeepChild(hudCreateRoom.transform, "ThemeDropdownArrow");
        if (arrowTransform == null)
        {
            EditorUtility.DisplayDialog("Erro",
                "ThemeDropdownArrow não encontrado em HUDCreateRoom.",
                "OK");
            return;
        }

        // Procurar ThemeSelected (texto do tema selecionado)
        Transform themeTextTransform = FindDeepChild(hudCreateRoom.transform, "ThemeSelected");
        TextMeshProUGUI selectedThemeText = null;
        if (themeTextTransform != null)
        {
            selectedThemeText = themeTextTransform.GetComponent<TextMeshProUGUI>();
            Debug.Log($"[ThemeDropdownSetup] ThemeSelected encontrado: {selectedThemeText != null}");
        }
        else
        {
            Debug.LogWarning("[ThemeDropdownSetup] ThemeSelected não encontrado em HUDCreateRoom!");
        }

        // Criar painel dropdown como filho do HUDCreateRoom
        GameObject dropdownPanel = new GameObject("ThemeDropdownPanel");
        dropdownPanel.transform.SetParent(hudCreateRoom.transform, false);

        // Obter posição do arrow para posicionar o painel
        RectTransform arrowRect = arrowTransform.GetComponent<RectTransform>();
        Vector2 arrowPos = arrowRect.anchoredPosition;

        RectTransform panelRect = dropdownPanel.AddComponent<RectTransform>();
        // Posicionar abaixo do dropdown arrow usando a mesma posição X
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        // Posicionar abaixo do arrow (arrow está em ~722, -63)
        panelRect.anchoredPosition = new Vector2(arrowPos.x - 100, arrowPos.y - 180);
        panelRect.sizeDelta = new Vector2(300, 250);

        Debug.Log($"[ThemeDropdownSetup] Arrow position: {arrowPos}, Panel position: {panelRect.anchoredPosition}");

        // Background do painel
        Image panelBg = dropdownPanel.AddComponent<Image>();
        panelBg.color = new Color(0.12f, 0.12f, 0.15f, 0.98f);

        // Outline
        Outline outline = dropdownPanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.6f, 0.5f, 0.2f, 1f);
        outline.effectDistance = new Vector2(2, 2);

        // CanvasGroup para fade
        CanvasGroup canvasGroup = dropdownPanel.AddComponent<CanvasGroup>();

        // Content container DIRETO no painel (sem ScrollView)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(dropdownPanel.transform, false);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(10, 10);
        contentRect.offsetMax = new Vector2(-10, -10);
        contentRect.pivot = new Vector2(0.5f, 1);

        VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = 5;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);

        // Adicionar ThemeDropdownUI ao HUDCreateRoom ou criar novo GameObject
        ThemeDropdownUI dropdownUI = hudCreateRoom.GetComponent<ThemeDropdownUI>();
        if (dropdownUI == null)
        {
            dropdownUI = hudCreateRoom.AddComponent<ThemeDropdownUI>();
        }

        // Configurar referências via SerializedObject
        SerializedObject serializedDropdown = new SerializedObject(dropdownUI);

        // Pegar o botão do arrow
        Button arrowButton = arrowTransform.GetComponent<Button>();
        if (arrowButton == null)
            arrowButton = arrowTransform.gameObject.AddComponent<Button>();

        serializedDropdown.FindProperty("dropdownArrowButton").objectReferenceValue = arrowButton;
        serializedDropdown.FindProperty("selectedThemeText").objectReferenceValue = selectedThemeText;
        serializedDropdown.FindProperty("dropdownPanel").objectReferenceValue = dropdownPanel;
        serializedDropdown.FindProperty("contentContainer").objectReferenceValue = content.transform;
        serializedDropdown.FindProperty("themeItemPrefab").objectReferenceValue = itemPrefab;
        serializedDropdown.ApplyModifiedProperties();

        // Desativar painel por padrão
        dropdownPanel.SetActive(false);

        // Selecionar o objeto criado
        Selection.activeGameObject = dropdownPanel;

        // Marcar cena como modificada
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[ThemeDropdownSetup] ThemeDropdownPanel criado com sucesso!");

        EditorUtility.DisplayDialog("Sucesso",
            "ThemeDropdownPanel criado!\n\n" +
            "O painel foi adicionado ao HUDCreateRoom.\n\n" +
            "Lembre-se de:\n" +
            "1. Ajustar a posição do painel se necessário\n" +
            "2. Atribuir SoundEffects no ThemeDropdownUI\n" +
            "3. Salvar a cena (Ctrl+S)",
            "OK");
    }

    /// <summary>
    /// Cria apenas o prefab do item de tema
    /// </summary>
    [MenuItem("Tools/TimeCrax/Create Theme Item Prefab")]
    public static GameObject CreateThemeItemPrefab()
    {
        // Criar GameObject do item
        GameObject itemGO = new GameObject("ThemeDropdownItem");

        RectTransform itemRect = itemGO.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(280, 45);

        // LayoutElement para garantir tamanho no VerticalLayoutGroup
        LayoutElement layoutElement = itemGO.AddComponent<LayoutElement>();
        layoutElement.minHeight = 45;
        layoutElement.preferredHeight = 45;
        layoutElement.flexibleWidth = 1;

        // Background do item
        Image itemBg = itemGO.AddComponent<Image>();
        itemBg.color = new Color(0.18f, 0.18f, 0.22f, 1f);

        // Button
        Button itemButton = itemGO.AddComponent<Button>();
        ColorBlock colors = itemButton.colors;
        colors.normalColor = new Color(0.18f, 0.18f, 0.22f, 1f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.18f, 1f);
        colors.selectedColor = new Color(0.3f, 0.5f, 0.3f, 1f);
        itemButton.colors = colors;

        // Texto do tema
        GameObject textGO = new GameObject("ThemeName");
        textGO.transform.SetParent(itemGO.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 5);
        textRect.offsetMax = new Vector2(-15, -5);

        TextMeshProUGUI themeText = textGO.AddComponent<TextMeshProUGUI>();
        themeText.text = "Nome do Tema";
        themeText.fontSize = 18;
        themeText.alignment = TextAlignmentOptions.MidlineLeft;
        themeText.color = Color.white;

        // Salvar como prefab
        string prefabPath = "Assets/Prefabs/UI/ThemeDropdownItem.prefab";

        // Garantir que a pasta existe
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        // Deletar prefab existente para recriar com as correções
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
            Debug.Log("[ThemeDropdownSetup] Prefab antigo deletado para recriação");
        }

        // Criar novo prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemGO, prefabPath);
        DestroyImmediate(itemGO);

        Debug.Log("[ThemeDropdownSetup] Prefab criado: " + prefabPath);

        return prefab;
    }

    /// <summary>
    /// Força recriação do prefab do item de tema
    /// </summary>
    [MenuItem("Tools/TimeCrax/Recreate Theme Item Prefab")]
    public static void RecreateThemeItemPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/ThemeDropdownItem.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (existingPrefab != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        CreateThemeItemPrefab();

        EditorUtility.DisplayDialog("Sucesso",
            "Prefab ThemeDropdownItem recriado!\n\n" +
            "Use 'Delete Theme Dropdown Panel' e 'Create Theme Dropdown Panel' para atualizar.",
            "OK");
    }

    /// <summary>
    /// Verifica e corrige o ThemeDropdownPanel existente
    /// </summary>
    [MenuItem("Tools/TimeCrax/Fix Theme Dropdown Panel")]
    public static void FixThemeDropdownPanel()
    {
        GameObject hudCreateRoom = GameObject.Find("HUDCreateRoom");
        if (hudCreateRoom == null)
        {
            EditorUtility.DisplayDialog("Erro", "HUDCreateRoom não encontrado na cena.", "OK");
            return;
        }

        // Procurar painel existente
        Transform panelTransform = FindDeepChild(hudCreateRoom.transform, "ThemeDropdownPanel");
        if (panelTransform == null)
        {
            EditorUtility.DisplayDialog("Aviso",
                "ThemeDropdownPanel não encontrado.\nUse 'Create Theme Dropdown Panel' primeiro.",
                "OK");
            return;
        }

        GameObject dropdownPanel = panelTransform.gameObject;
        int fixCount = 0;

        // 1. Verificar Canvas para sorting
        Canvas canvas = dropdownPanel.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = dropdownPanel.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            fixCount++;
            Debug.Log("[ThemeDropdownSetup] Canvas adicionado ao painel");
        }
        else if (!canvas.overrideSorting)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            fixCount++;
            Debug.Log("[ThemeDropdownSetup] Canvas sorting corrigido");
        }

        // 2. Verificar GraphicRaycaster
        if (dropdownPanel.GetComponent<GraphicRaycaster>() == null)
        {
            dropdownPanel.AddComponent<GraphicRaycaster>();
            fixCount++;
            Debug.Log("[ThemeDropdownSetup] GraphicRaycaster adicionado");
        }

        // 3. Verificar CanvasGroup
        CanvasGroup canvasGroup = dropdownPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dropdownPanel.AddComponent<CanvasGroup>();
            fixCount++;
            Debug.Log("[ThemeDropdownSetup] CanvasGroup adicionado");
        }

        // 4. Verificar e corrigir posição do painel
        RectTransform panelRect = dropdownPanel.GetComponent<RectTransform>();

        // Encontrar ThemeDropdownArrow para obter posição correta
        Transform arrowTransform = FindDeepChild(hudCreateRoom.transform, "ThemeDropdownArrow");
        if (arrowTransform != null)
        {
            RectTransform arrowRect = arrowTransform.GetComponent<RectTransform>();
            Vector2 arrowPos = arrowRect.anchoredPosition;
            Vector2 correctPos = new Vector2(arrowPos.x - 100, arrowPos.y - 180);

            if (Vector2.Distance(panelRect.anchoredPosition, correctPos) > 50)
            {
                panelRect.anchoredPosition = correctPos;
                fixCount++;
                Debug.Log($"[ThemeDropdownSetup] Posição do painel corrigida para: {correctPos}");
            }
        }

        if (panelRect.sizeDelta.x < 100 || panelRect.sizeDelta.y < 100)
        {
            panelRect.sizeDelta = new Vector2(300, 250);
            fixCount++;
            Debug.Log("[ThemeDropdownSetup] Tamanho do painel corrigido");
        }

        // 5. Verificar referência ao ThemeSelected
        ThemeDropdownUI dropdownUI = hudCreateRoom.GetComponent<ThemeDropdownUI>();
        if (dropdownUI != null)
        {
            SerializedObject serializedDropdown = new SerializedObject(dropdownUI);
            var selectedTextProp = serializedDropdown.FindProperty("selectedThemeText");

            if (selectedTextProp.objectReferenceValue == null)
            {
                Transform themeSelectedTransform = FindDeepChild(hudCreateRoom.transform, "ThemeSelected");
                if (themeSelectedTransform != null)
                {
                    TextMeshProUGUI themeSelectedText = themeSelectedTransform.GetComponent<TextMeshProUGUI>();
                    if (themeSelectedText != null)
                    {
                        selectedTextProp.objectReferenceValue = themeSelectedText;
                        serializedDropdown.ApplyModifiedProperties();
                        fixCount++;
                        Debug.Log("[ThemeDropdownSetup] Referência ThemeSelected corrigida");
                    }
                }
            }
        }

        // 6. Verificar Content (estrutura simplificada sem ScrollView)
        Transform contentTransform = dropdownPanel.transform.Find("Content");
        if (contentTransform != null)
        {
            RectTransform contentRect = contentTransform.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.one;
                contentRect.offsetMin = new Vector2(10, 10);
                contentRect.offsetMax = new Vector2(-10, -10);
            }

            // Verificar VerticalLayoutGroup
            VerticalLayoutGroup vlg = contentTransform.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 5;
                vlg.padding = new RectOffset(0, 0, 0, 0);
                fixCount++;
                Debug.Log("[ThemeDropdownSetup] VerticalLayoutGroup adicionado ao Content");
            }
            else
            {
                // Corrigir configurações do layout
                vlg.childControlHeight = true;
                Debug.Log("[ThemeDropdownSetup] VerticalLayoutGroup corrigido");
            }
        }
        else
        {
            Debug.LogWarning("[ThemeDropdownSetup] Content não encontrado! Recrie o painel.");
        }

        // Marcar cena como modificada
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = dropdownPanel;

        if (fixCount > 0)
        {
            EditorUtility.DisplayDialog("Correções Aplicadas",
                $"{fixCount} correção(ões) aplicada(s) ao ThemeDropdownPanel.\n\n" +
                "Lembre-se de salvar a cena (Ctrl+S).",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Verificação Completa",
                "ThemeDropdownPanel está configurado corretamente.",
                "OK");
        }
    }

    /// <summary>
    /// Mostra o painel dropdown para debug (ativa temporariamente)
    /// </summary>
    [MenuItem("Tools/TimeCrax/Show Theme Dropdown (Debug)")]
    public static void ShowThemeDropdownDebug()
    {
        GameObject hudCreateRoom = GameObject.Find("HUDCreateRoom");
        if (hudCreateRoom == null)
        {
            EditorUtility.DisplayDialog("Erro", "HUDCreateRoom não encontrado na cena.", "OK");
            return;
        }

        // Ativar HUDCreateRoom temporariamente
        bool wasActive = hudCreateRoom.activeSelf;
        hudCreateRoom.SetActive(true);

        Transform panelTransform = FindDeepChild(hudCreateRoom.transform, "ThemeDropdownPanel");
        if (panelTransform == null)
        {
            if (!wasActive) hudCreateRoom.SetActive(false);
            EditorUtility.DisplayDialog("Erro", "ThemeDropdownPanel não encontrado.", "OK");
            return;
        }

        GameObject panel = panelTransform.gameObject;
        panel.SetActive(true);

        // Verificar CanvasGroup alpha
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        RectTransform panelRect = panel.GetComponent<RectTransform>();

        Selection.activeGameObject = panel;

        Debug.Log($"[ThemeDropdownSetup] Panel ativado para debug:");
        Debug.Log($"  - Position: {panelRect.anchoredPosition}");
        Debug.Log($"  - Size: {panelRect.sizeDelta}");
        Debug.Log($"  - Active: {panel.activeSelf}");
        Debug.Log($"  - CanvasGroup Alpha: {(cg != null ? cg.alpha.ToString() : "N/A")}");

        EditorUtility.DisplayDialog("Debug",
            $"ThemeDropdownPanel ativado!\n\n" +
            $"Posição: {panelRect.anchoredPosition}\n" +
            $"Tamanho: {panelRect.sizeDelta}\n\n" +
            "Verifique a Game View ou Scene View para ver o painel.\n" +
            "O HUDCreateRoom também foi ativado.",
            "OK");
    }

    /// <summary>
    /// Deleta o ThemeDropdownPanel para recriação
    /// </summary>
    [MenuItem("Tools/TimeCrax/Delete Theme Dropdown Panel")]
    public static void DeleteThemeDropdownPanel()
    {
        GameObject hudCreateRoom = GameObject.Find("HUDCreateRoom");
        if (hudCreateRoom == null)
        {
            EditorUtility.DisplayDialog("Erro", "HUDCreateRoom não encontrado na cena.", "OK");
            return;
        }

        Transform panelTransform = FindDeepChild(hudCreateRoom.transform, "ThemeDropdownPanel");
        if (panelTransform == null)
        {
            EditorUtility.DisplayDialog("Aviso", "ThemeDropdownPanel não encontrado.", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Confirmar Exclusão",
            "Deseja deletar o ThemeDropdownPanel?\n\nIsso permitirá recriar o painel do zero.",
            "Deletar", "Cancelar"))
        {
            // Também remover ThemeDropdownUI do HUDCreateRoom
            ThemeDropdownUI dropdownUI = hudCreateRoom.GetComponent<ThemeDropdownUI>();
            if (dropdownUI != null)
            {
                DestroyImmediate(dropdownUI);
            }

            DestroyImmediate(panelTransform.gameObject);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Sucesso",
                "ThemeDropdownPanel deletado.\n\n" +
                "Use 'Create Theme Dropdown Panel' para criar novamente.",
                "OK");
        }
    }

    /// <summary>
    /// Encontra um filho recursivamente pelo nome
    /// </summary>
    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindDeepChild(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
