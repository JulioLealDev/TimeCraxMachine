using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu para limpar tokens de autenticação durante desenvolvimento.
/// Use: Edit > Clear Auth Tokens
/// </summary>
public static class ClearAuthTokens
{
    [MenuItem("Edit/Clear Auth Tokens")]
    public static void ClearTokens()
    {
        PlayerPrefs.DeleteKey("auth_access_token");
        PlayerPrefs.DeleteKey("auth_refresh_token");
        PlayerPrefs.DeleteKey("auth_token_expiration");
        PlayerPrefs.DeleteKey("auth_user_id");
        PlayerPrefs.DeleteKey("auth_user_email");
        PlayerPrefs.DeleteKey("auth_user_name");
        PlayerPrefs.Save();

        Debug.Log("[ClearAuthTokens] Tokens de autenticação limpos!");
    }
}
