using System;
using UnityEngine;
using TimeCrax.Core;

namespace TimeCrax.Auth
{
    /// <summary>
    /// Gerencia o armazenamento seguro de tokens de autenticação.
    ///
    /// NOTA DE SEGURANÇA: Para produção, considere usar:
    /// - Unity's Keychain (iOS) / Keystore (Android)
    /// - Encriptação adicional para PlayerPrefs
    /// - Secure Enclave em dispositivos suportados
    /// </summary>
    public static class TokenManager
    {
        private const string ACCESS_TOKEN_KEY = "auth_access_token";
        private const string REFRESH_TOKEN_KEY = "auth_refresh_token";
        private const string EXPIRATION_KEY = "auth_token_expiration";
        private const string USER_ID_KEY = "auth_user_id";
        private const string USER_EMAIL_KEY = "auth_user_email";
        private const string USER_NAME_KEY = "auth_user_name";

        /// <summary>
        /// Token de acesso atual
        /// </summary>
        public static string AccessToken
        {
            get => PlayerPrefs.GetString(ACCESS_TOKEN_KEY, string.Empty);
            private set => PlayerPrefs.SetString(ACCESS_TOKEN_KEY, value);
        }

        /// <summary>
        /// Refresh token para renovar o access token
        /// </summary>
        public static string RefreshToken
        {
            get => PlayerPrefs.GetString(REFRESH_TOKEN_KEY, string.Empty);
            private set => PlayerPrefs.SetString(REFRESH_TOKEN_KEY, value);
        }

        /// <summary>
        /// Data de expiração do access token
        /// </summary>
        public static DateTime TokenExpiration
        {
            get
            {
                string stored = PlayerPrefs.GetString(EXPIRATION_KEY, string.Empty);
                if (DateTime.TryParse(stored, out DateTime result))
                {
                    return result;
                }
                return DateTime.MinValue;
            }
            private set => PlayerPrefs.SetString(EXPIRATION_KEY, value.ToString("O"));
        }

        /// <summary>
        /// ID do usuário logado (extraído do JWT)
        /// </summary>
        public static string UserId
        {
            get => PlayerPrefs.GetString(USER_ID_KEY, string.Empty);
            private set => PlayerPrefs.SetString(USER_ID_KEY, value);
        }

        /// <summary>
        /// Email do usuário logado
        /// </summary>
        public static string UserEmail
        {
            get => PlayerPrefs.GetString(USER_EMAIL_KEY, string.Empty);
            private set => PlayerPrefs.SetString(USER_EMAIL_KEY, value);
        }

        /// <summary>
        /// Nome do usuário logado
        /// </summary>
        public static string UserName
        {
            get => PlayerPrefs.GetString(USER_NAME_KEY, string.Empty);
            private set => PlayerPrefs.SetString(USER_NAME_KEY, value);
        }

        /// <summary>
        /// Verifica se há um usuário logado com token válido
        /// </summary>
        public static bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken) && !IsTokenExpired;

        /// <summary>
        /// Verifica se o token atual está expirado
        /// </summary>
        public static bool IsTokenExpired => TokenExpiration <= DateTime.UtcNow;

        /// <summary>
        /// Verifica se o token está próximo de expirar (menos de 10 minutos)
        /// </summary>
        public static bool IsTokenExpiringSoon => TokenExpiration <= DateTime.UtcNow.AddMinutes(10);

        /// <summary>
        /// Salva os tokens de autenticação após login/registro bem-sucedido
        /// </summary>
        public static void SaveTokens(AuthResponse authResponse)
        {
            if (authResponse == null)
            {
                DebugHelper.Log("[TokenManager] AuthResponse é nulo, não salvando tokens");
                return;
            }

            AccessToken = authResponse.accessToken ?? string.Empty;
            RefreshToken = authResponse.refreshToken ?? string.Empty;
            TokenExpiration = authResponse.GetExpirationDate();

            // Extrai claims do JWT
            ExtractClaimsFromToken(authResponse.accessToken);

            PlayerPrefs.Save();
            DebugHelper.Log($"[TokenManager] Tokens salvos. Expira em: {TokenExpiration}");
        }

        /// <summary>
        /// Salva os dados do usuário após obter do endpoint /me
        /// </summary>
        public static void SaveUserData(UserData userData)
        {
            if (userData == null) return;

            UserId = userData.id ?? string.Empty;
            UserEmail = userData.email ?? string.Empty;
            UserName = userData.firstName ?? string.Empty;

            PlayerPrefs.Save();
            DebugHelper.Log($"[TokenManager] Dados do usuário salvos: {UserName}");
        }

        /// <summary>
        /// Limpa todos os dados de autenticação (logout)
        /// </summary>
        public static void ClearTokens()
        {
            PlayerPrefs.DeleteKey(ACCESS_TOKEN_KEY);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(EXPIRATION_KEY);
            PlayerPrefs.DeleteKey(USER_ID_KEY);
            PlayerPrefs.DeleteKey(USER_EMAIL_KEY);
            PlayerPrefs.DeleteKey(USER_NAME_KEY);
            PlayerPrefs.Save();

            DebugHelper.Log("[TokenManager] Tokens limpos (logout)");
        }

        /// <summary>
        /// Extrai informações do payload do JWT token
        /// </summary>
        private static void ExtractClaimsFromToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            try
            {
                // JWT tem 3 partes: header.payload.signature
                string[] parts = token.Split('.');
                if (parts.Length != 3) return;

                // Decodifica o payload (base64url)
                string payload = parts[1];

                // Adiciona padding se necessário
                int padding = 4 - (payload.Length % 4);
                if (padding != 4)
                {
                    payload += new string('=', padding);
                }

                // Converte de base64url para base64 padrão
                payload = payload.Replace('-', '+').Replace('_', '/');

                byte[] bytes = Convert.FromBase64String(payload);
                string json = System.Text.Encoding.UTF8.GetString(bytes);

                // Parse manual do JSON (evita dependência do JsonUtility para objetos dinâmicos)
                UserId = ExtractJsonValue(json, "nameid");
                UserEmail = ExtractJsonValue(json, "email");
                UserName = ExtractJsonValue(json, "name");

                DebugHelper.Log($"[TokenManager] Claims extraídos - ID: {UserId}, Email: {UserEmail}");
            }
            catch (Exception e)
            {
                DebugHelper.Log($"[TokenManager] Erro ao extrair claims do JWT: {e.Message}");
            }
        }

        /// <summary>
        /// Extrai um valor de uma string JSON simples
        /// </summary>
        private static string ExtractJsonValue(string json, string key)
        {
            string searchKey = $"\"{key}\":\"";
            int startIndex = json.IndexOf(searchKey);
            if (startIndex == -1) return string.Empty;

            startIndex += searchKey.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1) return string.Empty;

            return json.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Retorna o header de autorização para requisições HTTP
        /// </summary>
        public static string GetAuthorizationHeader()
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                return string.Empty;
            }
            return $"Bearer {AccessToken}";
        }
    }
}
