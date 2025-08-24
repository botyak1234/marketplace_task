using TaskMarketplace.Contracts.Auth;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.DAL.Repositories;
using TaskMarketplace.Service.Abstractions;
using TaskMarketplace.Tools;

namespace TaskMarketplace.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequest request)
    {
        var exists = await _userRepository.AnyAsync(u => u.Username == request.Username);
        if (exists) return (false, "Username already taken");

        var user = new User(
            request.Username,
            PasswordHasher.HashPassword(request.HashPassword),
            "User",
            0
        );

        await _userRepository.CreateAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return (true, null);
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null) return null;

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash)) return null;

        return _tokenService.CreateToken(user);
    }

    public async Task<int?> GetPointsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.Points;
    }
}