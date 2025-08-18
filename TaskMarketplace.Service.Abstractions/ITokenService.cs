using TaskMarketplace.DAL.Models;

namespace TaskMarketplace.Service.Abstractions;

public interface ITokenService
{
    string CreateToken(User user);
}
