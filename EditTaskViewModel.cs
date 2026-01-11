using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.Services;

namespace MultiHubApp.ViewModels
{
    public class EditTaskViewModel : BaseViewModel
    {
        private readonly TaskService _taskService = new TaskService();
        private readonly AuthServiceRest _auth = new AuthServiceRest();

        public TaskItem Task { get; set; }
        public bool IsNew { get; }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditTaskViewModel(TaskItem? task = null)
        {
            if (task == null)
            {
                Task = new TaskItem
                {
                    Id = null,
                    Title = string.Empty,
                    Details = string.Empty,
                    Date = DateTime.Today,
                    Time = DateTime.Now.ToString("HH:mm"),
                    Completed = false,
                    CreatedAt = DateTime.UtcNow
                };
                IsNew = true;
            }
            else
            {
                Task = task;
                IsNew = string.IsNullOrEmpty(task.Id);
            }

            SaveCommand = new Command(async () => await ExecuteSaveAsync());
            DeleteCommand = new Command(async () => await ExecuteDeleteAsync());
        }

        private async Task ExecuteSaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var uid = await _auth.GetUserIdAsync();
                if (string.IsNullOrEmpty(uid))
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Kullan?c? giri?li de?il.", "OK");
                    return;
                }

                Task.UserId = uid;

                if (IsNew)
                {
                    var newId = await _taskService.AddTaskAsync(Task);
                    if (!string.IsNullOrEmpty(newId))
                    {
                        Task.Id = newId;
                    }
                }
                else
                {
                    await _taskService.UpdateTaskAsync(Task);
                }

                // ???? ??? ?????? ???????
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var confirm = await Application.Current.MainPage.DisplayAlert("Sil", "Bu görev silinsin mi?", "Evet", "Hay?r");
                if (!confirm) return;

                var uid = await _auth.GetUserIdAsync();
                if (string.IsNullOrEmpty(uid))
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Kullan?c? giri?li de?il.", "OK");
                    return;
                }

                if (!string.IsNullOrEmpty(Task.Id))
                {
                    await _taskService.DeleteTaskAsync(uid, Task.Id);
                }

                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}