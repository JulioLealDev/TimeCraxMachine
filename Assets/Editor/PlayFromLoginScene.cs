using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Adiciona opção no menu para iniciar o jogo sempre pela LoginScreen.
/// Use: Edit > Play From Login Scene (Ctrl+Shift+P)
/// </summary>
public static class PlayFromLoginScene
{
    private const string LOGIN_SCENE_PATH = "Assets/Scenes/LoginScreen.unity";

    [MenuItem("Edit/Play From Login Scene %#p")] // Ctrl+Shift+P
    public static void PlayFromLogin()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        // Salva a cena atual se tiver mudanças
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Abre a cena de login e inicia o jogo
        EditorSceneManager.OpenScene(LOGIN_SCENE_PATH);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Edit/Play From Login Scene %#p", true)]
    public static bool PlayFromLoginValidate()
    {
        return !EditorApplication.isCompiling;
    }
}
