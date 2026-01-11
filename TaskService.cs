using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MultiHubApp.Models;

namespace MultiHubApp.Services
{
    // Firebase Realtime Database için REST tabanl? Task servisi.
    public class TaskService
    {
        // DB URL: Mariam'?n verdi?i realtime database URL
        private const string BaseUrl = "https://multihubapp-default-rtdb.firebaseio.com";
        private readonly HttpClient _http = new HttpClient();
        private readonly AuthServiceRest _authService = new AuthServiceRest();

        private string AuthQuery(string token) => string.IsNullOrEmpty(token) ? "" : $"?auth={token}";

        // Kullan?c?n?n görevlerini al
        public async Task<List<TaskItem>> GetTasksAsync(string userId)
        {
            var list = new List<TaskItem>();
            try
            {
                var token = await _authService.GetIdTokenAsync();
                var url = $"{BaseUrl}/tasks/{userId}.json{AuthQuery(token)}";
                var res = await _http.GetAsync(url);
                var body = await res.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(body) || body == "null") return list;
                var dict = JsonSerializer.Deserialize<Dictionary<string, TaskItem>>(body);
                if (dict == null) return list;
                foreach (var kv in dict)
                {
                    var item = kv.Value;
                    item.Id = kv.Key;
                    list.Add(item);
                }
            }
            catch
            {
                // Hata yutuluyor; UI taraf?nda kullan?c?ya gösterilecek
            }
            return list;
        }

        // Görev ekle
        public async Task<string?> AddTaskAsync(TaskItem task)
        {
            try
            {
                var token = await _authService.GetIdTokenAsync();
                var url = $"{BaseUrl}/tasks/{task.UserId}.json{AuthQuery(token)}";
                var json = JsonSerializer.Serialize(task);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await _http.PostAsync(url, content);
                var body = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("name", out var nameEl)) return nameEl.GetString();
            }
            catch { }
            return null;
        }

        // Görev güncelle
        public async Task UpdateTaskAsync(TaskItem task)
        {
            try
            {
                var token = await _authService.GetIdTokenAsync();
                var url = $"{BaseUrl}/tasks/{task.UserId}/{task.Id}.json{AuthQuery(token)}";
                var json = JsonSerializer.Serialize(task);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _http.PutAsync(url, content);
            }
            catch { }
        }

        // Görev sil
        public async Task DeleteTaskAsync(string userId, string taskId)
        {
            try
            {
                var token = await _authService.GetIdTokenAsync();
                var url = $"{BaseUrl}/tasks/{userId}/{taskId}.json{AuthQuery(token)}";
                await _http.DeleteAsync(url);
            }
            catch { }
        }
    }
}