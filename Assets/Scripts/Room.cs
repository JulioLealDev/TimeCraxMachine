using TMPro;
using UnityEngine;
using TimeCrax.Themes;

public class Room : MonoBehaviour
{
    public TextMeshProUGUI buttonName;
    public TextMeshProUGUI isLocked;

    // Dados do tema da sala
    private string themeId;
    private string themeName;

    public void SetThemeData(string id, string name)
    {
        themeId = id;
        themeName = name;
    }

    public string GetThemeId() => themeId;
    public string GetThemeName() => themeName;

    public void JoinRoom()
    {
        PasswordScreen passwordScreen = FindFirstObjectByType<PasswordScreen>(FindObjectsInactive.Include);
        LobbyOptions lobbyOptions = FindFirstObjectByType<LobbyOptions>(FindObjectsInactive.Include);

        // Verificar se é um tema da API e se o jogador possui
        if (!string.IsNullOrEmpty(themeId))
        {
            bool hasTheme = ThemeManager.Instance != null && ThemeManager.Instance.IsThemeDownloaded(themeId);

            if (!hasTheme)
            {
                // Mostrar popup de download
                var downloadPrompt = FindFirstObjectByType<ThemeDownloadPromptUI>(FindObjectsInactive.Include);
                if (downloadPrompt != null)
                {
                    downloadPrompt.Show(themeId, themeName, gameObject.name);
                    lobbyOptions.ActivateButtons(false);
                }
                return;
            }
        }

        // Fluxo normal de entrada
        if (isLocked.text == "Yes")
        {
            passwordScreen.gameObject.SetActive(true);
            passwordScreen.ActivateBackground(true);
            passwordScreen.SetRoomName(gameObject.name);
            lobbyOptions.ActivateButtons(false);
        }
        else
        {
            GameObject.Find("GameConnection").GetComponent<GameConnection>().JoinRoomInList(gameObject.name);
        }
    }
}
