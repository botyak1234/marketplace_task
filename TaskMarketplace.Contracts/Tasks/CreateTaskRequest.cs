namespace TaskMarketplace.Contracts.Tasks;

public class CreateTaskRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Reward { get; set; }
}
