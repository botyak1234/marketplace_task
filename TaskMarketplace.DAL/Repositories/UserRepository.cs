using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskMarketplace.DAL;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> AnyAsync(Expression<Func<User, bool>> predicate)
    {
        return await _db.Users.AnyAsync(predicate);
    }

    public async Task CreateAsync(User user)
    {
        await _db.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _db.Users.Update(user);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}