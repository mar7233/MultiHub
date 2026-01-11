using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MultiHubApp.Helpers
{
    public class IsSeparatorConverter : IValueConverter
    {
        // If parameter == "invert", return !isSep
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            var isSep = string.Equals(s, "SEP", StringComparison.OrdinalIgnoreCase);
            if (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase))
                return !isSep;
            return isSep;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}