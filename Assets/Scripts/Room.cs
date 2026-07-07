using TMPro;
using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Themes;

public class Room : MonoBehaviour
{
    public TextMeshProUGUI buttonName;
    public TextMeshProUGUI isLocked;

    // Dados do tema da sala
    private string themeId;
    private string themeName;
    private string themeCreatorName;
    private int themeCardCount;
    private string themeCoverUrl;

    public void SetThemeData(string id, string name)
    {
        themeId = id;
        themeName = name;
    }

    public void SetThemeData(string id, string name, string creatorName, int cardCount, string coverUrl)
    {
        themeId = id;
        themeName = name;
        themeCreatorName = creatorName;
        themeCardCount = cardCount;
        themeCoverUrl = coverUrl;
    }

    public string GetThemeId() => themeId;
    public string GetThemeName() => themeName;
    public string GetThemeCreatorName() => themeCreatorName;
    public int GetThemeCardCount() => themeCardCount;
    public string GetThemeCoverUrl() => themeCoverUrl;

    public void JoinRoom()
    {
        GameConnection gameConnection = FindFirstObjectByType<GameConnection>();
        PasswordScreen passwordScreen = FindFirstObjectByType<PasswordScreen>(FindObjectsInactive.Include);
        LobbyOptions lobbyOptions = FindFirstObjectByType<LobbyOptions>(FindObjectsInactive.Include);

        // Prevenir cliques múltiplos
        if (gameConnection != null && gameConnection.IsProcessingRoomOperation())
        {
            DebugHelper.Log("[Room] Operação em andamento, ignorando clique duplicado");
            return;
        }

        // Verificar se é um tema da API e se o jogador possui
        DebugHelper.Log($"[Room] JoinRoom - themeId: '{themeId}', themeName: '{themeName}'");

        if (!string.IsNullOrEmpty(themeId))
        {
            bool hasTheme = ThemeManager.Instance != null && ThemeManager.Instance.IsThemeDownloaded(themeId);
            DebugHelper.Log($"[Room] ThemeManager existe: {ThemeManager.Instance != null}, hasTheme: {hasTheme}");

            if (!hasTheme)
            {
                // Mostrar tela de download necessário
                var downloadNeeded = FindFirstObjectByType<ThemeDownloadNeededUI>(FindObjectsInactive.Include);
                DebugHelper.Log($"[Room] Tema não baixado. ThemeDownloadNeededUI encontrado: {downloadNeeded != null}");
                if (downloadNeeded != null)
                {
                    bool roomIsLocked = isLocked.text == "Yes";
                    downloadNeeded.Show(themeId, themeName, gameObject.name, roomIsLocked,
                                       themeCreatorName, themeCardCount, themeCoverUrl);
                    lobbyOptions.ActivateButtons(false);
                }
                else
                {
                    DebugHelper.Log("[Room] ERRO: ThemeDownloadNeededUI não encontrado na cena!");
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
            gameConnection.JoinRoomInList(gameObject.name);
        }
    }
}
