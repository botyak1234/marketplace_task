using TaskMarketplace.Contracts.Enums;
using TaskMarketplace.DAL.Models;

namespace TaskMarketplace.DAL.Repositories;

public interface ITaskRepository
{
    IQueryable<TaskItem> GetQueryable();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem?> GetByIdWithUserAsync(int id);
    Task<List<TaskItem>> GetAllWithUserAsync();
    Task<List<TaskItem>> GetByStatusAsync(MarketplaceTaskStatus status);
    Task<List<TaskItem>> GetByUserIdOrNotTakenAsync(int userId);
    Task CreateAsync(TaskItem task);
    void Update(TaskItem task);
    void Delete(TaskItem task);
    Task SaveChangesAsync();
}