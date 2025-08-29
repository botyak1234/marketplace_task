using Microsoft.EntityFrameworkCore;
using TaskMarketplace.Contracts.Enums;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.DAL.Abstractions;

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

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tasks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<TaskItem?> GetByIdWithUserAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<TaskItem>> GetAllWithUserAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TaskItem>> GetByStatusAsync(MarketplaceTaskStatus status, CancellationToken cancellationToken = default)
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .Where(t => t.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TaskItem>> GetByUserIdOrNotTakenAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _db.Tasks
            .Include(t => t.TakenByUser)
            .Where(t => t.TakenByUserId == null || t.TakenByUserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _db.Tasks.AddAsync(task, cancellationToken);
    }

    public void Update(TaskItem task)
    {
        _db.Tasks.Update(task);
    }

    public void Delete(TaskItem task)
    {
        _db.Tasks.Remove(task);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}