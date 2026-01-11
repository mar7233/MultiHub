using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.Services;
using System.Collections.Generic;

namespace MultiHubApp.ViewModels
{
    public class KurlarViewModel : BindableObject
    {
        private readonly CurrencyService _service = new CurrencyService();

        public ObservableCollection<RateItem> Rates { get; } = new ObservableCollection<RateItem>();

        bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        public ICommand RefreshCommand { get; }

        public KurlarViewModel()
        {
            RefreshCommand = new Command(async () => await LoadAsync());
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            Rates.Clear();

            try
            {
                var url = "https://finans.truncgil.com/today.json";
                var list = await _service.FetchRatesFromTruncgilAsync(url);

                if (list == null || list.Count == 0)
                {
                    ErrorMessage = "No rates found. Try refresh or check your internet connection.";
                    return;
                }

                // Ensure unique by Code (already done in service, but double-check)
                var unique = list.GroupBy(r => (r.Code ?? "").Trim().ToUpperInvariant())
                                 .Select(g => g.First())
                                 .ToList();

                // Sort: preferred currencies first then alphabetical
                var preferred = new[] { "USD", "EUR", "GBP" }.Select(x => x.ToUpperInvariant()).ToList();
                var ordered = unique.OrderBy(r => preferred.Contains((r.Code ?? "").ToUpperInvariant()) ? preferred.IndexOf((r.Code ?? "").ToUpperInvariant()) : int.MaxValue)
                                    .ThenBy(r => r.Code)
                                    .ToList();

                // Clean up name duplicates: if Name equals Code, only show Code once (we will display NameOrCode property in UI)
                foreach (var r in ordered)
                {
                    // optional: if Name equals Code, set Name to Code (UI will show once)
                    // nothing to change in model; handled in UI via binding to NameOrCode
                    Rates.Add(r);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading rates: " + ex);
                ErrorMessage = "Error loading rates: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}