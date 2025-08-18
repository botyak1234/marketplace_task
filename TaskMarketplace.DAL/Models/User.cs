namespace TaskMarketplace.DAL.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "User";
    public int Points { get; set; } = 0;

    public ICollection<TaskItem> TakenTasks { get; set; } = new List<TaskItem>();

    public User(string username, string passwordHash, string role = "User", int points = 0)
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        Points = points;
    }
}
