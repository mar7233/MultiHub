using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using MultiHubApp.Models;

namespace MultiHubApp.Helpers
{
    // Converter used in KurlarPage XAML to decide whether to show the Code label.
    // It expects to receive the whole RateItem as the binding value (Binding=".").
    // Returns true when Name != Code (i.e. show the Code label), false otherwise.
    public class StringEqualsConverter : IValueConverter
    {
        // value: the bound object (RateItem) OR a string name (legacy)
        // parameter: optional (not used)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // If the XAML binds the whole object (Binding="."), value will be RateItem
                if (value is RateItem r)
                {
                    var name = (r.Name ?? "").Trim();
                    var code = (r.Code ?? "").Trim();

                    // If name is empty -> don't show second label
                    if (string.IsNullOrEmpty(name)) return false;

                    // Show Code label only if Code is not same as Name
                    return !string.Equals(name, code, StringComparison.OrdinalIgnoreCase);
                }

                // Fallback: if value is a string (e.g. Name) and parameter supplied as code (not likely in MAUI)
                if (value is string nameStr && parameter is string codeParam)
                {
                    if (string.IsNullOrWhiteSpace(nameStr)) return false;
                    return !string.Equals(nameStr.Trim(), codeParam.Trim(), StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}