

namespace TaskMarketplace.Contracts.Tasks;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Reward { get; set; }
    public Enums.MarketplaceTaskStatus Status { get; set; }
    public int? TakenByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
