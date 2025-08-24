using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.Contracts.Tasks;

/// <summary>
/// Модель ответа с баллами пользователя
/// </summary>
public class PointsResponse
{
    /// <summary>
    /// Количество баллов пользователя
    /// </summary>
    [Required]
    public int Points { get; set; }
}