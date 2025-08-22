using System.ComponentModel.DataAnnotations;
using TaskMarketplace.Contracts.Enums;

namespace TaskMarketplace.Contracts.Tasks;

// <summary>
/// Запрос на обновление задачи
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// Название задачи
    /// </summary>
    /// <example>Fix authentication bug</example>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
    public required string Title { get; set; }

    /// <summary>
    /// Описание задачи
    /// </summary>
    /// <example>Fix the issue with JWT token expiration</example>
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public required string Description { get; set; }

    /// <summary>
    /// Награда за выполнение задачи (в баллах)
    /// </summary>
    /// <example>50</example>
    [Range(1, 1000, ErrorMessage = "Reward must be between 1 and 1000")]
    public int Reward { get; set; }

    /// <summary>
    /// Статус задачи
    /// </summary>
    /// <example>InProgress</example>
    [EnumDataType(typeof(MarketplaceTaskStatus), ErrorMessage = "Invalid status value")]
    public MarketplaceTaskStatus Status { get; set; }

    /// <summary>
    /// ID пользователя, взявшего задачу (опционально)
    /// </summary>
    /// <example>5</example>
    [Range(1, int.MaxValue, ErrorMessage = "TakenByUserId must be a positive number")]
    public int? TakenByUserId { get; set; }
}