using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ferramenta para substituir fontes em todos os componentes TextMeshPro do projeto.
/// Acesse via: Tools > TimeCrax > Font Replacer
/// </summary>
public class FontReplacerTool : EditorWindow
{
    // Fontes
    private TMP_FontAsset newFont;
    private TMP_FontAsset headerFont;
    private TMP_FontAsset bodyFont;

    // Opções
    private bool useSeperateHeaderFont = false;
    private bool searchInScenes = true;
    private bool searchInPrefabs = true;
    private bool previewOnly = true;

    // Filtros para identificar headers
    private string[] headerKeywords = new string[] { "Title", "Header", "Titulo", "Cabeçalho", "Name" };
    private int minHeaderFontSize = 36;

    // Resultados
    private Vector2 scrollPosition;
    private List<FontReplaceInfo> foundComponents = new List<FontReplaceInfo>();
    private bool hasSearched = false;

    [MenuItem("Tools/TimeCrax/Font Replacer")]
    public static void ShowWindow()
    {
        var window = GetWindow<FontReplacerTool>("Font Replacer");
        window.minSize = new Vector2(450, 500);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Substituir Fontes TextMeshPro", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        DrawFontSelection();
        EditorGUILayout.Space(10);

        DrawSearchOptions();
        EditorGUILayout.Space(10);

        DrawActionButtons();
        EditorGUILayout.Space(10);

        DrawResults();
    }

    private void DrawFontSelection()
    {
        EditorGUILayout.LabelField("Fontes", EditorStyles.boldLabel);

        useSeperateHeaderFont = EditorGUILayout.Toggle("Usar fonte diferente para títulos", useSeperateHeaderFont);

        if (useSeperateHeaderFont)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            headerFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "Fonte para Títulos (Cinzel)",
                headerFont,
                typeof(TMP_FontAsset),
                false);

            bodyFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "Fonte para Texto (Marcellus)",
                bodyFont,
                typeof(TMP_FontAsset),
                false);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Critérios para identificar títulos:", EditorStyles.miniLabel);
            minHeaderFontSize = EditorGUILayout.IntField("Tamanho mínimo de fonte", minHeaderFontSize);

            EditorGUILayout.LabelField("Palavras-chave no nome do objeto:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(string.Join(", ", headerKeywords), EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }
        else
        {
            newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "Nova Fonte",
                newFont,
                typeof(TMP_FontAsset),
                false);
        }
    }

    private void DrawSearchOptions()
    {
        EditorGUILayout.LabelField("Onde buscar", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        searchInScenes = EditorGUILayout.ToggleLeft("Cenas do Build", searchInScenes, GUILayout.Width(150));
        searchInPrefabs = EditorGUILayout.ToggleLeft("Prefabs", searchInPrefabs);
        EditorGUILayout.EndHorizontal();

        previewOnly = EditorGUILayout.Toggle("Apenas visualizar (não aplicar)", previewOnly);
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Buscar Componentes", GUILayout.Height(30)))
        {
            SearchForComponents();
        }

        GUI.enabled = hasSearched && foundComponents.Count > 0 && !previewOnly;
        if (GUILayout.Button("Aplicar Substituição", GUILayout.Height(30)))
        {
            ApplyReplacement();
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawResults()
    {
        if (!hasSearched) return;

        EditorGUILayout.LabelField($"Encontrados: {foundComponents.Count} componentes", EditorStyles.boldLabel);

        if (foundComponents.Count == 0)
        {
            EditorGUILayout.HelpBox("Nenhum componente TextMeshPro encontrado.", MessageType.Info);
            return;
        }

        // Estatísticas
        int headerCount = foundComponents.Count(x => x.isHeader);
        int bodyCount = foundComponents.Count - headerCount;

        if (useSeperateHeaderFont)
        {
            EditorGUILayout.LabelField($"  Títulos: {headerCount} | Texto: {bodyCount}", EditorStyles.miniLabel);
        }

        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));

        foreach (var info in foundComponents)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Ícone
            string icon = info.isHeader ? "d_TextAsset Icon" : "d_Font Icon";
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(icon), GUILayout.Width(20));

            // Info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(info.objectName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{info.location} | Size: {info.fontSize}", EditorStyles.miniLabel);

            string currentFontName = info.currentFont != null ? info.currentFont.name : "(nenhuma)";
            string newFontName = info.newFont != null ? info.newFont.name : "(nenhuma)";
            EditorGUILayout.LabelField($"{currentFontName} → {newFontName}", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            // Botão para selecionar
            if (info.gameObject != null && GUILayout.Button("Selecionar", GUILayout.Width(70)))
            {
                Selection.activeObject = info.gameObject;
                EditorGUIUtility.PingObject(info.gameObject);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void SearchForComponents()
    {
        foundComponents.Clear();
        hasSearched = true;

        if (searchInScenes)
        {
            SearchInScenes();
        }

        if (searchInPrefabs)
        {
            SearchInPrefabs();
        }

        Debug.Log($"[FontReplacer] Encontrados {foundComponents.Count} componentes TextMeshPro");
    }

    private void SearchInScenes()
    {
        // Busca em todas as cenas do Build Settings
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;

            var loadedScene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);

            var rootObjects = loadedScene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                SearchInGameObject(root, scene.path);
            }

            // Fecha a cena se não for a cena atual
            if (loadedScene != EditorSceneManager.GetActiveScene())
            {
                EditorSceneManager.CloseScene(loadedScene, true);
            }
        }
    }

    private void SearchInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                SearchInGameObject(prefab, path, true);
            }
        }
    }

    private void SearchInGameObject(GameObject obj, string location, bool isPrefab = false)
    {
        // Busca TextMeshProUGUI (UI)
        var tmpUGUIComponents = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmpUGUIComponents)
        {
            AddFoundComponent(tmp.gameObject, tmp.font, tmp.fontSize, location, isPrefab);
        }

        // Busca TextMeshPro (3D)
        var tmp3DComponents = obj.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var tmp in tmp3DComponents)
        {
            AddFoundComponent(tmp.gameObject, tmp.font, tmp.fontSize, location, isPrefab);
        }
    }

    private void AddFoundComponent(GameObject obj, TMP_FontAsset currentFont, float fontSize, string location, bool isPrefab)
    {
        bool isHeader = IsHeader(obj.name, fontSize);
        TMP_FontAsset targetFont;

        if (useSeperateHeaderFont)
        {
            targetFont = isHeader ? headerFont : bodyFont;
        }
        else
        {
            targetFont = newFont;
        }

        foundComponents.Add(new FontReplaceInfo
        {
            gameObject = obj,
            objectName = obj.name,
            location = location,
            currentFont = currentFont,
            newFont = targetFont,
            fontSize = fontSize,
            isHeader = isHeader,
            isPrefab = isPrefab
        });
    }

    private bool IsHeader(string objectName, float fontSize)
    {
        // Verifica por tamanho de fonte
        if (fontSize >= minHeaderFontSize)
        {
            return true;
        }

        // Verifica por palavras-chave no nome
        string nameLower = objectName.ToLower();
        foreach (string keyword in headerKeywords)
        {
            if (nameLower.Contains(keyword.ToLower()))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyReplacement()
    {
        int replacedCount = 0;
        HashSet<string> modifiedPrefabs = new HashSet<string>();

        foreach (var info in foundComponents)
        {
            if (info.newFont == null || info.gameObject == null) continue;

            // Para prefabs, precisamos editar o asset
            if (info.isPrefab)
            {
                string prefabPath = info.location;

                if (!modifiedPrefabs.Contains(prefabPath))
                {
                    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                    // Aplica em todos os componentes do prefab
                    ApplyFontToGameObject(prefabRoot);

                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);

                    modifiedPrefabs.Add(prefabPath);
                }
            }
            else
            {
                // Para cenas, aplica diretamente
                var tmpUGUI = info.gameObject.GetComponent<TextMeshProUGUI>();
                if (tmpUGUI != null)
                {
                    Undo.RecordObject(tmpUGUI, "Replace Font");
                    tmpUGUI.font = info.newFont;
                    EditorUtility.SetDirty(tmpUGUI);
                }

                var tmp3D = info.gameObject.GetComponent<TextMeshPro>();
                if (tmp3D != null)
                {
                    Undo.RecordObject(tmp3D, "Replace Font");
                    tmp3D.font = info.newFont;
                    EditorUtility.SetDirty(tmp3D);
                }
            }

            replacedCount++;
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkAllScenesDirty();

        Debug.Log($"[FontReplacer] Substituídas {replacedCount} fontes em {modifiedPrefabs.Count} prefabs e cenas");
        EditorUtility.DisplayDialog("Concluído", $"Fontes substituídas: {replacedCount}\nPrefabs modificados: {modifiedPrefabs.Count}", "OK");

        // Atualiza a busca para mostrar o novo estado
        SearchForComponents();
    }

    private void ApplyFontToGameObject(GameObject obj)
    {
        var tmpUGUIComponents = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmpUGUIComponents)
        {
            bool isHeader = IsHeader(tmp.gameObject.name, tmp.fontSize);
            TMP_FontAsset targetFont = useSeperateHeaderFont
                ? (isHeader ? headerFont : bodyFont)
                : newFont;

            if (targetFont != null)
            {
                tmp.font = targetFont;
            }
        }

        var tmp3DComponents = obj.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var tmp in tmp3DComponents)
        {
            bool isHeader = IsHeader(tmp.gameObject.name, tmp.fontSize);
            TMP_FontAsset targetFont = useSeperateHeaderFont
                ? (isHeader ? headerFont : bodyFont)
                : newFont;

            if (targetFont != null)
            {
                tmp.font = targetFont;
            }
        }
    }

    private class FontReplaceInfo
    {
        public GameObject gameObject;
        public string objectName;
        public string location;
        public TMP_FontAsset currentFont;
        public TMP_FontAsset newFont;
        public float fontSize;
        public bool isHeader;
        public bool isPrefab;
    }
}
