using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using TimeCrax.Core;

namespace TimeCrax.Auth
{
    /// <summary>
    /// Controller da UI de Login.
    /// Gerencia o formulário de login e redirecionamento para registro no site.
    /// </summary>
    public class LoginUI : MonoBehaviour
    {
        [Header("Painéis")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject exitConfirmPanel;

        [Header("Login - Campos")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Configurações")]
        [SerializeField] private string mainMenuSceneName = "TimeCraxMachine";
        [SerializeField] private string registerWebsiteUrl = "http://localhost:5173/register";
        [SerializeField] private bool autoLoginIfTokenExists = true;
        [SerializeField] private float minimumLoadingTime = 3f;

        [Header("Exit Confirm")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Button cancelButton;

        [Header("Audio")]
        [SerializeField] private SoundEffects soundEffects;

        private bool isLoading = false;
        private bool isExitDialogOpen = false;
        private float loadingStartTime;
        private AuthResult pendingResult;
        private EventSystem cachedEventSystem;

        private void Start()
        {
            // Cache do EventSystem
            cachedEventSystem = EventSystem.current;

            SetupButtons();
            SetupInputFields();
            ClearError();

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

        private void Update()
        {
            // Bloqueia input durante loading
            if (isLoading) return;

            // ESC para abrir/fechar diálogo de saída
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isExitDialogOpen)
                {
                    HideExitDialog();
                }
                else
                {
                    ShowExitDialog();
                }
                return;
            }

            // Tab para navegar entre campos (apenas se o diálogo de saída não estiver aberto)
            if (!isExitDialogOpen && Input.GetKeyDown(KeyCode.Tab))
            {
                if (emailInput != null && emailInput.isFocused)
                {
                    emailInput.DeactivateInputField();
                    passwordInput?.ActivateInputField();
                    passwordInput?.Select();
                }
                else if (passwordInput != null && passwordInput.isFocused)
                {
                    passwordInput.DeactivateInputField();
                    emailInput?.ActivateInputField();
                    emailInput?.Select();
                }
            }
        }

        private void SetupButtons()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);

            if (registerButton != null)
                registerButton.onClick.AddListener(OnRegisterClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelExitClicked);

            // Esconder painel de confirmação de saída inicialmente
            if (exitConfirmPanel != null)
                exitConfirmPanel.SetActive(false);
        }

        private void SetupInputFields()
        {
            // Configura campo de senha para ocultar texto
            if (passwordInput != null)
                passwordInput.contentType = TMP_InputField.ContentType.Password;

            // Permite enviar com Enter
            if (passwordInput != null)
                passwordInput.onSubmit.AddListener(_ => OnLoginClicked());
        }

        #region Panel Navigation

        public void ShowLoginPanel()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            ClearError();
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

            // Desabilita todo input (mouse/teclado) durante loading
            if (cachedEventSystem != null)
            {
                cachedEventSystem.enabled = !show;
            }
        }

        private void ClearError()
        {
            if (errorText != null)
            {
                errorText.text = string.Empty;
                errorText.gameObject.SetActive(false);
            }
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }

        #endregion

        #region Login

        private void OnLoginClicked()
        {
            if (isLoading) return;

            PlayButtonSound();
            ClearError();

            string email = emailInput?.text?.Trim() ?? string.Empty;
            string password = passwordInput?.text ?? string.Empty;

            // Validação local
            if (string.IsNullOrEmpty(email))
            {
                ShowError("Digite seu email");
                return;
            }

            if (!AuthService.ValidateEmail(email))
            {
                ShowError("Email inválido");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Digite sua senha");
                return;
            }

            // Envia para o servidor
            ShowLoading(true);
            loadingStartTime = Time.time;
            pendingResult = null;
            AuthService.Instance.Login(email, password, OnLoginComplete);
        }

        private void OnLoginComplete(AuthResult result)
        {
            pendingResult = result;
            StartCoroutine(WaitMinimumLoadingTime());
        }

        private IEnumerator WaitMinimumLoadingTime()
        {
            // Calcula quanto tempo falta para completar o tempo mínimo
            float elapsedTime = Time.time - loadingStartTime;
            float remainingTime = minimumLoadingTime - elapsedTime;

            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }

            // Agora processa o resultado
            ShowLoading(false);

            if (pendingResult.Success)
            {
                DebugHelper.Log("[LoginUI] Login bem-sucedido!");
                OnAuthenticationSuccess();
            }
            else
            {
                DebugHelper.Log($"[LoginUI] Erro no login: {pendingResult.ErrorCode}");
                ShowError(pendingResult.ErrorMessage);
            }
        }

        #endregion

        #region Register (Website Redirect)

        /// <summary>
        /// Abre o site de registro no navegador
        /// </summary>
        private void OnRegisterClicked()
        {
            PlayButtonSound();
            DebugHelper.Log($"[LoginUI] Abrindo URL de registro: {registerWebsiteUrl}");
            Application.OpenURL(registerWebsiteUrl);
        }

        #endregion

        #region Session Validation

        private void ValidateExistingSession()
        {
            ShowLoading(true);
            loadingStartTime = Time.time;

            AuthService.Instance.GetCurrentUser(result =>
            {
                StartCoroutine(WaitMinimumLoadingTimeForSession(result));
            });
        }

        private IEnumerator WaitMinimumLoadingTimeForSession(UserResult result)
        {
            // Calcula quanto tempo falta para completar o tempo mínimo
            float elapsedTime = Time.time - loadingStartTime;
            float remainingTime = minimumLoadingTime - elapsedTime;

            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }

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
            if (emailInput != null) emailInput.text = string.Empty;
            if (passwordInput != null) passwordInput.text = string.Empty;
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

        #region Exit Confirmation

        private void ShowExitDialog()
        {
            if (exitConfirmPanel == null) return;

            PlayButtonSound();
            isExitDialogOpen = true;
            exitConfirmPanel.SetActive(true);

            // Desabilita interação com o painel de login
            if (loginButton != null)
                loginButton.interactable = false;
            if (registerButton != null)
                registerButton.interactable = false;
            if (emailInput != null)
                emailInput.interactable = false;
            if (passwordInput != null)
                passwordInput.interactable = false;
        }

        private void HideExitDialog()
        {
            if (exitConfirmPanel == null) return;

            PlayButtonSound();
            isExitDialogOpen = false;
            exitConfirmPanel.SetActive(false);

            // Reabilita interação com o painel de login
            if (loginButton != null)
                loginButton.interactable = true;
            if (registerButton != null)
                registerButton.interactable = true;
            if (emailInput != null)
                emailInput.interactable = true;
            if (passwordInput != null)
                passwordInput.interactable = true;
        }

        private void OnExitClicked()
        {
            PlayButtonSound();
            DebugHelper.Log("[LoginUI] Saindo do jogo...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void OnCancelExitClicked()
        {
            HideExitDialog();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Remover listeners para evitar memory leaks
            if (loginButton != null)
                loginButton.onClick.RemoveListener(OnLoginClicked);

            if (registerButton != null)
                registerButton.onClick.RemoveListener(OnRegisterClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelExitClicked);

            if (passwordInput != null)
                passwordInput.onSubmit.RemoveAllListeners();
        }

        #endregion
    }
}
