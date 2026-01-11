using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.ViewModels;

namespace MultiHubApp.Views
{
    // EditTask page code-behind: sadece ViewModel bağlama.
    public partial class EditTaskPage : ContentPage
    {
        public EditTaskPage(TaskItem? task = null)
        {
            InitializeComponent();
            BindingContext = new EditTaskViewModel(task);
        }
    }
}