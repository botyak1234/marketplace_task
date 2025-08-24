using TaskMarketplace.Contracts;
using TaskMarketplace.Contracts.Enums;
using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.DAL.Repositories;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.Service;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<List<TaskDto>> GetAllAsync(int userId, string? role)
    {
        List<TaskItem> tasks;
        
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            tasks = await _taskRepository.GetAllWithUserAsync();
        }
        else
        {
            tasks = await _taskRepository.GetByUserIdOrNotTakenAsync(userId);
        }

        return tasks.Select(Map).ToList();
    }

    public async Task<TaskDto?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdWithUserAsync(id);
        return task is null ? null : Map(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request)
    {
        var entity = new TaskItem(request.Title, request.Description, request.Reward);

        await _taskRepository.CreateAsync(entity);
        await _taskRepository.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest request)
    {
        var entity = await _taskRepository.GetByIdAsync(id);
        if (entity is null) return false;

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Reward = request.Reward;
        entity.Status = request.Status;
        entity.TakenByUserId = request.TakenByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(entity);
        await _taskRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _taskRepository.GetByIdAsync(id);
        if (entity is null) return false;

        _taskRepository.Delete(entity);
        await _taskRepository.SaveChangesAsync();
        return true;
    }

    public async Task<TaskDto?> TakeAsync(int id, int userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;
        if (task.TakenByUserId != null) return null;
        if (task.Status != MarketplaceTaskStatus.New) return null;

        task.TakenByUserId = userId;
        task.Status = MarketplaceTaskStatus.Taken;
        task.UpdatedAt = DateTime.UtcNow;
        
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();
        
        // Получаем обновленную задачу с информацией о пользователе
        var updatedTask = await _taskRepository.GetByIdWithUserAsync(id);
        return updatedTask is null ? null : Map(updatedTask);
    }

    public async Task<TaskDto?> SubmitAsync(int id, int userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;
        if (task.TakenByUserId != userId) return null;
        if (task.Status != MarketplaceTaskStatus.Taken) return null;

        task.Status = MarketplaceTaskStatus.Submitted;
        task.UpdatedAt = DateTime.UtcNow;
        
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();
        
        var updatedTask = await _taskRepository.GetByIdWithUserAsync(id);
        return updatedTask is null ? null : Map(updatedTask);
    }

    public async Task<TaskDto?> ReviewAsync(int id, ReviewStatus statusByAdmin)
    {
        var task = await _taskRepository.GetByIdWithUserAsync(id);
        if (task is null) return null;

        switch (statusByAdmin)
        {
            case ReviewStatus.Approved:
                task.Status = MarketplaceTaskStatus.Approved;
                if (task.TakenByUserId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(task.TakenByUserId.Value);
                    if (user != null)
                    {
                        user.Points += task.Reward;
                        _userRepository.Update(user);
                    }
                }
                break;
            case ReviewStatus.Rejected:
                task.Status = MarketplaceTaskStatus.Rejected;
                break;
            default:
                return null;
        }

        task.UpdatedAt = DateTime.UtcNow;
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();
        
        return Map(task);
    }

    public async Task<List<TaskDto>?> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<MarketplaceTaskStatus>(status, true, out var parsedStatus))
            return null;

        var tasks = await _taskRepository.GetByStatusAsync(parsedStatus);
        
        return tasks.Select(t => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Reward = t.Reward,
            Status = t.Status.ToString(),
            TakenByUserId = t.TakenByUserId,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();
    }

    public async Task<List<TaskDto>> GetSortedAsync(string? sortBy, string? order)
    {
        var tasks = await _taskRepository.GetAllWithUserAsync();
        bool desc = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);

        var sortedTasks = sortBy?.ToLower() switch
        {
            "created" => desc ? tasks.OrderByDescending(t => t.CreatedAt) : tasks.OrderBy(t => t.CreatedAt),
            "updated" => desc ? tasks.OrderByDescending(t => t.UpdatedAt) : tasks.OrderBy(t => t.UpdatedAt),
            _ => tasks.OrderBy(t => t.Id)
        };

        return sortedTasks.Select(t => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Reward = t.Reward,
            Status = t.Status.ToString(),
            TakenByUserId = t.TakenByUserId,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();
    }

    private static TaskDto Map(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Reward = t.Reward,
        Status = t.Status.ToString(),
        TakenByUserId = t.TakenByUserId,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}