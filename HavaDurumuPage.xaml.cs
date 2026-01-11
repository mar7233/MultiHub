using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.Services;

namespace MultiHubApp.Views
{
    public partial class HavaDurumuPage : ContentPage
    {
        // Şehir listesini tutan ve arayüzle senkronize çalışan koleksiyon (ObservableCollection).
        public ObservableCollection<Sehir> Sehirler { get; set; } = new ObservableCollection<Sehir>();

        private readonly LocalJsonStorage _localStorage;

        public HavaDurumuPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Verileri yerel dosyaya kaydetmek için kullanılan servis başlatılıyor.
            _localStorage = new LocalJsonStorage();

            _ = LoadSehirlerAsync();
        }

        // Uygulama açıldığında kayıtlı şehir listesini yükleyen metod.
        private async Task LoadSehirlerAsync()
        {
            try
            {
                var list = await _localStorage.LoadCitiesAsync();
                Sehirler.Clear();

                // Eğer kayıtlı şehir yoksa, varsayılan şehir listesi atanır.
                if (list == null || list.Count == 0)
                {
                    list = new System.Collections.Generic.List<string> { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya" };
                }

                foreach (var name in list)
                {
                    Sehirler.Add(new Sehir { SehirAdi = name });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Şehirler yüklenemedi: {ex.Message}", "Tamam");
            }
        }

        // Yeni bir şehir eklemek için kullanılan buton olayı.
        private async void OnAddCityClicked(object sender, EventArgs e)
        {
            var text = await DisplayPromptAsync("Şehir Ekle", "Şehir adını giriniz (Örn: Bartın):", "Ekle", "İptal", placeholder: "Bartın");
            if (string.IsNullOrWhiteSpace(text)) return;

            var name = text.Trim();

            // Şehrin listede zaten olup olmadığı kontrol edilir.
            if (Sehirler.Any(s => s.SehirAdi.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Bilgi", "Bu şehir zaten listede mevcut.", "Tamam");
                return;
            }

            var sObj = new Sehir { SehirAdi = name };
            Sehirler.Add(sObj);

            // Yeni listeyi yerel hafızaya kaydeder.
            await SaveSehirlerAsync();
        }

        // Listeden bir şehri silmek için kullanılan metod (❌ butonu).
        private async void OnRemoveCityClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is Sehir sehir)
            {
                var ok = await DisplayAlert("Onay", $"{sehir.SehirAdi} silinsin mi?", "Evet", "Hayır");
                if (!ok) return;

                Sehirler.Remove(sehir);
                await SaveSehirlerAsync();
            }
        }

        // Tüm şehirlerin hava durumu verilerini (WebView) yeniler.
        private async void OnRefreshAllClicked(object sender, EventArgs e)
        {
            await RefreshAllAsync();
        }

        // Belirli bir şehrin verilerini yenilemek için nesneyi yeniden oluşturur (🔄 butonu).
        private async void OnRefreshCityClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is Sehir sehir)
            {
                var idx = Sehirler.IndexOf(sehir);
                if (idx >= 0)
                {
                    // Nesne yenilenerek WebView'ın URL'i tekrar yüklemesi sağlanır.
                    Sehirler[idx] = new Sehir { SehirAdi = sehir.SehirAdi };
                }
                await Task.CompletedTask;
            }
        }

        // Tüm listeyi döngüye alarak her bir şehri yenileyen yardımcı metod.
        private async Task RefreshAllAsync()
        {
            for (int i = 0; i < Sehirler.Count; i++)
            {
                var s = Sehirler[i];
                Sehirler[i] = new Sehir { SehirAdi = s.SehirAdi };
            }
            await Task.CompletedTask;
        }

        // Güncel şehir isim listesini yerel JSON dosyasına kaydeder.
        private async Task SaveSehirlerAsync()
        {
            try
            {
                var names = Sehirler.Select(s => s.SehirAdi).ToList();
                await _localStorage.SaveCitiesAsync(names);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Kaydedilemedi: {ex.Message}", "Tamam");
            }
        }

        // Sayfa her göründüğünde listeyi günceller.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadSehirlerAsync();
        }
    }
}