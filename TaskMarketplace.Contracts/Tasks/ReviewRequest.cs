using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.Contracts.Tasks;

/// <summary>
/// Запрос на проверку задачи администратором
/// </summary>
public class ReviewRequest
{
    /// <summary>
    /// Статус, установленный администратором
    /// </summary>
    /// <example>Approved</example>
    [Required(ErrorMessage = "StatusByAdmin is required")]
    [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "StatusByAdmin must be either 'Approved' or 'Rejected'")]
    public required string StatusByAdmin { get; set; }
}