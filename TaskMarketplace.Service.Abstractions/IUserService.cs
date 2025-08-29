using TaskMarketplace.Contracts.Auth;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.Service.Abstractions;

public interface IUserService
{
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<string?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<int?> GetPointsAsync(int userId, CancellationToken cancellationToken = default);
}