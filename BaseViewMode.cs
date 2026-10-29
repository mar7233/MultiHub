// name=ViewModels/BaseViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiHubApp.ViewModels
{
    // Basit BaseViewModel (Türkçe yorum).
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return;
            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}