using System;
using System.Globalization;
using System.Windows.Data;

namespace RecipePlanner.Helpers
{
    public class IsTodayConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date.Date == DateTime.Today;
            return false;
        }

        public object ConvertBacK(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
