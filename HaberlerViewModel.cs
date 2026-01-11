using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.Services;

namespace MultiHubApp.ViewModels
{
    public class HaberlerViewModel : BindableObject
    {
        private readonly NewsService _newsService = new NewsService();

        public ObservableCollection<NewsItem> Items { get; } = new ObservableCollection<NewsItem>();

        bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        public ICommand RefreshCommand { get; }

        // default categories (you can show these as buttons)
        public List<string> FeedUrls { get; } = new List<string>
        {
            "https://www.trthaber.com/manset_articles.rss",
            "https://www.trthaber.com/sondakika_articles.rss",
            "https://www.trthaber.com/gundem_articles.rss",
            "https://www.trthaber.com/ekonomi_articles.rss",
            "https://www.trthaber.com/spor_articles.rss",
            "https://www.trthaber.com/bilim_teknoloji_articles.rss",
            "https://www.trthaber.com/guncel_articles.rss"
        };

        public HaberlerViewModel()
        {
            RefreshCommand = new Command(async () => await LoadAllFeedsAsync());
            // load initial categories (merge of first 5 by default)
            _ = LoadAllFeedsAsync();
        }

        // load and merge many feeds (used for "All" view)
        public async Task LoadAllFeedsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            Items.Clear();

            try
            {
                // You can choose a subset of FeedUrls; here we use the first 5 as requested
                var urls = FeedUrls.Count >= 5 ? FeedUrls.GetRange(0, 5) : FeedUrls;
                var list = await _newsService.GetNewsFromFeedsAsync(urls);
                if (list == null || list.Count == 0)
                {
                    ErrorMessage = "No news found. Try refresh or check your internet connection.";
                    return;
                }

                foreach (var n in list) Items.Add(n);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading news: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // load a single category RSS
        public async Task LoadCategoryAsync(string rssUrl)
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = null;
            Items.Clear();

            try
            {
                var list = await _newsService.GetNewsFromRssAsync(rssUrl);
                if (list == null || list.Count == 0)
                {
                    ErrorMessage = "No news found for this category.";
                    return;
                }
                foreach (var n in list) Items.Add(n);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading news: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}