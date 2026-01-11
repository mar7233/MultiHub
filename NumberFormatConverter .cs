using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MultiHubApp.Helpers
{
    public class NumberFormatConverter : IValueConverter
    {
        private readonly CultureInfo displayCulture = CultureInfo.InvariantCulture; // use "." decimal

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            if (!double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) &&
                !double.TryParse(value.ToString(), NumberStyles.Any, new CultureInfo("tr-TR"), out d))
            {
                return value.ToString();
            }

            var param = (parameter as string) ?? "auto";

            if (param.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                // dynamic formatting:
                // - if value < 1 show 4 decimals
                // - if value < 100 show 4 decimals
                // - if value between 100 and 1000 show 2 decimals
                // - if value >= 1000 show 2 decimals with thousands separator
                if (Math.Abs(d) < 100) return d.ToString("N4", displayCulture);
                return d.ToString("N2", displayCulture);
            }

            // otherwise use provided format (e.g., "N4", "N2")
            try
            {
                return d.ToString(param, displayCulture);
            }
            catch
            {
                return d.ToString("N4", displayCulture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}