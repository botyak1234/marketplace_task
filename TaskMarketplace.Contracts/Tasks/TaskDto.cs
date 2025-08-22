using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.Contracts.Tasks;

/// <summary>
/// Модель данных задачи
/// </summary>
public class TaskDto
{
    /// <summary>
    /// Идентификатор задачи
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Заголовок задачи
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Описание задачи
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Награда за выполнение задачи
    /// </summary>
    public double Reward { get; set; }
    
    /// <summary>
    /// Статус задачи
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя, взявшего задачу
    /// </summary>
    public int? TakenByUserId { get; set; }
    
    /// <summary>
    /// Дата создания задачи
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата последнего обновления задачи
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
