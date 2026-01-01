using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/TimeCrax/Find Missing Scripts")]
    public static void FindMissing()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    count++;
                    Debug.LogWarning($"Missing script encontrado em: {GetFullPath(go)}", go);
                }
            }
        }

        if (count == 0)
        {
            Debug.Log("Nenhum missing script encontrado!");
        }
        else
        {
            Debug.LogWarning($"Total: {count} missing scripts encontrados. Clique nas mensagens acima para localizar.");
        }
    }

    [MenuItem("Tools/TimeCrax/Remove All Missing Scripts")]
    public static void RemoveAllMissing()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = 0;

        foreach (GameObject go in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                count += removed;
                EditorUtility.SetDirty(go);
                Debug.Log($"Removido {removed} missing script(s) de: {GetFullPath(go)}");
            }
        }

        AssetDatabase.SaveAssets();

        if (count == 0)
        {
            Debug.Log("Nenhum missing script para remover!");
        }
        else
        {
            Debug.Log($"Total: {count} missing scripts removidos!");
        }
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
