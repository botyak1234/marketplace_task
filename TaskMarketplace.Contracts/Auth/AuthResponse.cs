using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.Contracts.Auth;

/// <summary>
/// Модель ответа аутентификации
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT токен для доступа к API
    /// </summary>
    [Required]
    public string Token { get; set; }
}
