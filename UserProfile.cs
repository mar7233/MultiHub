namespace MultiHubApp.Models
{
    public class UserProfile
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? StudentNumber { get; set; }
        public string? PhotoPath { get; set; }
        public DateTime CreatedAt { get; set; }

      
        public string? Department { get; set; } = "Bartın Üniversitesi Bilgisayar Mühendisliği Bölümü";
    }
}