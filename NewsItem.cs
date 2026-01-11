using System;

namespace MultiHubApp.Models
{
    // Haber maddesi modeli (güncellendi)
    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime? PublishDate { get; set; } = null;
        public string Link { get; set; } = string.Empty;

        // Yeni alanlar
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
    }
}