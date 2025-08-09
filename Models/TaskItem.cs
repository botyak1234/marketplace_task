namespace TaskMarketplace.API.Models;

public enum TaskStatus
{
    New,
    Taken,
    Submitted,
    Approved,
    Rejected
}

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Reward { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.New;

    public int? TakenByUserId { get; set; }
    public User? TakenByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}