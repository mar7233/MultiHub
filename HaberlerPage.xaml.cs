using System;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.ViewModels;

namespace MultiHubApp.Views
{
    public partial class HaberlerPage : ContentPage
    {
        private HaberlerViewModel? _vm;
        private Button? _selectedCategoryButton;

        public HaberlerPage()
        {
            InitializeComponent();

            // BindingContext kontrolü: Eğer XAML tarafında atanmadıysa yeni bir ViewModel oluşturulur.
            if (BindingContext == null)
            {
                BindingContext = new HaberlerViewModel();
            }

            _vm = BindingContext as HaberlerViewModel;

            // Varsayılan olarak "Hepsi" (BtnAll) butonunu seçili hale getirir.
            if (BtnAll != null)
            {
                SetSelectedCategoryButton(BtnAll);
            }
        }

        // Bir kategori butonuna tıklandığında ilgili haberleri yükler.
        private async void OnCategoryClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button b && b.CommandParameter is string url)
                {
                    // Tıklanan butonu görsel olarak seçili yapar.
                    SetSelectedCategoryButton(b);

                    if (_vm != null)
                    {
                        // ViewModel üzerinden ilgili kategorideki haberleri yükler.
                        await _vm.LoadCategoryAsync(url);
                    }
                    else
                    {
                        // ViewModel boşsa geçici bir tane oluşturup verileri çeker.
                        var temp = new HaberlerViewModel();
                        await temp.LoadCategoryAsync(url);
                        BindingContext = temp;
                        _vm = temp;
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya uyarı mesajı gösterilir.
                await DisplayAlert("Hata", "Kategori yüklenemedi: " + ex.Message, "Tamam");
            }
        }

        // "Hepsi" butonuna tıklandığında tüm haber akışını yükler.
        private async void OnAllClicked(object sender, EventArgs e)
        {
            try
            {
                SetSelectedCategoryButton(BtnAll);
                if (_vm != null)
                {
                    await _vm.LoadAllFeedsAsync();
                }
                else
                {
                    var temp = new HaberlerViewModel();
                    await temp.LoadAllFeedsAsync();
                    BindingContext = temp;
                    _vm = temp;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Haberler yüklenemedi: " + ex.Message, "Tamam");
            }
        }

        // Bir haber öğesine tıklandığında detay sayfasına yönlendirir.
        private async void NewsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection.Count == 0) return;

                if (e.CurrentSelection[0] is NewsItem item)
                {
                    // Seçilen haberi detay sayfasına göndererek sayfayı açar.
                    await Navigation.PushAsync(new HaberDetailPage(item));

                    // Liste üzerindeki seçimi temizler (geri dönüldüğünde tekrar tıklanabilsin diye).
                    ((CollectionView)sender).SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Makale açılamadı: " + ex.Message, "Tamam");
            }
        }

        // Seçilen kategori butonunu görsel olarak vurgulayan yardımcı metod.
        private void SetSelectedCategoryButton(Button? btn)
        {
            try
            {
                // Önceki seçili butonun stilini normale döndürür.
                if (_selectedCategoryButton != null && Resources.ContainsKey("CategoryButton"))
                {
                    _selectedCategoryButton.Style = (Style)Resources["CategoryButton"];
                }

                // Yeni seçilen butona "Seçili" (Selected) stilini uygular.
                _selectedCategoryButton = btn;
                if (_selectedCategoryButton != null && Resources.ContainsKey("CategoryButtonSelected"))
                {
                    _selectedCategoryButton.Style = (Style)Resources["CategoryButtonSelected"];
                }
            }
            catch
            {
                // Stil uygulama hatalarını görmezden gelir.
            }
        }
    }
}