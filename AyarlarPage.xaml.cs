using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MultiHubApp.Views
{
    public partial class AyarlarPage : ContentPage
    {
        const string PrefKeyDark = "is_dark_theme";

        public AyarlarPage()
        {
            InitializeComponent();

            // Kayıtlı tema ayarı okunuyor ve Switch konumuna aktarılıyor.
            ThemeSwitch.IsToggled = Preferences.Get(PrefKeyDark, false);
        }

        // Tema anahtarı değiştirildiğinde: Seçimi kaydeder ve tüm uygulamaya uygular.
        private void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            var isDark = e.Value;

            // Kullanıcının tercihi cihaz hafızasına kaydediliyor.
            Preferences.Set(PrefKeyDark, isDark);

            try
            {
                // Uygulama genelindeki SetUserTheme metodu çağrılarak tema değiştiriliyor.
                App.SetUserTheme(isDark);
            }
            catch
            {
                // Hata durumunda alternatif olarak tema doğrudan uygulama üzerinden değiştirilir.
                Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
            }
        }

        // Yerel verileri (userprofile.json, cities.json gibi) silme işlemi.
        private async void OnClearDataClicked(object sender, EventArgs e)
        {
            var ok = await DisplayAlert("Onay", "Yerel veriler (profil, şehirler) silinsin mi?", "Evet", "Hayır");
            if (!ok) return;

            try
            {
                // Kullanıcı profil dosyasının yolu belirleniyor ve varsa siliniyor.
                var profilePath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "userprofile.json");
                if (System.IO.File.Exists(profilePath)) System.IO.File.Delete(profilePath);

                // Şehirler veri dosyasının yolu belirleniyor ve varsa siliniyor.
                var citiesPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "cities.json");
                if (System.IO.File.Exists(citiesPath)) System.IO.File.Delete(citiesPath);

                await DisplayAlert("Başarılı", "Yerel veriler temizlendi.", "Tamam");
            }
            catch (Exception ex)
            {
                // İşlem sırasında hata oluşursa kullanıcıya bildirilir.
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
    }
}