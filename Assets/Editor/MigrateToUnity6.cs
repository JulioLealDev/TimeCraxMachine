using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Script de migração para Unity 6
/// Substitui APIs obsoletas pelos novos métodos
///
/// Como usar:
/// 1. Abra o projeto no Unity 6
/// 2. Vá em Tools > TimeCrax > Migrate to Unity 6
/// 3. Revise as mudanças no Console
/// </summary>
public class MigrateToUnity6 : EditorWindow
{
    private static int totalReplacements = 0;
    private static int filesModified = 0;

    [MenuItem("Tools/TimeCrax/Migrate to Unity 6")]
    public static void ShowWindow()
    {
        GetWindow<MigrateToUnity6>("Migrate to Unity 6");
    }

    private void OnGUI()
    {
        GUILayout.Label("Migração para Unity 6", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Este script irá substituir:", EditorStyles.label);
        GUILayout.Label("• FindObjectOfType<T>() → FindFirstObjectByType<T>()", EditorStyles.miniLabel);
        GUILayout.Label("• FindObjectsOfType<T>() → FindObjectsByType<T>(FindObjectsSortMode.None)", EditorStyles.miniLabel);

        GUILayout.Space(20);

        if (GUILayout.Button("Analisar Scripts (Preview)", GUILayout.Height(30)))
        {
            AnalyzeScripts();
        }

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Executar Migração", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Confirmar Migração",
                "Isso irá modificar todos os scripts C# na pasta Assets/Scripts.\n\nCertifique-se de ter um backup!\n\nDeseja continuar?",
                "Sim, Migrar", "Cancelar"))
            {
                ExecuteMigration();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(20);
        GUILayout.Label("Dica: Faça backup antes de executar!", EditorStyles.helpBox);
    }

    private static void AnalyzeScripts()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");

        if (!Directory.Exists(scriptsPath))
        {
            Debug.LogError("Pasta Assets/Scripts não encontrada!");
            return;
        }

        string[] csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
        int findObjectOfTypeCount = 0;
        int findObjectsOfTypeCount = 0;

        foreach (string filePath in csFiles)
        {
            string content = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);

            // Contar FindObjectOfType (singular)
            var matches1 = Regex.Matches(content, @"FindObjectOfType<");
            if (matches1.Count > 0)
            {
                findObjectOfTypeCount += matches1.Count;
                Debug.Log($"[PREVIEW] {fileName}: {matches1.Count}x FindObjectOfType<T>()");
            }

            // Contar FindObjectsOfType (plural)
            var matches2 = Regex.Matches(content, @"FindObjectsOfType<");
            if (matches2.Count > 0)
            {
                findObjectsOfTypeCount += matches2.Count;
                Debug.Log($"[PREVIEW] {fileName}: {matches2.Count}x FindObjectsOfType<T>()");
            }
        }

        Debug.Log("========================================");
        Debug.Log($"RESUMO DA ANÁLISE:");
        Debug.Log($"  FindObjectOfType<T>(): {findObjectOfTypeCount} ocorrências");
        Debug.Log($"  FindObjectsOfType<T>(): {findObjectsOfTypeCount} ocorrências");
        Debug.Log($"  TOTAL: {findObjectOfTypeCount + findObjectsOfTypeCount} substituições necessárias");
        Debug.Log("========================================");
    }

    private static void ExecuteMigration()
    {
        totalReplacements = 0;
        filesModified = 0;

        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");

        if (!Directory.Exists(scriptsPath))
        {
            Debug.LogError("Pasta Assets/Scripts não encontrada!");
            return;
        }

        string[] csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);

        foreach (string filePath in csFiles)
        {
            ProcessFile(filePath);
        }

        AssetDatabase.Refresh();

        Debug.Log("========================================");
        Debug.Log("MIGRAÇÃO CONCLUÍDA!");
        Debug.Log($"  Arquivos modificados: {filesModified}");
        Debug.Log($"  Total de substituições: {totalReplacements}");
        Debug.Log("========================================");

        EditorUtility.DisplayDialog("Migração Concluída",
            $"Migração finalizada!\n\nArquivos modificados: {filesModified}\nSubstituições: {totalReplacements}",
            "OK");
    }

    private static void ProcessFile(string filePath)
    {
        string content = File.ReadAllText(filePath);
        string originalContent = content;
        string fileName = Path.GetFileName(filePath);
        int fileReplacements = 0;

        // Padrão 1: FindObjectsOfType<Type>() → FindObjectsByType<Type>(FindObjectsSortMode.None)
        // Deve vir primeiro para não conflitar com FindObjectOfType
        string pattern1 = @"FindObjectsOfType<([^>]+)>\s*\(\s*\)";
        string replacement1 = "FindObjectsByType<$1>(FindObjectsSortMode.None)";

        var matches1 = Regex.Matches(content, pattern1);
        if (matches1.Count > 0)
        {
            content = Regex.Replace(content, pattern1, replacement1);
            fileReplacements += matches1.Count;
            Debug.Log($"[MIGRADO] {fileName}: {matches1.Count}x FindObjectsOfType → FindObjectsByType");
        }

        // Padrão 2: FindObjectOfType<Type>() → FindFirstObjectByType<Type>()
        string pattern2 = @"FindObjectOfType<([^>]+)>\s*\(\s*\)";
        string replacement2 = "FindFirstObjectByType<$1>()";

        var matches2 = Regex.Matches(content, pattern2);
        if (matches2.Count > 0)
        {
            content = Regex.Replace(content, pattern2, replacement2);
            fileReplacements += matches2.Count;
            Debug.Log($"[MIGRADO] {fileName}: {matches2.Count}x FindObjectOfType → FindFirstObjectByType");
        }

        // Salvar apenas se houve mudanças
        if (content != originalContent)
        {
            File.WriteAllText(filePath, content);
            filesModified++;
            totalReplacements += fileReplacements;
        }
    }
}
