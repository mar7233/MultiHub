using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MultiHubApp.Helpers
{
    public class DirectionToBoolConverter : IValueConverter
    {
        // param: "up" or "down"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dir = value as string;
            var param = parameter as string;
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(param)) return false;
            return string.Equals(dir, param, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}