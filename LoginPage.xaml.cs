using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MultiHubApp.Services;

namespace MultiHubApp.Views
{
    public partial class LoginPage : ContentPage
    {
        // Kimlik doğrulama işlemleri için REST servisi başlatılıyor.
        private readonly AuthServiceRest _authService = new AuthServiceRest();

        public LoginPage()
        {
            InitializeComponent();
        }

        // E-posta alanı doldurulup 'Enter'a basıldığında şifre alanına odaklanır.
        private void OnEmailCompleted(object sender, EventArgs e)
        {
            PasswordEntry.Focus();
        }

        // Şifre alanı doldurulup 'Enter'a basıldığında giriş butonunu tetikler.
        private void OnPasswordCompleted(object sender, EventArgs e)
        {
            OnLoginClicked(this, EventArgs.Empty);
        }

        // Giriş butonuna tıklandığında çalışan ana metod.
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            SetLoading(true); // Yükleme göstergesini başlat.

            try
            {
                var email = EmailEntry.Text?.Trim();
                var password = PasswordEntry.Text;

                // Boş alan kontrolü (Validasyon).
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    await DisplayAlert("Doğrulama", "Lütfen e-posta ve şifre giriniz.", "Tamam");
                    return;
                }

                // API servisi üzerinden giriş yapmayı dene.
                var (success, message) = await _authService.LoginAsync(email, password);
                if (!success)
                {
                    await DisplayAlert("Giriş Başarısız", message, "Tamam");
                    return;
                }

                // Başarılı giriş bildirimi.
                await DisplayAlert("Başarılı", "Giriş yapıldı.", "Tamam");

                // Giriş durumunu cihazın güvenli hafızasına (SecureStorage) kaydet.
                try
                {
                    await SecureStorage.SetAsync("is_logged_in", "1");
                }
                catch
                {
                    // SecureStorage hatası durumunda işlemi durdurma.
                }

                // Navigasyon kontrolü: Shell üzerinden ana sayfaya yönlendirme.
                if (Shell.Current == null)
                {
                    // Eğer Shell yapısı yoksa yedek yöntemle (MainPage üzerinden) yönlendir.
                    Application.Current.MainPage = new NavigationPage(new HomePage());
                    return;
                }

                try
                {
                    // Shell rotası kullanarak ana sayfaya (Home) git.
                    await Shell.Current.GoToAsync("//main/home");
                }
                catch (Exception navEx)
                {
                    await DisplayAlert("Navigasyon Hatası", navEx.Message, "Tamam");
                    // Hata durumunda manuel olarak ana sayfayı ata.
                    Application.Current.MainPage = new NavigationPage(new HomePage());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
            finally
            {
                SetLoading(false); // İşlem bitince yükleme göstergesini kapat.
            }
        }

        // Kayıt ol sayfasına yönlendiren metod.
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {
                // Shell rotası üzerinden kayıt sayfasına git.
                await Shell.Current.GoToAsync("//register");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigasyon Hatası", ex.Message, "Tamam");
            }
        }

        // İşlem sırasında butonları ve giriş alanlarını kilitleyen yardımcı metod.
        private void SetLoading(bool isLoading)
        {
            LoadingIndicator.IsRunning = isLoading;
            LoadingIndicator.IsVisible = isLoading;
            LoginButton.IsEnabled = !isLoading;
            RegisterButton.IsEnabled = !isLoading;
            EmailEntry.IsEnabled = !isLoading;
            PasswordEntry.IsEnabled = !isLoading;
        }
    }
}