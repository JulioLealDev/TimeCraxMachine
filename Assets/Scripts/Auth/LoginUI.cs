using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Auth
{
    /// <summary>
    /// Controller da UI de Login/Registro.
    /// Gerencia os formulários, validação e navegação.
    /// </summary>
    public class LoginUI : MonoBehaviour
    {
        [Header("Painéis")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Login - Campos")]
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button goToRegisterButton;
        [SerializeField] private TextMeshProUGUI loginErrorText;

        [Header("Registro - Campos")]
        [SerializeField] private TMP_InputField registerFirstNameInput;
        [SerializeField] private TMP_InputField registerLastNameInput;
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private TMP_InputField registerConfirmPasswordInput;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button goToLoginButton;
        [SerializeField] private TextMeshProUGUI registerErrorText;

        [Header("Configurações")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private bool autoLoginIfTokenExists = true;

        [Header("Audio")]
        [SerializeField] private SoundEffects soundEffects;

        private bool isLoading = false;

        private void Start()
        {
            SetupButtons();
            SetupInputFields();
            ClearErrors();

            // Tenta auto-login se já tem token válido
            if (autoLoginIfTokenExists && TokenManager.IsLoggedIn)
            {
                DebugHelper.Log("[LoginUI] Token válido encontrado, verificando...");
                ValidateExistingSession();
            }
            else
            {
                ShowLoginPanel();
            }
        }

        private void SetupButtons()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);

            if (goToRegisterButton != null)
                goToRegisterButton.onClick.AddListener(ShowRegisterPanel);

            if (registerButton != null)
                registerButton.onClick.AddListener(OnRegisterClicked);

            if (goToLoginButton != null)
                goToLoginButton.onClick.AddListener(ShowLoginPanel);
        }

        private void SetupInputFields()
        {
            // Configura campos de senha para ocultar texto
            if (loginPasswordInput != null)
                loginPasswordInput.contentType = TMP_InputField.ContentType.Password;

            if (registerPasswordInput != null)
                registerPasswordInput.contentType = TMP_InputField.ContentType.Password;

            if (registerConfirmPasswordInput != null)
                registerConfirmPasswordInput.contentType = TMP_InputField.ContentType.Password;

            // Permite enviar com Enter
            if (loginPasswordInput != null)
                loginPasswordInput.onSubmit.AddListener(_ => OnLoginClicked());

            if (registerConfirmPasswordInput != null)
                registerConfirmPasswordInput.onSubmit.AddListener(_ => OnRegisterClicked());
        }

        #region Panel Navigation

        public void ShowLoginPanel()
        {
            PlayButtonSound();
            if (loginPanel != null) loginPanel.SetActive(true);
            if (registerPanel != null) registerPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            ClearErrors();
        }

        public void ShowRegisterPanel()
        {
            PlayButtonSound();
            if (loginPanel != null) loginPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(true);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            ClearErrors();
        }

        private void ShowLoading(bool show)
        {
            isLoading = show;

            if (loadingPanel != null)
                loadingPanel.SetActive(show);

            // Desabilita botões durante loading
            if (loginButton != null)
                loginButton.interactable = !show;

            if (registerButton != null)
                registerButton.interactable = !show;
        }

        private void ClearErrors()
        {
            if (loginErrorText != null)
            {
                loginErrorText.text = string.Empty;
                loginErrorText.gameObject.SetActive(false);
            }

            if (registerErrorText != null)
            {
                registerErrorText.text = string.Empty;
                registerErrorText.gameObject.SetActive(false);
            }
        }

        private void ShowLoginError(string message)
        {
            if (loginErrorText != null)
            {
                loginErrorText.text = message;
                loginErrorText.gameObject.SetActive(true);
            }
        }

        private void ShowRegisterError(string message)
        {
            if (registerErrorText != null)
            {
                registerErrorText.text = message;
                registerErrorText.gameObject.SetActive(true);
            }
        }

        #endregion

        #region Login

        private void OnLoginClicked()
        {
            if (isLoading) return;

            PlayButtonSound();
            ClearErrors();

            string email = loginEmailInput?.text?.Trim() ?? string.Empty;
            string password = loginPasswordInput?.text ?? string.Empty;

            // Validação local
            if (string.IsNullOrEmpty(email))
            {
                ShowLoginError("Digite seu email");
                return;
            }

            if (!AuthService.ValidateEmail(email))
            {
                ShowLoginError("Email inválido");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowLoginError("Digite sua senha");
                return;
            }

            // Envia para o servidor
            ShowLoading(true);
            AuthService.Instance.Login(email, password, OnLoginComplete);
        }

        private void OnLoginComplete(AuthResult result)
        {
            ShowLoading(false);

            if (result.Success)
            {
                DebugHelper.Log("[LoginUI] Login bem-sucedido!");
                OnAuthenticationSuccess();
            }
            else
            {
                DebugHelper.Log($"[LoginUI] Erro no login: {result.ErrorCode}");
                ShowLoginError(result.ErrorMessage);
            }
        }

        #endregion

        #region Register

        private void OnRegisterClicked()
        {
            if (isLoading) return;

            PlayButtonSound();
            ClearErrors();

            string firstName = registerFirstNameInput?.text?.Trim() ?? string.Empty;
            string lastName = registerLastNameInput?.text?.Trim() ?? string.Empty;
            string email = registerEmailInput?.text?.Trim() ?? string.Empty;
            string password = registerPasswordInput?.text ?? string.Empty;
            string confirmPassword = registerConfirmPasswordInput?.text ?? string.Empty;

            // Validações locais
            if (string.IsNullOrEmpty(firstName) || firstName.Length < 2)
            {
                ShowRegisterError("Nome deve ter pelo menos 2 caracteres");
                return;
            }

            if (string.IsNullOrEmpty(lastName) || lastName.Length < 2)
            {
                ShowRegisterError("Sobrenome deve ter pelo menos 2 caracteres");
                return;
            }

            if (!AuthService.ValidateEmail(email))
            {
                ShowRegisterError("Email inválido");
                return;
            }

            var (isValidPassword, passwordError) = AuthService.ValidatePassword(password);
            if (!isValidPassword)
            {
                ShowRegisterError(AuthErrorCodes.GetMessage(passwordError));
                return;
            }

            if (password != confirmPassword)
            {
                ShowRegisterError("As senhas não coincidem");
                return;
            }

            // Envia para o servidor
            ShowLoading(true);
            AuthService.Instance.Register(firstName, lastName, email, password, OnRegisterComplete);
        }

        private void OnRegisterComplete(AuthResult result)
        {
            ShowLoading(false);

            if (result.Success)
            {
                DebugHelper.Log("[LoginUI] Registro bem-sucedido!");
                OnAuthenticationSuccess();
            }
            else
            {
                DebugHelper.Log($"[LoginUI] Erro no registro: {result.ErrorCode}");
                ShowRegisterError(result.ErrorMessage);
            }
        }

        #endregion

        #region Session Validation

        private void ValidateExistingSession()
        {
            ShowLoading(true);

            AuthService.Instance.GetCurrentUser(result =>
            {
                ShowLoading(false);

                if (result.Success)
                {
                    DebugHelper.Log($"[LoginUI] Sessão válida para: {result.Data.FullName}");
                    OnAuthenticationSuccess();
                }
                else
                {
                    DebugHelper.Log("[LoginUI] Sessão inválida, mostrando login");
                    ShowLoginPanel();
                }
            });
        }

        #endregion

        #region Navigation

        private void OnAuthenticationSuccess()
        {
            // Salva o nickname para uso no jogo (compatibilidade com SessionData existente)
            SessionData.Nickname = TokenManager.UserName;

            DebugHelper.Log($"[LoginUI] Navegando para: {mainMenuSceneName}");

            // Carrega a cena principal
            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        /// <summary>
        /// Botão de logout (pode ser chamado de outros lugares)
        /// </summary>
        public void OnLogoutClicked()
        {
            PlayButtonSound();
            AuthService.Instance.Logout();
            ShowLoginPanel();

            // Limpa campos
            if (loginEmailInput != null) loginEmailInput.text = string.Empty;
            if (loginPasswordInput != null) loginPasswordInput.text = string.Empty;
        }

        /// <summary>
        /// Botão para jogar como convidado (sem login)
        /// </summary>
        public void OnPlayAsGuestClicked()
        {
            PlayButtonSound();
            SessionData.Nickname = "Jogador";
            SceneManager.LoadScene(mainMenuSceneName);
        }

        #endregion

        #region Audio

        private void PlayButtonSound()
        {
            if (soundEffects != null)
            {
                soundEffects.PressHudButtonSound();
            }
        }

        #endregion
    }
}
