using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using MultiHubApp.Models;

namespace MultiHubApp.Services
{
    // Kullan?c? profilini yerel olarak JSON format?nda kaydeden/okuyan servis.
    // Foto?raf dosyalar? FileSystem.AppDataDirectory içine kaydedilir.
    public class LocalProfileService
    {
        private string ProfileFilePath => Path.Combine(FileSystem.AppDataDirectory, "userprofile.json");

        // Profili kaydet
        public async Task SaveProfileAsync(UserProfile profile)
        {
            try
            {
                var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(ProfileFilePath, json);
            }
            catch
            {
                // Hata durumunda uygulaman?n çökmesini önlemek için yutuyoruz.
                // Gerekirse logging ekleyebilirsin.
            }
        }

        // Profili yükle (yoksa null döner)
        public async Task<UserProfile> LoadProfileAsync()
        {
            try
            {
                if (!File.Exists(ProfileFilePath)) return null;
                var json = await File.ReadAllTextAsync(ProfileFilePath);
                return JsonSerializer.Deserialize<UserProfile>(json);
            }
            catch
            {
                return null;
            }
        }

        // Foto?raf? uygulama dizinine kaydet ve dönülen yolu ver
        public async Task<string> SavePhotoAsync(string fileName, System.IO.Stream sourceStream)
        {
            try
            {
                var safeName = SanitizeFileName(fileName);
                var destPath = Path.Combine(FileSystem.AppDataDirectory, safeName);

                // E?er ayn? isimde dosya varsa üzerine yaz
                using var dest = File.OpenWrite(destPath);
                await sourceStream.CopyToAsync(dest);
                await dest.FlushAsync();
                return destPath;
            }
            catch
            {
                return null;
            }
        }

        // Basit dosya ad? temizleyici
        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}