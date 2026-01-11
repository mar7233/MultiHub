using Microsoft.Maui.Controls;

namespace MultiHubApp.Views
{
    public partial class KurlarPage : ContentPage
    {
        public KurlarPage()
        {
            InitializeComponent();
            // BindingContext is set in XAML; if you prefer to set it here instead:
            // BindingContext = new MultiHubApp.ViewModels.KurlarViewModel();
        }
    }
}