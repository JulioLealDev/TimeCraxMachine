using UnityEngine;
using TMPro;
using TimeCrax.Core;
using TimeCrax.Auth;

/// <summary>
/// Exibe o nome do usuário logado em um TextMeshPro.
/// Atualiza automaticamente no Start e quando o usuário faz login.
/// </summary>
public class UserNameDisplay : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private string guestName = "Player";

    private void Start()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        UpdateDisplayName();
    }

    /// <summary>
    /// Atualiza o texto com o nome do usuário logado.
    /// Usa TokenManager se logado, ou SessionData.Nickname, ou nome de convidado.
    /// </summary>
    public void UpdateDisplayName()
    {
        if (textMesh == null)
        {
            return;
        }

        string displayName;

        // Prioridade: TokenManager > SessionData > Guest
        if (TokenManager.IsLoggedIn && !string.IsNullOrEmpty(TokenManager.UserName))
        {
            displayName = TokenManager.UserName;
        }
        else if (!string.IsNullOrEmpty(SessionData.Nickname))
        {
            displayName = SessionData.Nickname;
        }
        else
        {
            displayName = guestName;
        }

        textMesh.text = displayName;
    }
}
