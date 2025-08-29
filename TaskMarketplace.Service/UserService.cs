using TaskMarketplace.Contracts.Auth;
using TaskMarketplace.DAL.Models;
using TaskMarketplace.DAL.Abstractions;
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

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _userRepository.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (exists) return (false, "Username already taken");

        var user = new User(
            request.Username,
            PasswordHasher.HashPassword(request.HashPassword),
            "User",
            0
        );

        await _userRepository.CreateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        
        return (true, null);
    }

    public async Task<string?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null) return null;

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash)) return null;

        return _tokenService.CreateToken(user);
    }

    public async Task<int?> GetPointsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user?.Points;
    }
}