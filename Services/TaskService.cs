using Microsoft.EntityFrameworkCore;
using TaskMarketplace.API.Data;
using TaskMarketplace.API.Models;

namespace TaskMarketplace.API.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks.Include(t => t.TakenByUser).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.Tasks.Include(t => t.TakenByUser).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> UpdateAsync(int id, TaskItem task)
    {
        var existing = await _context.Tasks.FindAsync(id);
        if (existing == null) return false;

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Reward = task.Reward;
        existing.Status = task.Status;
        existing.TakenByUserId = task.TakenByUserId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}
