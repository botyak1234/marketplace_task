using TaskMarketplace.Contracts.Auth;

namespace TaskMarketplace.Service.Abstractions;

public interface IUserService
{
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request);
    Task<int?> GetPointsAsync(int userId);
}
