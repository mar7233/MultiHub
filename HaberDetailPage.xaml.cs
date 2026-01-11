using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;                // Browser
using Microsoft.Maui.ApplicationModel.DataTransfer;   // Share
using MultiHubApp.Models;
using System.Threading.Tasks;

namespace MultiHubApp.Views
{
    public partial class HaberDetailPage : ContentPage
    {
        private readonly NewsItem _item;
        private bool _showingOriginal = false;

        public HaberDetailPage(NewsItem item)
        {
            InitializeComponent();
            _item = item ?? throw new ArgumentNullException(nameof(item));
            SetupUI();

            // Show the original article by default. If it fails to load, we'll fall back to the summary.
            ShowOriginal();
        }

        private void SetupUI()
        {
            TitleLabel.Text = _item.Title ?? string.Empty;
            SourceLabel.Text = _item.Source ?? string.Empty;
            DateLabel.Text = _item.PublishDate?.ToString("g") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(_item.ImageUrl))
            {
                HeaderImage.Source = _item.ImageUrl;
                HeaderImage.IsVisible = true;
            }
            else
            {
                HeaderImage.IsVisible = false;
            }
        }

        private void ShowSummary()
        {
            _showingOriginal = false;
            BtnToggleView.Text = "View original";

            var contentHtml = string.IsNullOrWhiteSpace(_item.Summary)
                ? "<html><body><p>No summary available.</p></body></html>"
                : (_item.Summary.Contains("<") && _item.Summary.Contains(">") ? _item.Summary : $"<html><body><p>{System.Net.WebUtility.HtmlEncode(_item.Summary).Replace("\n", "<br/>")}</p></body></html>");

            ContentWebView.Source = new HtmlWebViewSource { Html = $"<html><head><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\" /></head><body style=\"font-family: -apple-system, Roboto, 'Segoe UI', Arial; padding:12px;\">{contentHtml}</body></html>" };
        }

        private void ShowOriginal()
        {
            _showingOriginal = true;
            BtnToggleView.Text = "Show summary";

            // If Link is empty, fall back to summary immediately
            if (string.IsNullOrWhiteSpace(_item.Link))
            {
                ShowSummary();
                return;
            }

            try
            {
                // Load the original article URL inside the WebView
                ContentWebView.Source = new UrlWebViewSource { Url = _item.Link };
            }
            catch (Exception)
            {
                // If an exception occurs while assigning the URL, fall back to summary
                ShowSummary();
            }
        }

        private void OnToggleViewClicked(object sender, EventArgs e)
        {
            if (_showingOriginal)
                ShowSummary();
            else
                ShowOriginal();
        }

        private async void OnOpenInBrowserClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_item.Link))
            {
                await DisplayAlert("Open", "No link available.", "OK");
                return;
            }

            try
            {
                await Browser.OpenAsync(_item.Link, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Open Error", ex.Message, "OK");
            }
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            try
            {
                var req = new ShareTextRequest
                {
                    Title = _item.Title,
                    Text = _item.Summary,
                    Uri = _item.Link
                };
                await Share.RequestAsync(req);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Share Error", ex.Message, "OK");
            }
        }

        // WebView loading indicators
        private void ContentWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            WebLoadingIndicator.IsRunning = true;
            WebLoadingIndicator.IsVisible = true;
        }

        private async void ContentWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            WebLoadingIndicator.IsRunning = false;
            WebLoadingIndicator.IsVisible = false;

            // If navigation failed when trying to load original, switch automatically to summary (if summary exists)
            if (_showingOriginal && (e.Result == WebNavigationResult.Failure || e.Result == WebNavigationResult.Timeout))
            {
                if (!string.IsNullOrWhiteSpace(_item.Summary))
                {
                    // Inform user then show summary
                    await DisplayAlert("Unable to load", "The original article could not be loaded inside the app. Showing the summary instead. You can open the original in browser.", "OK");
                    ShowSummary();
                }
                else
                {
                    // No summary available — inform user and offer to open in browser
                    var open = await DisplayAlert("Unable to load", "The original article could not be loaded inside the app and no summary is available. Open in browser?", "Open", "Cancel");
                    if (open && !string.IsNullOrWhiteSpace(_item.Link))
                    {
                        try { await Browser.OpenAsync(_item.Link, BrowserLaunchMode.SystemPreferred); }
                        catch (Exception ex) { await DisplayAlert("Open Error", ex.Message, "OK"); }
                    }
                }
                return;
            }

            // If loaded successfully and showing original, try to remove ads by JS injection.
            if (_showingOriginal && e.Result == WebNavigationResult.Success)
            {
                await RemoveAdsFromWebViewAsync();
            }
        }

        // Attempt to remove common ad/banner elements by injecting JS.
        // Runs multiple times (delays) to catch dynamically loaded banners.
        private async Task RemoveAdsFromWebViewAsync()
        {
            // JavaScript: remove elements matching common ad-related selectors, and run repeatedly
            var js = @"
(function removeAds(){
    try {
        var selectors = [
            '[id*=""ad""]',
            '[class*=""ad""]',
            '[class*=""reklam""]',
            '[id*=""reklam""]',
            '[class*=""banner""]',
            '[id*=""banner""]',
            '[class*=""sponsor""]',
            '[id*=""sponsor""]',
            '.advert',
            '.advertisement',
            'iframe[src*=""ads""]',
            'iframe[id*=""ad""]',
            'ins', // common ad tag
            '[role=""complementary""]'
        ];
        selectors.forEach(function(s){
            var els = document.querySelectorAll(s);
            for (var i=0;i<els.length;i++){ try{ els[i].style.display = 'none'; }catch(e){} }
        });
        // also try to hide large fixed banners by checking element position/size
        var all = Array.from(document.querySelectorAll('div,section'));
        all.forEach(function(el){
            try {
                var rect = el.getBoundingClientRect();
                if (rect.width > (window.innerWidth * 0.6) && rect.height > 80) {
                    // Heuristic: likely a large banner — hide if contains images with no article content
                    el.style.display = 'none';
                }
            } catch(e) {}
        });
    } catch(e) {}
})();
// run again after delays to catch lazy-loaded ads
setTimeout(removeAds, 700);
setTimeout(removeAds, 1500);
setTimeout(removeAds, 3000);
";

            try
            {
                // Evaluate JS inside the WebView
                // Use EvaluateJavaScriptAsync (available on MAUI WebView)
                await ContentWebView.EvaluateJavaScriptAsync(js);
            }
            catch (Exception)
            {
                // Some pages may block script injection or WebView may not allow it; ignore and leave page as-is
            }
        }
    }
}