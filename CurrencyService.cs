using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MultiHubApp.Models;

namespace MultiHubApp.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly CultureInfo tr = new CultureInfo("tr-TR"); // Türkçe sayı formatındaki virgülleri doğru işlemek için.
        private readonly CultureInfo inv = CultureInfo.InvariantCulture;

        public CurrencyService() { }

        public async Task<List<RateItem>> FetchRatesFromTruncgilAsync(string url = "https://finans.truncgil.com/today.json")
        {
            var results = new List<RateItem>();
            string json;

            try
            {
                json = await _http.GetStringAsync(url); // Veriler kaynaktan çekiliyor.
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Failed to fetch rates from truncgil.", ex);
            }

            JObject? root;
            try
            {
                root = JsonConvert.DeserializeObject<JObject>(json); // JSON verisi nesneye dönüştürülüyor.
                if (root == null) return results;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse truncgil JSON.", ex);
            }

            foreach (var prop in root.Properties())
            {
                var key = prop.Name;

                // Döviz verisi içermeyen üst bilgi alanları (Tarih, Güncelleme vb.) filtreleniyor.
                if (string.Equals(key, "Date", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "Update", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "BIST", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "Update_Date", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (prop.Value is JObject obj)
                {
                    // JSON içindeki Türkçe anahtarlar (Alış, Satış, Değişim) kontrol edilerek okunuyor.
                    var buyingStr = GetStringInsensitive(obj, "Alış", "Alis", "BanknoteBuying", "ForexBuying");
                    var sellingStr = GetStringInsensitive(obj, "Satış", "Satis", "BanknoteSelling", "ForexSelling");
                    var changeStr = GetStringInsensitive(obj, "Değişim", "Degisim", "Change", "Change24");

                    if (string.IsNullOrWhiteSpace(buyingStr) && string.IsNullOrWhiteSpace(sellingStr) && string.IsNullOrWhiteSpace(changeStr))
                        continue;

                    var item = new RateItem
                    {
                        Code = key,
                        Name = GetStringInsensitive(obj, "Isim", "Name") ?? key
                    };

                    item.Buying = ParseDoubleClean(buyingStr); // Alış verisi sayıya dönüştürülüyor.
                    item.Selling = ParseDoubleClean(sellingStr); // Satış verisi sayıya dönüştürülüyor.

                    item.Change = ParseDoubleClean(changeStr); // Yüzdelik değişim temizlenerek işleniyor.

                    if (item.Change.HasValue)
                    {
                        // Değişim oranına göre fiyatın yönü belirleniyor.
                        item.Direction = item.Change.Value > 0 ? "up" : item.Change.Value < 0 ? "down" : "none";
                    }

                    if (item.Buying.HasValue || item.Selling.HasValue || item.Change.HasValue)
                    {
                        results.Add(item);
                    }
                }
            }

            // Mükerrer olan döviz kodları temizleniyor.
            var unique = new Dictionary<string, RateItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in results)
            {
                if (string.IsNullOrWhiteSpace(r.Code)) continue;
                if (!unique.ContainsKey(r.Code))
                    unique[r.Code] = r;
            }

            var finalList = new List<RateItem>(unique.Values);

            // Tercih edilen para birimleri (USD, EUR, GBP) listenin en başına getiriliyor.
            var preferred = new[] { "USD", "EUR", "GBP" };
            var ordered = new List<RateItem>();
            foreach (var p in preferred)
            {
                foreach (var r in finalList)
                {
                    if (string.Equals(r.Code, p, StringComparison.OrdinalIgnoreCase))
                        ordered.Add(r);
                }
            }

            foreach (var r in finalList)
            {
                if (!ordered.Contains(r))
                    ordered.Add(r);
            }

            return ordered;
        }

        private string? GetStringInsensitive(JObject obj, params string[] names)
        {
            // JSON içindeki anahtar kelimeleri büyük/küçük harf duyarlılığı olmadan arar.
            foreach (var n in names)
            {
                var p = obj.Property(n, StringComparison.OrdinalIgnoreCase);
                if (p != null)
                {
                    var s = p.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
            }
            return null;
        }

        private double? ParseDoubleClean(string? s)
        {
            // Metin formatındaki sayıları temizleyip (nokta/virgül ayrımı yaparak) double formatına çevirir.
            if (string.IsNullOrWhiteSpace(s)) return null;
            var cleaned = s.Trim();

            cleaned = cleaned.Replace("%", "").Trim();

            // Sayı hem nokta hem virgül içeriyorsa (örn: 1.234,56) uygun formata getirilir.
            if (cleaned.Contains('.') && cleaned.Contains(','))
            {
                var step = cleaned.Replace(".", "");
                step = step.Replace(',', '.');
                if (double.TryParse(step, NumberStyles.Any, CultureInfo.InvariantCulture, out var v1))
                    return v1;
            }
            else if (cleaned.Contains(',') && !cleaned.Contains('.'))
            {
                // Sadece virgül içeren değerler ondalık sayı olarak işlenir.
                var step = cleaned.Replace(',', '.');
                if (double.TryParse(step, NumberStyles.Any, CultureInfo.InvariantCulture, out var v2))
                    return v2;

                if (double.TryParse(cleaned, NumberStyles.Any, tr, out var v2b))
                    return v2b;
            }
            else
            {
                if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var v3))
                    return v3;

                if (double.TryParse(cleaned, NumberStyles.Any, tr, out var v3b))
                    return v3b;
            }

            return null;
        }
    }
}