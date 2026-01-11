using System;

namespace MultiHubApp.Models
{
    // Döviz / Altın için model.
    // Property isimleri İngilizce, yorumlar Türkçe.
    public class CurrencyRate
    {
        public string Code { get; set; } = string.Empty; // e.g. "USD", "EUR", "GRAM_ALTIN"
        public double? Buying { get; set; } // Alış
        public double? Selling { get; set; } // Satış
        public double? Change { get; set; } // Değişim (varsa)
        public string? LastUpdated { get; set; } // Tarih/saat bilgisi string olarak
    }
}