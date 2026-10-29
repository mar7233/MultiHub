using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using MultiHubApp.Models;

namespace MultiHubApp.Services
{
    // Reads RSS feeds directly and parses them with LINQ to XML,
    // with helpers for image extraction and multi-feed merging.
    public class NewsService
    {
        private static readonly XNamespace Media = "http://search.yahoo.com/mrss/";

        private readonly HttpClient _http;

        public NewsService()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("MultiHubApp/1.0");
        }

        // Parse a single RSS feed into a list of NewsItem
        public async Task<List<NewsItem>> GetNewsFromRssAsync(string rssUrl)
        {
            var list = new List<NewsItem>();
            try
            {
                using var res = await _http.GetAsync(rssUrl);
                if (!res.IsSuccessStatusCode) return list;

                // Read raw bytes: some feeds send a malformed charset header,
                // and the XML declaration already names the encoding.
                await using var stream = await res.Content.ReadAsStreamAsync();
                var doc = XDocument.Load(stream);
                var channel = doc.Root?.Element("channel");
                if (channel == null) return list;

                var source = channel.Element("title")?.Value;

                foreach (var item in channel.Elements("item"))
                {
                    try
                    {
                        var title = item.Element("title")?.Value ?? "";
                        var summary = item.Element("description")?.Value ?? "";
                        var link = item.Element("link")?.Value ?? item.Element("guid")?.Value ?? "";

                        DateTime? pub = null;
                        var pubText = item.Element("pubDate")?.Value;
                        if (!string.IsNullOrWhiteSpace(pubText) &&
                            DateTimeOffset.TryParse(pubText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                        {
                            pub = dto.LocalDateTime;
                        }

                        // Image: media:content, then enclosure, then <img> inside the description
                        string? image = item.Element(Media + "content")?.Attribute("url")?.Value
                                     ?? item.Element(Media + "thumbnail")?.Attribute("url")?.Value;

                        var enclosure = item.Element("enclosure");
                        if (image == null && enclosure != null &&
                            (enclosure.Attribute("type")?.Value ?? "image").StartsWith("image", StringComparison.OrdinalIgnoreCase))
                        {
                            image = enclosure.Attribute("url")?.Value;
                        }

                        if (image == null)
                        {
                            var imgMatch = Regex.Match(summary, "<img[^>]+src\\s*=\\s*['\\\"](?<src>[^'\\\"]+)['\\\"][^>]*>", RegexOptions.IgnoreCase);
                            if (imgMatch.Success) image = imgMatch.Groups["src"].Value;
                        }

                        list.Add(new NewsItem
                        {
                            Title = HtmlToPlainText(title),
                            Summary = HtmlToPlainText(summary),
                            Link = link.Trim(),
                            PublishDate = pub,
                            ImageUrl = image,
                            Source = source
                        });
                    }
                    catch
                    {
                        // skip malformed item
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