using TaskMarketplace.Contracts.Enums;
using TaskMarketplace.DAL.Models;

namespace TaskMarketplace.DAL.Abstractions;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdWithUserAsync(int id, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetAllWithUserAsync(CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetByStatusAsync(MarketplaceTaskStatus status, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetByUserIdOrNotTakenAsync(int userId, CancellationToken cancellationToken = default);
    Task CreateAsync(TaskItem task, CancellationToken cancellationToken = default);
    void Update(TaskItem task);
    void Delete(TaskItem task);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}