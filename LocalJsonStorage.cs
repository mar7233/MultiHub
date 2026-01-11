using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MultiHubApp.Services
{
    
    public class LocalJsonStorage
    {
        private readonly string _filePath;

        public LocalJsonStorage(string fileName = "sehirler.json")
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        }

        public async Task<List<string>> LoadCitiesAsync()
        {
            try
            {
                if (!File.Exists(_filePath)) return new List<string>();
                var json = await File.ReadAllTextAsync(_filePath);
                var list = JsonSerializer.Deserialize<List<string>>(json);
                return list ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task SaveCitiesAsync(List<string> cities)
        {
            try
            {
                var json = JsonSerializer.Serialize(cities);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch
            {
                // ignore save errors for now
            }
        }
    }
}