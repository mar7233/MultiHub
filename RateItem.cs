namespace MultiHubApp.Models
{
    public class RateItem
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public double? Buying { get; set; }
        public double? Selling { get; set; }
        public double? Change { get; set; }
        // "up", "down", "none"
        public string Direction { get; set; } = "none";

        // Helper: return Name if it's meaningful and different from Code, otherwise return Code
        public string NameOrCode
        {
            get
            {
                var n = (Name ?? "").Trim();
                var c = (Code ?? "").Trim();
                if (string.IsNullOrEmpty(n)) return c;
                if (string.Equals(n, c, StringComparison.OrdinalIgnoreCase)) return c;
                return n;
            }
        }
    }
}