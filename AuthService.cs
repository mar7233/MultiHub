using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MultiHubApp.Services
{
    // Bu sınıf Firebase Authentication REST API'sini kullanır.
    // Tüm görünür stringler İngilizce, açıklamalar (yorumlar) Türkçe olacak şekilde düzenlendi.
    public class AuthServiceRest
    {
        private readonly HttpClient _http = new();
        private const string FirebaseApiKey = "AIzaSyAVKwktWr0fnDMtPVoSLecEnCnK6RnziFQ"; // BURAYA GERÇEK API KEY EKLE

        // Firebase endpoints
        private string SignUpUrl => $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}";
        private string SignInUrl => $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}";
        private string RefreshUrl => $"https://securetoken.googleapis.com/v1/token?key={FirebaseApiKey}";

        // İç kullanım için basit response modeli
        private class AuthResponse
        {
            public string idToken { get; set; }
            public string refreshToken { get; set; }
            public string localId { get; set; }
            public string expiresIn { get; set; }
            public string email { get; set; }
        }

        // Kullanıcı kaydı (register)
        // Dönen tuple: Success ve Message
        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password)
        {
            // Basit input doğrulama
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email and password are required.");

            var payload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var res = await _http.PostAsync(SignUpUrl, content);
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                var msg = TryParseErrorMessage(body);
                return (false, $"Register failed: {msg}");
            }

            var auth = JsonSerializer.Deserialize<AuthResponse>(body);
            await SaveTokensAsync(auth);
            return (true, "Registered successfully");
        }

        // Kullanıcı girişi (login)
        public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email and password are required.");

            var payload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var res = await _http.PostAsync(SignInUrl, content);
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                var msg = TryParseErrorMessage(body);
                return (false, $"Login failed: {msg}");
            }

            var auth = JsonSerializer.Deserialize<AuthResponse>(body);
            await SaveTokensAsync(auth);
            return (true, "Logged in successfully");
        }

        // Çıkış (logout) — güvenli depolamadan tokenları temizler
        public async Task LogoutAsync()
        {
            try
            {
                SecureStorage.Remove("fb_id_token");
                SecureStorage.Remove("fb_refresh_token");
                SecureStorage.Remove("fb_user_id");
            }
            catch
            {
                // SecureStorage hatası olsa bile uygulama çökmesin diye yutuyoruz
            }
            await Task.CompletedTask;
        }

        // Geçerli idToken al. Yoksa refreshToken ile yenilemeye çalış.
        public async Task<string> GetIdTokenAsync()
        {
            var token = await SecureStorage.GetAsync("fb_id_token");
            if (!string.IsNullOrEmpty(token))
                return token;

            var refresh = await SecureStorage.GetAsync("fb_refresh_token");
            if (string.IsNullOrEmpty(refresh))
                return null;

            var form = new StringContent($"grant_type=refresh_token&refresh_token={refresh}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var res = await _http.PostAsync(RefreshUrl, form);
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return null;

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("access_token", out var accessTokenEl) &&
                doc.RootElement.TryGetProperty("refresh_token", out var refreshTokenEl) &&
                doc.RootElement.TryGetProperty("user_id", out var userIdEl))
            {
                var newToken = accessTokenEl.GetString();
                var newRefresh = refreshTokenEl.GetString();
                var uid = userIdEl.GetString();

                await SecureStorage.SetAsync("fb_id_token", newToken ?? string.Empty);
                await SecureStorage.SetAsync("fb_refresh_token", newRefresh ?? string.Empty);
                if (!string.IsNullOrEmpty(uid))
                    await SecureStorage.SetAsync("fb_user_id", uid);

                return newToken;
            }

            return null;
        }

        // Kullanıcı id'sini al
        public async Task<string> GetUserIdAsync()
        {
            return await SecureStorage.GetAsync("fb_user_id");
        }

        // Token ve uid kaydetme yardımcı metodu
        private static async Task SaveTokensAsync(AuthResponse auth)
        {
            if (auth == null) return;
            try
            {
                await SecureStorage.SetAsync("fb_id_token", auth.idToken ?? string.Empty);
                await SecureStorage.SetAsync("fb_refresh_token", auth.refreshToken ?? string.Empty);
                await SecureStorage.SetAsync("fb_user_id", auth.localId ?? string.Empty);
            }
            catch
            {
                // SecureStorage yazma hatası: loglama eklenebilir
            }
        }

        // Firebase hata mesajını parse eder, okunabilir hale getirir
        private static string TryParseErrorMessage(string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("error", out var err) &&
                    err.TryGetProperty("message", out var msg))
                {
                    return msg.GetString() ?? responseBody;
                }
            }
            catch
            {
                // Parse hatası olursa orijinal body'yi döndür
            }
            return responseBody;
        }
    }
}