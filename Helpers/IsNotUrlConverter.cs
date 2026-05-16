using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RecipePlanner.Helpers
{
    public class IsNotUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s) &&
                (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                return Visibility.Collapsed;

            if (value is string text && !string.IsNullOrWhiteSpace(text))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, object targetType, object parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
}
