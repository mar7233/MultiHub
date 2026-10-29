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
                SecureStorage.Remove("fb_token_expiry");
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
            // ID tokens expire after about an hour, so only reuse one that is still valid.
            var token = await SecureStorage.GetAsync("fb_id_token");
            var expiryText = await SecureStorage.GetAsync("fb_token_expiry");
            if (!string.IsNullOrEmpty(token) &&
                long.TryParse(expiryText, out var expiryUnix) &&
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() < expiryUnix)
            {
                return token;
            }

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

                var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var expEl) ? expEl.GetString() : null;
                await SaveExpiryAsync(expiresIn);

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
                await SaveExpiryAsync(auth.expiresIn);
            }
            catch
            {
                // SecureStorage yazma hatası: loglama eklenebilir
            }
        }

        // Stores when the current ID token stops being valid (one minute early, to be safe).
        private static async Task SaveExpiryAsync(string? expiresIn)
        {
            var seconds = long.TryParse(expiresIn, out var s) ? s : 3600;
            var expiry = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + seconds - 60;
            await SecureStorage.SetAsync("fb_token_expiry", expiry.ToString());
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