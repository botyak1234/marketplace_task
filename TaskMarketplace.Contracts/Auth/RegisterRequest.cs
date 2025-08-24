using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.Contracts.Auth;

/// <summary>
/// Модель запроса на регистрацию
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required]
    public required string Username { get; set; }
    
    /// <summary>
    /// Хеш пароля
    /// </summary>
    [Required]
    public required string HashPassword { get; set; }
}
