namespace TaskMarketplace.Contracts.Tasks;

public class UpdateTaskRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Reward { get; set; }
    public Enums.MarketplaceTaskStatus Status { get; set; }
    public int? TakenByUserId { get; set; }
}
