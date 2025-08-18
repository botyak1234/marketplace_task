using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.DAL.Models;

namespace TaskMarketplace.Service.Abstractions;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllAsync(int userId, string? role);
    Task<TaskDto?> GetByIdAsync(int id);
    Task<TaskDto> CreateAsync(CreateTaskRequest request);
    Task<bool> UpdateAsync(int id, UpdateTaskRequest request);
    Task<bool> DeleteAsync(int id);
    Task<TaskDto?> TakeAsync(int id, int userId);
    Task<TaskDto?> SubmitAsync(int id, int userId);
    Task<TaskDto?> ReviewAsync(int id, string statusByAdmin);
}
