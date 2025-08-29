using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.DAL.Models;

namespace TaskMarketplace.Service.Mappings;

public static class TaskMapping
{
    public static TaskDto Map(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Reward = task.Reward,
            Status = task.Status.ToString(),
            TakenByUserId = task.TakenByUserId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}