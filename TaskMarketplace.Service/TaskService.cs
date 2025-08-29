using TaskMarketplace.Contracts.Enums;
using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.DAL.Abstractions;
using TaskMarketplace.Service.Abstractions;
using TaskMarketplace.Service.Mappings;

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

    public async Task<List<TaskDto>> GetAllAsync(int userId, string? role, CancellationToken cancellationToken = default)
    {
        List<TaskItem> tasks;
        
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            tasks = await _taskRepository.GetAllWithUserAsync(cancellationToken);
        }
        else
        {
            tasks = await _taskRepository.GetByUserIdOrNotTakenAsync(userId, cancellationToken);
        }

        return tasks.Select(TaskMapping.Map).ToList();
    }

    public async Task<TaskDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
        var task = await _taskRepository.GetByIdWithUserAsync(id, cancellationToken);
        if (task == null) throw new Exception("Задача не найдена");
        return TaskMapping.Map(task);
}

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem(request.Title, request.Description, request.Reward);

        await _taskRepository.CreateAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return TaskMapping.Map(task);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null) return false;

        task.Title = request.Title;
        task.Description = request.Description;
        task.Reward = request.Reward;
        task.Status = request.Status;
        task.TakenByUserId = request.TakenByUserId;
        task.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null) return false;

        _taskRepository.Delete(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TaskDto> TakeAsync(int id, int userId, CancellationToken cancellationToken = default)
{
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task == null) throw new Exception("Задача не найдена");
        
        if (task.TakenByUserId != null) throw new Exception("Задача уже взята");
        if (task.Status != MarketplaceTaskStatus.New) throw new Exception("Задача не доступна");

        task.TakenByUserId = userId;
        task.Status = MarketplaceTaskStatus.Taken;
        task.UpdatedAt = DateTime.UtcNow;
        
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        
        var updatedTask = await _taskRepository.GetByIdWithUserAsync(id, cancellationToken);
        if (updatedTask == null) throw new Exception("Задача не найдена");
        return TaskMapping.Map(updatedTask);
}

    public async Task<TaskDto> SubmitAsync(int id, int userId, CancellationToken cancellationToken = default)
{
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task == null) throw new Exception("Задача не найдена");
        
        if (task.TakenByUserId != userId) throw new Exception("Задача вам не принадлежит");
        if (task.Status != MarketplaceTaskStatus.Taken) throw new Exception("Задача не взята");

        task.Status = MarketplaceTaskStatus.Submitted;
        task.UpdatedAt = DateTime.UtcNow;
        
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        
        var updatedTask = await _taskRepository.GetByIdWithUserAsync(id, cancellationToken);
        if (updatedTask == null) throw new Exception("Задача не найдена");
        return TaskMapping.Map(updatedTask);
}

    public async Task<TaskDto> ReviewAsync(int id, ReviewStatus statusByAdmin, CancellationToken cancellationToken = default)
{
        var task = await _taskRepository.GetByIdWithUserAsync(id, cancellationToken);
        if (task == null) throw new Exception("Задача не найдена");

        switch (statusByAdmin)
        {
            case ReviewStatus.Approved:
                task.Status = MarketplaceTaskStatus.Approved;
                if (task.TakenByUserId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(task.TakenByUserId.Value, cancellationToken);
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
                throw new Exception("Неверный статус оценки");
        }

        task.UpdatedAt = DateTime.UtcNow;
        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        
        return TaskMapping.Map(task);
}
    public async Task<List<TaskDto>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TaskStatus>(status, true, out var parsedStatus))
            throw new Exception("Неправильный статус");

        var marketplaceStatus = (MarketplaceTaskStatus)parsedStatus;
        var tasks = await _taskRepository.GetByStatusAsync(marketplaceStatus, cancellationToken);
        
        return tasks.Select(TaskMapping.Map).ToList();
    }

    public async Task<List<TaskDto>> GetSortedAsync(string? sortBy, string? order, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllWithUserAsync(cancellationToken);
        bool desc = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);

        var sortedTasks = sortBy?.ToLower() switch
        {
            "created" => desc ? tasks.OrderByDescending(t => t.CreatedAt) : tasks.OrderBy(t => t.CreatedAt),
            "updated" => desc ? tasks.OrderByDescending(t => t.UpdatedAt) : tasks.OrderBy(t => t.UpdatedAt),
            _ => tasks.OrderBy(t => t.Id)
        };

        return sortedTasks.Select(TaskMapping.Map).ToList();
    }
}