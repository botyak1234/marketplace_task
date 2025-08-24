using TaskMarketplace.Contracts.Enums;

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
    public ReviewStatus StatusByAdmin { get; set; }
}
