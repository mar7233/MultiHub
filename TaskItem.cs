using System;

namespace MultiHubApp.Models
{
    // Task (Görev) modeli.
    public class TaskItem
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Details { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string? Time { get; set; }
        public bool Completed { get; set; } = false;
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}