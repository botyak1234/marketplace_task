using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.Contracts.Auth;

/// <summary>
/// Модель запроса на аутентификацию
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required]
    public required string Username { get; set; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    public required string Password { get; set; }
}
