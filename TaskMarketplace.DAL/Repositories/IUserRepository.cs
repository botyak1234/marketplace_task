using System.Linq.Expressions;
using TaskMarketplace.DAL.Models;

namespace TaskMarketplace.DAL.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> AnyAsync(Expression<Func<User, bool>> predicate);
    Task CreateAsync(User user);
    void Update(User user);
    Task SaveChangesAsync();
}