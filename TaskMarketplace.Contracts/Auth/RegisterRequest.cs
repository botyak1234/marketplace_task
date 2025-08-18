namespace TaskMarketplace.Contracts.Auth;

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string HashPassword { get; set; }
}
