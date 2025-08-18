using System;
using System.ComponentModel.DataAnnotations.Schema;
using TaskMarketplace.Contracts.Enums;

namespace TaskMarketplace.DAL.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Reward { get; set; }
        public MarketplaceTaskStatus Status { get; set; } = MarketplaceTaskStatus.New;

        public int? TakenByUserId { get; set; }
        public User? TakenByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public TaskItem(string title, string description, int reward)
        {
            Title = title;
            Description = description;
            Reward = reward;
        }
    }
}
