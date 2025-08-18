using Microsoft.EntityFrameworkCore;
using TaskMarketplace.Contracts;
using TaskMarketplace.Contracts.Enums;
using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.DAL;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.Service;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _db;
    public TaskService(ApplicationDbContext db) => _db = db;

    public async Task<List<TaskDto>> GetAllAsync(int userId, string? role)
    {
        IQueryable<TaskItem> query = _db.Tasks.Include(t => t.TakenByUser);

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.TakenByUserId == null || t.TakenByUserId == userId);
        }

        var tasks = await query.ToListAsync();
        return tasks.Select(Map).ToList();
    }

    public async Task<TaskDto?> GetByIdAsync(int id)
    {
        var task = await _db.Tasks.Include(t => t.TakenByUser)
            .FirstOrDefaultAsync(t => t.Id == id);
        return task is null ? null : Map(task);
    }

   public async Task<TaskDto> CreateAsync(CreateTaskRequest request)
    {
        var entity = new TaskItem(request.Title, request.Description, request.Reward);

        _db.Tasks.Add(entity);
        await _db.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest request)
    {
        var entity = await _db.Tasks.FindAsync(id);
        if (entity is null) return false;

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Reward = request.Reward;
        entity.Status = request.Status;
        entity.TakenByUserId = request.TakenByUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Tasks.FindAsync(id);
        if (entity is null) return false;

        _db.Tasks.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TaskDto?> TakeAsync(int id, int userId)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return null;
        if (task.TakenByUserId != null) return null;
        if (task.Status != MarketplaceTaskStatus.New) return null;

        task.TakenByUserId = userId;
        task.Status = MarketplaceTaskStatus.Taken;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(task);
    }

    public async Task<TaskDto?> SubmitAsync(int id, int userId)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return null;
        if (task.TakenByUserId != userId) return null;
        if (task.Status != MarketplaceTaskStatus.New) return null;

        task.Status = MarketplaceTaskStatus.Submitted;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(task);
    }

    public async Task<TaskDto?> ReviewAsync(int id, string statusByAdmin)
    {
        var task = await _db.Tasks.Include(t => t.TakenByUser).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return null;

        if (string.Equals(statusByAdmin, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            task.Status = MarketplaceTaskStatus.Approved;
            if (task.TakenByUserId.HasValue)
            {
                var user = await _db.Users.FindAsync(task.TakenByUserId.Value);
                if (user != null)
                {
                    user.Points += task.Reward;
                }
            }
        }
        else if (string.Equals(statusByAdmin, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            task.Status = MarketplaceTaskStatus.Rejected;
        }
        else
        {
            return null;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(task);
    }

    private static TaskDto Map(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Reward = t.Reward,
        Status = t.Status,
        TakenByUserId = t.TakenByUserId,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
