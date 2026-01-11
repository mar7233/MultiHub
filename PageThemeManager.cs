using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MultiHubApp.Models;

namespace MultiHubApp.Services
{
    // RSS -> JSON using rss2json API (your original approach),
    // with helpers for image extraction and multi-feed merging.
    public class NewsService
    {
        private readonly HttpClient _http = new();

        private const string Rss2JsonEndpoint = "https://api.rss2json.com/v1/api.json?rss_url=";

        // Parse single RSS (via rss2json) to list of NewsItem
        public async Task<List<NewsItem>> GetNewsFromRssAsync(string rssUrl)
        {
            var list = new List<NewsItem>();
            try
            {
                var url = Rss2JsonEndpoint + Uri.EscapeDataString(rssUrl);
                var res = await _http.GetAsync(url);
                if (!res.IsSuccessStatusCode) return list;

                var body = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        try
                        {
                            var title = item.GetProperty("title").GetString() ?? "";
                            var summary = item.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                            var link = item.TryGetProperty("link", out var l) ? l.GetString() ?? "" : "";
                            DateTime? pub = null;
                            if (item.TryGetProperty("pubDate", out var p) && !string.IsNullOrWhiteSpace(p.GetString()))
                            {
                                if (DateTime.TryParse(p.GetString(), out var dt)) pub = dt.ToLocalTime();
                            }

                            // image: rss2json sometimes returns "thumbnail" or "enclosure" or inside description
                            string? image = null;
                            if (item.TryGetProperty("thumbnail", out var thumb) && thumb.ValueKind == JsonValueKind.String)
                            {
                                image = thumb.GetString();
                            }
                            else if (item.TryGetProperty("enclosure", out var enclosure) && enclosure.ValueKind == JsonValueKind.Object)
                            {
                                if (enclosure.TryGetProperty("link", out var encLink)) image = encLink.GetString();
                                else if (enclosure.TryGetProperty("url", out var encUrl)) image = encUrl.GetString();
                            }
                            else if (item.TryGetProperty("media_thumbnail", out var mt) && mt.ValueKind == JsonValueKind.Object)
                            {
                                if (mt.TryGetProperty("url", out var mtUrl)) image = mtUrl.GetString();
                            }
                            else
                            {
                                // fallback: extract <img src=""> from description HTML
                                var desc = summary ?? "";
                                var imgMatch = Regex.Match(desc, "<img[^>]+src\\s*=\\s*['\\\"](?<src>[^'\\\"]+)['\\\"][^>]*>", RegexOptions.IgnoreCase);
                                if (imgMatch.Success) image = imgMatch.Groups["src"].Value;
                            }

                            // Source: rss2json returns "feed" -> "title" sometimes
                            string? source = null;
                            if (root.TryGetProperty("feed", out var feed) && feed.ValueKind == JsonValueKind.Object)
                            {
                                if (feed.TryGetProperty("title", out var feedTitle) && feedTitle.ValueKind == JsonValueKind.String)
                                    source = feedTitle.GetString();
                            }

                            var news = new NewsItem
                            {
                                Title = HtmlToPlainText(title),
                                Summary = HtmlToPlainText(summary),
                                Link = link,
                                PublishDate = pub,
                                ImageUrl = image,
                                Source = source
                            };
                            list.Add(news);
                        }
                        catch
                        {
                            // skip malformed item
                        }
                    }
                }
            }
            catch
            {
                // swallow; caller will handle empty list / error message
            }
            return list;
        }

        // Helper: fetch multiple RSS feeds (merge, dedupe by Link, sort by date desc)
        public async Task<List<NewsItem>> GetNewsFromFeedsAsync(IEnumerable<string> rssUrls)
        {
            var all = new List<NewsItem>();
            foreach (var u in rssUrls)
            {
                try
                {
                    var partial = await GetNewsFromRssAsync(u);
                    all.AddRange(partial);
                }
                catch
                {
                    // ignore single feed failure
                }
            }

            // dedupe by Link
            var unique = all
                .Where(x => !string.IsNullOrWhiteSpace(x.Link))
                .GroupBy(x => x.Link.Trim())
                .Select(g => g.OrderByDescending(i => i.PublishDate ?? DateTime.MinValue).First())
                .ToList();

            // sort by PublishDate desc (nulls last)
            var ordered = unique.OrderByDescending(i => i.PublishDate ?? DateTime.MinValue).ToList();
            return ordered;
        }

        // Simple HTML -> plain text (remove tags and decode)
        private string HtmlToPlainText(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var noTags = Regex.Replace(html, "<.*?>", " ");
            noTags = System.Net.WebUtility.HtmlDecode(noTags);
            noTags = Regex.Replace(noTags, "\\s+", " ").Trim();
            if (noTags.Length > 800) noTags = noTags.Substring(0, 800) + "...";
            return noTags;
        }
    }
}