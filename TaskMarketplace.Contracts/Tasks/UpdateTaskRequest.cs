using System.ComponentModel.DataAnnotations;
using TaskMarketplace.Contracts.Enums;

namespace TaskMarketplace.Contracts.Tasks;

// <summary>
/// Запрос на обновление задачи
//</summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// Название задачи
    /// </summary>
    /// <example>Фикс бага</example>
    public required string Title { get; set; }

    /// <summary>
    /// Описание задачи
    /// </summary>
    /// <example>Токен полетел</example>
    public required string Description { get; set; }

    /// <summary>
    /// Награда за выполнение задачи (в баллах)
    /// </summary>
    /// <example>50</example>
    public int Reward { get; set; }

    /// <summary>
    /// Статус задачи
    /// </summary>
    /// <example>New</example>
    public MarketplaceTaskStatus Status { get; set; }

    /// <summary>
    /// ID пользователя, взявшего задачу (опционально)
    /// </summary>
    /// <example>5</example>
    public int? TakenByUserId { get; set; }
}