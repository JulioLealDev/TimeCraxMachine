using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ferramenta para habilitar Read/Write em todos os modelos do projeto.
/// Necessário para o OutlineComponent funcionar corretamente.
/// Use: Edit > Fix All Mesh Read-Write (execução direta)
/// Ou: Tools > TimeCrax > Enable Mesh Read-Write (janela)
/// </summary>
public class EnableMeshReadWrite : EditorWindow
{
    // Atalho rápido - executa direto sem janela
    [MenuItem("Edit/Fix All Mesh Read-Write")]
    public static void FixAllMeshesQuick()
    {
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        int fixedCount = 0;
        int alreadyEnabled = 0;
        int totalModels = modelGuids.Length;
        List<string> fixedFiles = new List<string>();

        for (int i = 0; i < modelGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(modelGuids[i]);

            EditorUtility.DisplayProgressBar(
                "Habilitando Read/Write",
                $"({i + 1}/{totalModels}) {System.IO.Path.GetFileName(path)}",
                (float)i / totalModels);

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                if (!importer.isReadable)
                {
                    importer.isReadable = true;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    fixedCount++;
                    fixedFiles.Add(path);
                    Debug.Log($"[Fix] Habilitado: {path}");
                }
                else
                {
                    alreadyEnabled++;
                }
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        Debug.Log($"[EnableMeshReadWrite] === RESUMO ===");
        Debug.Log($"[EnableMeshReadWrite] Total de modelos: {totalModels}");
        Debug.Log($"[EnableMeshReadWrite] Já estavam OK: {alreadyEnabled}");
        Debug.Log($"[EnableMeshReadWrite] Corrigidos agora: {fixedCount}");

        if (fixedCount > 0)
        {
            Debug.Log($"[EnableMeshReadWrite] Arquivos corrigidos:");
            foreach (var f in fixedFiles)
            {
                Debug.Log($"  - {f}");
            }
        }

        EditorUtility.DisplayDialog("Concluído",
            $"Total: {totalModels} modelos\nJá OK: {alreadyEnabled}\nCorrigidos: {fixedCount}", "OK");
    }

    private Vector2 scrollPosition;
    private List<string> modelsToFix = new List<string>();
    private bool hasSearched = false;

    [MenuItem("Tools/TimeCrax/Enable Mesh Read-Write")]
    public static void ShowWindow()
    {
        var window = GetWindow<EnableMeshReadWrite>("Mesh Read/Write");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Habilitar Read/Write em Modelos", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "O OutlineComponent precisa acessar os vértices dos meshes em runtime. " +
            "Esta ferramenta habilita 'Read/Write Enabled' em todos os modelos.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Buscar Modelos", GUILayout.Height(30)))
        {
            SearchModels();
        }

        GUI.enabled = modelsToFix.Count > 0;
        if (GUILayout.Button($"Corrigir Todos ({modelsToFix.Count})", GUILayout.Height(30)))
        {
            FixAllModels();
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        if (hasSearched)
        {
            if (modelsToFix.Count == 0)
            {
                EditorGUILayout.HelpBox("Todos os modelos já estão com Read/Write habilitado!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"Modelos para corrigir: {modelsToFix.Count}", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                foreach (var path in modelsToFix)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel);

                    if (GUILayout.Button("Selecionar", GUILayout.Width(70)))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }

    private void SearchModels()
    {
        modelsToFix.Clear();
        hasSearched = true;

        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });

        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer != null && !importer.isReadable)
            {
                modelsToFix.Add(path);
            }
        }

        Debug.Log($"[EnableMeshReadWrite] Encontrados {modelsToFix.Count} modelos sem Read/Write habilitado");
    }

    private void FixAllModels()
    {
        int fixed_count = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < modelsToFix.Count; i++)
            {
                string path = modelsToFix[i];

                EditorUtility.DisplayProgressBar(
                    "Habilitando Read/Write",
                    $"Processando: {path}",
                    (float)i / modelsToFix.Count);

                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    fixed_count++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        modelsToFix.Clear();
        hasSearched = false;

        Debug.Log($"[EnableMeshReadWrite] Corrigidos {fixed_count} modelos!");
        EditorUtility.DisplayDialog("Concluído", $"Read/Write habilitado em {fixed_count} modelos.", "OK");

        // Atualiza a busca
        SearchModels();
    }
}
