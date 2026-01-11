using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MultiHubApp
{
    public partial class AppShell : Shell
    {
        bool _initialized = false;

        public AppShell()
        {
            InitializeComponent();

            // Sayfalar arası geçiş için ek rotalar (Routes) kaydediliyor.
            Routing.RegisterRoute(nameof(MultiHubApp.Views.RegisterPage), typeof(MultiHubApp.Views.RegisterPage));
            Routing.RegisterRoute(nameof(MultiHubApp.Views.HaberlerPage), typeof(MultiHubApp.Views.HaberlerPage));
            Routing.RegisterRoute(nameof(MultiHubApp.Views.HomePage), typeof(MultiHubApp.Views.HomePage));
        }

        // Shell ilk göründüğünde çalışacak olan metod.
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Kontrol işleminin uygulama açılışında sadece bir kez çalışmasını sağlar.
            if (_initialized) return;
            _initialized = true;

            this.Dispatcher.Dispatch(async () =>
            {
                try
                {
                    // Güvenli hafızadan (SecureStorage) kullanıcının giriş yapıp yapmadığı kontrol edilir.
                    var logged = await SecureStorage.GetAsync("is_logged_in");

                    if (logged == "1")
                    {
                        // Kullanıcı giriş yapmışsa doğrudan ana sayfaya yönlendirilir.
                        await Shell.Current.GoToAsync("//main/home");
                    }
                    else
                    {
                        // Giriş yapılmamışsa giriş sayfasına (Login) yönlendirilir.
                        await Shell.Current.GoToAsync("//login");
                    }
                }
                catch
                {
                    // Herhangi bir hata durumunda varsayılan olarak giriş sayfasına yönlendirilir.
                    await Shell.Current.GoToAsync("//login");
                }
            });
        }
    }
}