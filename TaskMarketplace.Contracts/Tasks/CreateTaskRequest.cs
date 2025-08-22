using System.ComponentModel.DataAnnotations;
using TaskMarketplace.Contracts.Enums;


namespace TaskMarketplace.Contracts.Tasks;

// <summary>
/// Запрос на создание новой задачи
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// Название задачи
    /// </summary>
    /// <example>Implement user profile page</example>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
    public required string Title { get; set; }

    /// <summary>
    /// Описание задачи
    /// </summary>
    /// <example>Create a user profile page with avatar upload functionality</example>
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public required string Description { get; set; }

    /// <summary>
    /// Награда за выполнение задачи (в баллах)
    /// </summary>
    /// <example>100</example>
    [Range(1, 1000, ErrorMessage = "Reward must be between 1 and 1000")]
    public int Reward { get; set; }
}
