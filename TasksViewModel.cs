using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.Services;

namespace MultiHubApp.ViewModels
{
    // Yapilacaklar listesi için ViewModel.
    public class TasksViewModel : BaseViewModel
    {
        private readonly TaskService _taskService = new TaskService();
        private readonly AuthServiceRest _auth = new AuthServiceRest();

        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public TasksViewModel()
        {
            RefreshCommand = new Command(async () => await LoadTasksAsync());
            AddCommand = new Command(async () => await ExecuteAddAsync());
            EditCommand = new Command<TaskItem>(async (t) => await ExecuteEditAsync(t));
            DeleteCommand = new Command<TaskItem>(async (t) => await ExecuteDeleteAsync(t));

            _ = LoadTasksAsync();
        }

        public async Task LoadTasksAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Tasks.Clear();
                var uid = await _auth.GetUserIdAsync();
                if (string.IsNullOrEmpty(uid))
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "No user logged in.", "OK");
                    return;
                }
                var list = await _taskService.GetTasksAsync(uid);
                foreach (var t in list) Tasks.Add(t);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load tasks: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteAddAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new Views.EditTaskPage());
        }

        private async Task ExecuteEditAsync(TaskItem task)
        {
            if (task == null) return;
            await Application.Current.MainPage.Navigation.PushAsync(new Views.EditTaskPage(task));
        }

        private async Task ExecuteDeleteAsync(TaskItem task)
        {
            if (task == null) return;
            var confirm = await Application.Current.MainPage.DisplayAlert("Confirm", "Delete this task?", "Yes", "No");
            if (!confirm) return;

            try
            {
                var uid = await _auth.GetUserIdAsync();
                if (string.IsNullOrEmpty(uid)) { await Application.Current.MainPage.DisplayAlert("Error", "User not logged in.", "OK"); return; }
                await _taskService.DeleteTaskAsync(uid, task.Id);
                Tasks.Remove(task);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}