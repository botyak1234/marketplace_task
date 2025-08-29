using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.DAL.Models;

namespace TaskMarketplace.Service.Abstractions;

// ITaskService.cs
public interface ITaskService
{
    Task<List<TaskDto>> GetAllAsync(int userId, string? role, CancellationToken cancellationToken = default);
    Task<TaskDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskDto> TakeAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<TaskDto> SubmitAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<TaskDto> ReviewAsync(int id, ReviewStatus statusByAdmin, CancellationToken cancellationToken = default);
    Task<List<TaskDto>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<List<TaskDto>> GetSortedAsync(string? sortBy, string? order, CancellationToken cancellationToken = default);
}


