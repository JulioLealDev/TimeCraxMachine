using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Ferramenta para criar o material base para cartas de evento com temas da API.
/// </summary>
public class EventCardMaterialSetup : EditorWindow
{
    [MenuItem("Tools/TimeCrax/Create Event Card Base Material")]
    public static void CreateEventCardBaseMaterial()
    {
        // Verificar se o shader existe
        Shader compositeShader = Shader.Find("TimeCrax/EventCardComposite");
        if (compositeShader == null)
        {
            EditorUtility.DisplayDialog("Erro",
                "Shader 'TimeCrax/EventCardComposite' não encontrado.\n" +
                "Certifique-se de que o arquivo EventCardComposite.shader existe em Assets/Shaders/",
                "OK");
            return;
        }

        // Criar material
        Material baseMaterial = new Material(compositeShader);
        baseMaterial.name = "EventCardBase";

        // Configurar propriedades padrão
        baseMaterial.SetFloat("_Glossiness", 0.2f);
        baseMaterial.SetFloat("_Metallic", 0f);
        baseMaterial.SetColor("_Color", Color.white);

        // Tentar encontrar textura de template existente
        string[] templateGuids = AssetDatabase.FindAssets("CardTemplate t:Texture2D");
        if (templateGuids.Length > 0)
        {
            string templatePath = AssetDatabase.GUIDToAssetPath(templateGuids[0]);
            Texture2D templateTex = AssetDatabase.LoadAssetAtPath<Texture2D>(templatePath);
            if (templateTex != null)
            {
                baseMaterial.SetTexture("_MainTex", templateTex);
                Debug.Log($"[EventCardMaterialSetup] Template encontrado: {templatePath}");
            }
        }
        else
        {
            Debug.LogWarning("[EventCardMaterialSetup] Textura CardTemplate não encontrada. Crie uma textura PNG com a moldura da carta e centro transparente.");
        }

        // Garantir que a pasta existe
        if (!AssetDatabase.IsValidFolder("Assets/Materials/Themes"))
        {
            AssetDatabase.CreateFolder("Assets/Materials", "Themes");
        }

        // Salvar material
        string materialPath = "Assets/Materials/Themes/EventCardBase.mat";

        // Deletar se já existir
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) != null)
        {
            AssetDatabase.DeleteAsset(materialPath);
        }

        AssetDatabase.CreateAsset(baseMaterial, materialPath);
        AssetDatabase.SaveAssets();

        // Selecionar o material criado
        Selection.activeObject = baseMaterial;

        Debug.Log($"[EventCardMaterialSetup] Material base criado: {materialPath}");

        EditorUtility.DisplayDialog("Sucesso",
            "Material EventCardBase criado!\n\n" +
            "Caminho: " + materialPath + "\n\n" +
            "IMPORTANTE:\n" +
            "1. Crie uma textura PNG chamada 'CardTemplate' com:\n" +
            "   - A moldura/frame da carta\n" +
            "   - Centro TRANSPARENTE onde a imagem do tema aparece\n" +
            "2. Coloque em Assets/Textures/\n" +
            "3. Configure o Import Settings da textura:\n" +
            "   - Alpha Source: Input Texture Alpha\n" +
            "   - Alpha Is Transparency: checked\n" +
            "4. Atribua a textura no campo '_MainTex' do material",
            "OK");
    }

    [MenuItem("Tools/TimeCrax/Configure LegacyThemesData for Themes")]
    public static void ConfigureLegacyThemesData()
    {
        LegacyThemesData legacyThemesData = FindFirstObjectByType<LegacyThemesData>();
        if (legacyThemesData == null)
        {
            EditorUtility.DisplayDialog("Erro",
                "LegacyThemesData não encontrado na cena.",
                "OK");
            return;
        }

        Material baseMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Themes/EventCardBase.mat");
        if (baseMaterial == null)
        {
            if (EditorUtility.DisplayDialog("Material não encontrado",
                "Material EventCardBase não encontrado.\n\nDeseja criar agora?",
                "Criar", "Cancelar"))
            {
                CreateEventCardBaseMaterial();
                baseMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Themes/EventCardBase.mat");
            }
        }

        Texture2D templateTexture = null;
        string[] templateGuids = AssetDatabase.FindAssets("CardTemplate t:Texture2D");
        if (templateGuids.Length > 0)
        {
            string templatePath = AssetDatabase.GUIDToAssetPath(templateGuids[0]);
            templateTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(templatePath);
        }

        SerializedObject serializedObj = new SerializedObject(legacyThemesData);
        serializedObj.FindProperty("eventCardBaseMaterial").objectReferenceValue = baseMaterial;
        serializedObj.FindProperty("cardTemplateTexture").objectReferenceValue = templateTexture;
        serializedObj.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = legacyThemesData.gameObject;

        string templateStatus = templateTexture != null ? "Encontrada" : "NÃO ENCONTRADA - crie CardTemplate.png";

        EditorUtility.DisplayDialog("Configuração",
            $"LegacyThemesData configurado!\n\n" +
            $"Material Base: {(baseMaterial != null ? "OK" : "Não encontrado")}\n" +
            $"Textura Template: {templateStatus}\n\n" +
            "Salve a cena (Ctrl+S).",
            "OK");
    }

    [MenuItem("Tools/TimeCrax/Setup Event Cards for Theme System")]
    public static void SetupEventCardsForThemeSystem()
    {
        // Encontrar todos os EventCards na cena
        EventCard[] eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        if (eventCards.Length == 0)
        {
            EditorUtility.DisplayDialog("Aviso",
                "Nenhum EventCard encontrado na cena.",
                "OK");
            return;
        }

        // Carregar material base
        Material baseMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Themes/EventCardBase.mat");
        if (baseMaterial == null)
        {
            if (EditorUtility.DisplayDialog("Material não encontrado",
                "Material EventCardBase não encontrado.\n\nDeseja criar agora?",
                "Criar", "Cancelar"))
            {
                CreateEventCardBaseMaterial();
                baseMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Themes/EventCardBase.mat");
            }

            if (baseMaterial == null)
                return;
        }

        int configuredCount = 0;
        foreach (var eventCard in eventCards)
        {
            Renderer renderer = eventCard.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Criar instância do material para cada carta
                Material instanceMaterial = new Material(baseMaterial);
                instanceMaterial.name = $"EventCard_{eventCard.name}_Material";
                renderer.sharedMaterial = instanceMaterial;
                configuredCount++;
            }
        }

        // Marcar cena como modificada
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Sucesso",
            $"{configuredCount} EventCards configurados!\n\n" +
            "Salve a cena (Ctrl+S) para manter as alterações.",
            "OK");
    }
}
