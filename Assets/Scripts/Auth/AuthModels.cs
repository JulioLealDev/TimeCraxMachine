using System;

namespace TimeCrax.Auth
{
    /// <summary>
    /// Request para login
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;

        public LoginRequest(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }

    /// <summary>
    /// Request para registro
    /// </summary>
    [Serializable]
    public class RegisterRequest
    {
        public string role;
        public string firstName;
        public string lastName;
        public string email;
        public string password;
        public string language;

        public RegisterRequest(string firstName, string lastName, string email, string password, string language = "pt-br")
        {
            this.role = "player"; // Sempre "player" para o jogo
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.password = password;
            this.language = language;
        }
    }

    /// <summary>
    /// Resposta de autenticação (login/registro)
    /// </summary>
    [Serializable]
    public class AuthResponse
    {
        public string accessToken;
        public string refreshToken;
        public string accessTokenExpiresAt;

        public DateTime GetExpirationDate()
        {
            if (DateTime.TryParse(accessTokenExpiresAt, out DateTime result))
            {
                return result;
            }
            return DateTime.UtcNow.AddMinutes(300); // Fallback: 5 horas
        }
    }

    /// <summary>
    /// Dados do usuário logado
    /// </summary>
    [Serializable]
    public class UserData
    {
        public string id;
        public string role;
        public string firstName;
        public string lastName;
        public string email;
        public string schoolName;
        public string picture;
        public int score;
        public string createdAt;
        public string updatedAt;

        public string FullName => $"{firstName} {lastName}";
    }

    /// <summary>
    /// Resposta de erro da API
    /// </summary>
    [Serializable]
    public class ErrorResponse
    {
        public string code;
        public ErrorDetails errors;
    }

    [Serializable]
    public class ErrorDetails
    {
        public string email;
        public string password;
        public string firstName;
        public string lastName;
        public string role;
    }

    /// <summary>
    /// Códigos de erro da API
    /// </summary>
    public static class AuthErrorCodes
    {
        public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
        public const string EMAIL_IN_USE = "EMAIL_IN_USE";
        public const string INVALID_EMAIL = "INVALID_EMAIL";
        public const string INVALID_ROLE = "INVALID_ROLE";
        public const string PASSWORD_REQUIRED = "PASSWORD_REQUIRED";
        public const string PASSWORD_TOO_SHORT = "PASSWORD_TOO_SHORT";
        public const string PASSWORD_NO_UPPERCASE = "PASSWORD_NO_UPPERCASE";
        public const string PASSWORD_NO_LOWERCASE = "PASSWORD_NO_LOWERCASE";
        public const string PASSWORD_NO_DIGIT = "PASSWORD_NO_DIGIT";
        public const string USER_NOT_FOUND = "USER_NOT_FOUND";
        public const string TOO_MANY_REQUESTS = "TOO_MANY_REQUESTS";

        /// <summary>
        /// Converte código de erro para mensagem amigável em português
        /// </summary>
        public static string GetMessage(string code)
        {
            return code switch
            {
                INVALID_CREDENTIALS => "Email ou senha incorretos",
                EMAIL_IN_USE => "Este email já está em uso",
                INVALID_EMAIL => "Email inválido",
                INVALID_ROLE => "Tipo de usuário inválido",
                PASSWORD_REQUIRED => "Senha é obrigatória",
                PASSWORD_TOO_SHORT => "Senha deve ter no mínimo 12 caracteres",
                PASSWORD_NO_UPPERCASE => "Senha deve conter pelo menos uma letra maiúscula",
                PASSWORD_NO_LOWERCASE => "Senha deve conter pelo menos uma letra minúscula",
                PASSWORD_NO_DIGIT => "Senha deve conter pelo menos um número",
                USER_NOT_FOUND => "Usuário não encontrado",
                TOO_MANY_REQUESTS => "Muitas tentativas. Aguarde alguns minutos.",
                _ => "Erro desconhecido. Tente novamente."
            };
        }
    }

    /// <summary>
    /// Resultado de uma operação de autenticação
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public AuthResponse Data { get; private set; }

        public static AuthResult Ok(AuthResponse data)
        {
            return new AuthResult { Success = true, Data = data };
        }

        public static AuthResult Fail(string errorCode)
        {
            return new AuthResult
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = AuthErrorCodes.GetMessage(errorCode)
            };
        }

        public static AuthResult Fail(string errorCode, string customMessage)
        {
            return new AuthResult
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = customMessage
            };
        }
    }

    /// <summary>
    /// Resultado de obter dados do usuário
    /// </summary>
    public class UserResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public UserData Data { get; private set; }

        public static UserResult Ok(UserData data)
        {
            return new UserResult { Success = true, Data = data };
        }

        public static UserResult Fail(string message)
        {
            return new UserResult { Success = false, ErrorMessage = message };
        }
    }
}
