using System;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.ViewModels;
using MultiHubApp.Services;

namespace MultiHubApp.Views
{
    public partial class YapilacaklarPage : ContentPage
    {
        private readonly TasksViewModel _vm;
        private readonly TaskService _taskService = new TaskService();

        public YapilacaklarPage()
        {
            InitializeComponent();
            _vm = new TasksViewModel();
            BindingContext = _vm; // ViewModel bağlaması yapılıyor.
        }

        // Sayfa her göründüğünde görev listesini günceller.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadTasksAsync();
        }

        // Yeni görev eklemek için düzenleme sayfasını açar.
        private async void OnAddClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditTaskPage());
        }

        // Seçili görevi düzenlemek için ilgili verilerle sayfayı açar.
        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is TaskItem item)
            {
                await Navigation.PushAsync(new EditTaskPage(item));
            }
        }

        // Bir görevi Firebase üzerinden siler ve listeden kaldırır.
        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is TaskItem item)
            {
                var ok = await DisplayAlert("Sil", "Bu görev silinsin mi?", "Evet", "Hayır");
                if (!ok) return;

                try
                {
                    var auth = new AuthServiceRest();
                    var uid = await auth.GetUserIdAsync(); // Kullanıcı ID'si alınıyor.

                    if (string.IsNullOrEmpty(uid))
                    {
                        await DisplayAlert("Hata", "Kullanıcı girişi yapılmamış.", "Tamam");
                        return;
                    }

                    // Servis üzerinden silme işlemi ve listeden çıkarma.
                    await _taskService.DeleteTaskAsync(uid, item.Id);
                    _vm.Tasks.Remove(item);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hata", ex.Message, "Tamam");
                }
            }
        }

        // Görevin tamamlanma durumunu (Checkbox) Firebase'de günceller.
        private async void OnCompletedCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox cb && cb.BindingContext is TaskItem item)
            {
                try
                {
                    item.Completed = e.Value; // Yeni durum atanıyor.
                    await _taskService.UpdateTaskAsync(item); // Veritabanında güncelleniyor.
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Hata", $"Güncelleme başarısız: {ex.Message}", "Tamam");
                }
            }
        }

        // Listeyi manuel olarak yenilemek için kullanılır.
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await _vm.LoadTasksAsync();
        }
    }
}