using System;

namespace MultiHubApp.Services
{
    // مساعد لتوليد روابط MGM حسب قواعد الموقع (بإزالة الحروف التركية وكتابة بالحروف الكبيرة)
    public static class HavaDurumuServisi
    {
        // Normalize according to MGM rules: uppercase, replace Turkish letters with ASCII equivalents.
        public static string NormalizeCityName(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName)) return string.Empty;

            // Trim & uppercase
            cityName = cityName.Trim().ToUpperInvariant();

            // Replace Turkish characters with ASCII equivalents
            cityName = cityName.Replace("Ç", "C")
                               .Replace("Ö", "O")
                               .Replace("Ş", "S")
                               .Replace("İ", "I")
                               .Replace("I", "I") // keep I
                               .Replace("Ü", "U")
                               .Replace("Ğ", "G")
                               .Replace("ç", "C")
                               .Replace("ö", "O")
                               .Replace("ş", "S")
                               .Replace("ı", "I")
                               .Replace("i", "I")
                               .Replace("ü", "U")
                               .Replace("ğ", "G");

            // Special names according to MGM doc
            if (cityName == "KAHRAMANMARAS" || cityName == "KAHRAMAN-MARAŞ" || cityName == "KAHRAMAN MARAS")
                return "K.MARAS";

            if (cityName == "AFYON")
                return "AFYONKARAHISAR";

            // Some MGM names expect full names without spaces; keep as upper ASCII string
            return cityName.Replace(" ", "");
        }

        // URL for current conditions (sondurum)
        public static string HavaDurumuBugunUrl(string normalizedCity)
        {
            // Use HTTPS to avoid insecure content issues
            return $"https://www.mgm.gov.tr/sunum/sondurum-show-2.aspx?m={Uri.EscapeDataString(normalizedCity)}&rC=111&rZ=fff";
        }

        // URL for 5 day forecast
        public static string HavaDurumu5GunUrl(string normalizedCity)
        {
            return $"https://www.mgm.gov.tr/sunum/tahmin-show-2.aspx?m={Uri.EscapeDataString(normalizedCity)}&basla=1&bitir=5&rC=111&rZ=fff";
        }
    }
}