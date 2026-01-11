// name=Helpers/TurkishNormalizer.cs
using System.Text;

namespace MultiHubApp.Helpers
{
    // Türkçe karakterleri normalize eden yardımcı (Türkçe yorum).
    public static class TurkishNormalizer
    {
        private static readonly System.Collections.Generic.Dictionary<char, char> Map = new()
        {
            ['Ş'] = 'S',
            ['ş'] = 's',
            ['İ'] = 'I',
            ['ı'] = 'i',
            ['Ö'] = 'O',
            ['ö'] = 'o',
            ['Ü'] = 'U',
            ['ü'] = 'u',
            ['Ç'] = 'C',
            ['ç'] = 'c',
            ['Ğ'] = 'G',
            ['ğ'] = 'g'
        };

        public static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
                sb.Append(Map.ContainsKey(ch) ? Map[ch] : ch);
            return sb.ToString();
        }
    }
}