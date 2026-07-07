using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TimeCrax.Core;

namespace TimeCrax.Auth
{
    /// <summary>
    /// Serviço para comunicação com a API de autenticação do TimeCrax Backend.
    /// Implementa login, registro e obtenção de dados do usuário.
    /// </summary>
    public class AuthService : MonoBehaviour
    {
        [Header("Configuração da API")]
        [SerializeField] private string apiBaseUrl = "https://timecrax-backend-production.up.railway.app";

        [Header("Debug")]
        [SerializeField] private bool logRequests = true;

        // Singleton
        private static AuthService _instance;
        public static AuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AuthService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AuthService");
                        _instance = go.AddComponent<AuthService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// URL base da API (pode ser alterada para produção)
        /// </summary>
        public string ApiBaseUrl
        {
            get => apiBaseUrl;
            set => apiBaseUrl = value;
        }

        #region Login

        /// <summary>
        /// Realiza login com email e senha
        /// </summary>
        public void Login(string email, string password, Action<AuthResult> onComplete)
        {
            StartCoroutine(LoginCoroutine(email, password, onComplete));
        }

        private IEnumerator LoginCoroutine(string email, string password, Action<AuthResult> onComplete)
        {
            LoginRequest request = new LoginRequest(email, password);
            string jsonBody = JsonUtility.ToJson(request);

            using (UnityWebRequest www = CreatePostRequest("/auth/login", jsonBody))
            {
                yield return www.SendWebRequest();

                AuthResult result = ProcessAuthResponse(www);

                if (result.Success)
                {
                    TokenManager.SaveTokens(result.Data);

                    // Busca dados completos do usuário após login
                    yield return GetCurrentUserCoroutine(_ => { });
                }

                onComplete?.Invoke(result);
            }
        }

        #endregion

        #region Register

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        public void Register(string firstName, string lastName, string email, string password, Action<AuthResult> onComplete, string language = "pt-br")
        {
            StartCoroutine(RegisterCoroutine(firstName, lastName, email, password, language, onComplete));
        }

        private IEnumerator RegisterCoroutine(string firstName, string lastName, string email, string password, string language, Action<AuthResult> onComplete)
        {
            RegisterRequest request = new RegisterRequest(firstName, lastName, email, password, language);
            string jsonBody = JsonUtility.ToJson(request);

            using (UnityWebRequest www = CreatePostRequest("/auth/register", jsonBody))
            {
                yield return www.SendWebRequest();

                AuthResult result = ProcessAuthResponse(www);

                if (result.Success)
                {
                    TokenManager.SaveTokens(result.Data);
                }

                onComplete?.Invoke(result);
            }
        }

        #endregion

        #region Get User Data

        /// <summary>
        /// Obtém os dados do usuário logado
        /// </summary>
        public void GetCurrentUser(Action<UserResult> onComplete)
        {
            StartCoroutine(GetCurrentUserCoroutine(onComplete));
        }

        private IEnumerator GetCurrentUserCoroutine(Action<UserResult> onComplete)
        {
            if (!TokenManager.IsLoggedIn)
            {
                onComplete?.Invoke(UserResult.Fail("Usuário não está logado"));
                yield break;
            }

            using (UnityWebRequest www = CreateGetRequest("/me"))
            {
                www.SetRequestHeader("Authorization", TokenManager.GetAuthorizationHeader());

                yield return www.SendWebRequest();

                UserResult result = ProcessUserResponse(www);

                if (result.Success)
                {
                    TokenManager.SaveUserData(result.Data);
                }

                onComplete?.Invoke(result);
            }
        }

        #endregion

        #region Logout

        /// <summary>
        /// Realiza logout (limpa tokens locais)
        /// </summary>
        public void Logout()
        {
            TokenManager.ClearTokens();
            DebugHelper.Log("[AuthService] Logout realizado");
        }

        #endregion

        #region Password Validation

        /// <summary>
        /// Valida senha localmente antes de enviar para o servidor
        /// </summary>
        public static (bool isValid, string errorCode) ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, AuthErrorCodes.PASSWORD_REQUIRED);
            }

            if (password.Length < 12)
            {
                return (false, AuthErrorCodes.PASSWORD_TOO_SHORT);
            }

            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
            }

            if (!hasUpper) return (false, AuthErrorCodes.PASSWORD_NO_UPPERCASE);
            if (!hasLower) return (false, AuthErrorCodes.PASSWORD_NO_LOWERCASE);
            if (!hasDigit) return (false, AuthErrorCodes.PASSWORD_NO_DIGIT);

            return (true, null);
        }

        /// <summary>
        /// Valida email localmente
        /// </summary>
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            // Validação simples: contém @ e pelo menos um ponto após @
            int atIndex = email.IndexOf('@');
            if (atIndex <= 0) return false;

            int dotIndex = email.LastIndexOf('.');
            return dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
        }

        #endregion

        #region HTTP Helpers

        private UnityWebRequest CreatePostRequest(string endpoint, string jsonBody)
        {
            string url = apiBaseUrl + endpoint;

            if (logRequests)
            {
                DebugHelper.Log($"[AuthService] POST {url}");
            }

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            UnityWebRequest www = new UnityWebRequest(url, "POST");
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 30;

            return www;
        }

        private UnityWebRequest CreateGetRequest(string endpoint)
        {
            string url = apiBaseUrl + endpoint;

            if (logRequests)
            {
                DebugHelper.Log($"[AuthService] GET {url}");
            }

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.timeout = 30;

            return www;
        }

        private AuthResult ProcessAuthResponse(UnityWebRequest www)
        {
            if (logRequests)
            {
                DebugHelper.Log($"[AuthService] Response: {www.responseCode} - {www.downloadHandler.text}");
            }

            // Erro de rede
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return AuthResult.Fail("NETWORK_ERROR", "Erro de conexão. Verifique sua internet.");
            }

            // Rate limiting
            if (www.responseCode == 429)
            {
                return AuthResult.Fail(AuthErrorCodes.TOO_MANY_REQUESTS);
            }

            // Sucesso
            if (www.responseCode == 200)
            {
                try
                {
                    AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                    return AuthResult.Ok(authResponse);
                }
                catch (Exception e)
                {
                    DebugHelper.Log($"[AuthService] Erro ao parsear resposta: {e.Message}");
                    return AuthResult.Fail("PARSE_ERROR", "Erro ao processar resposta do servidor");
                }
            }

            // Erro do servidor (400, 401, etc.)
            try
            {
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);

                // Verifica erros de campos específicos
                if (errorResponse.errors != null)
                {
                    if (!string.IsNullOrEmpty(errorResponse.errors.password))
                        return AuthResult.Fail(errorResponse.errors.password);
                    if (!string.IsNullOrEmpty(errorResponse.errors.email))
                        return AuthResult.Fail(errorResponse.errors.email);
                    if (!string.IsNullOrEmpty(errorResponse.errors.firstName))
                        return AuthResult.Fail(errorResponse.errors.firstName, "Nome é obrigatório (mínimo 2 caracteres)");
                    if (!string.IsNullOrEmpty(errorResponse.errors.lastName))
                        return AuthResult.Fail(errorResponse.errors.lastName, "Sobrenome é obrigatório (mínimo 2 caracteres)");
                }

                return AuthResult.Fail(errorResponse.code ?? "UNKNOWN_ERROR");
            }
            catch
            {
                return AuthResult.Fail("UNKNOWN_ERROR", $"Erro do servidor: {www.responseCode}");
            }
        }

        private UserResult ProcessUserResponse(UnityWebRequest www)
        {
            if (logRequests)
            {
                DebugHelper.Log($"[AuthService] Response: {www.responseCode}");
            }

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                return UserResult.Fail("Erro de conexão. Verifique sua internet.");
            }

            if (www.responseCode == 401)
            {
                TokenManager.ClearTokens();
                return UserResult.Fail("Sessão expirada. Faça login novamente.");
            }

            if (www.responseCode == 200)
            {
                try
                {
                    UserData userData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
                    return UserResult.Ok(userData);
                }
                catch (Exception e)
                {
                    DebugHelper.Log($"[AuthService] Erro ao parsear usuário: {e.Message}");
                    return UserResult.Fail("Erro ao processar dados do usuário");
                }
            }

            return UserResult.Fail($"Erro do servidor: {www.responseCode}");
        }

        #endregion
    }
}
