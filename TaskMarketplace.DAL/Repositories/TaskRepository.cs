using Microsoft.EntityFrameworkCore;
using TaskMarketplace.Contracts.Enums;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.DAL.Repositories;

namespace TaskMarketplace.DAL.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _db;

    public TaskRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public IQueryable<TaskItem> GetQueryable()
    {
        return _db.Tasks.AsQueryable();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _db.Tasks.FindAsync(id);
    }

    public async Task<TaskItem?> GetByIdWithUserAsync(int id)
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TaskItem>> GetAllWithUserAsync()
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetByStatusAsync(MarketplaceTaskStatus status)
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetByUserIdOrNotTakenAsync(int userId)
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .Where(t => t.TakenByUserId == null || t.TakenByUserId == userId)
            .ToListAsync();
    }

    public async Task CreateAsync(TaskItem task)
    {
        await _db.Tasks.AddAsync(task);
    }

    public void Update(TaskItem task)
    {
        _db.Tasks.Update(task);
    }

    public void Delete(TaskItem task)
    {
        _db.Tasks.Remove(task);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}