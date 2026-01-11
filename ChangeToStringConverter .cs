using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MultiHubApp.Helpers
{
    public class ChangeToStringConverter : IValueConverter
    {
        private readonly CultureInfo displayCulture = CultureInfo.InvariantCulture;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (!double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) &&
                !double.TryParse(value.ToString(), NumberStyles.Any, new CultureInfo("tr-TR"), out d))
            {
                return value.ToString();
            }

            var sign = d > 0 ? "+" : d < 0 ? "-" : "";
            return $"{sign}{Math.Abs(d).ToString("N2", displayCulture)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}