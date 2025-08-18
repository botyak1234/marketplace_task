using Microsoft.EntityFrameworkCore;
using TaskMarketplace.Contracts.Auth;
using TaskMarketplace.DAL;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.Service.Abstractions;
using TaskMarketplace.Tools;

namespace TaskMarketplace.Service;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public UserService(ApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequest request)
    {
        var exists = await _db.Users.AnyAsync(u => u.Username == request.Username);
        if (exists) return (false, "Username already taken");

        var user = new User(
            request.Username,
            PasswordHasher.HashPassword(request.HashPassword),
            "User",
            0
        );

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null) return null;

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash)) return null;

        return _tokenService.CreateToken(user);
    }

    public async Task<int?> GetPointsAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user?.Points;
    }
}
