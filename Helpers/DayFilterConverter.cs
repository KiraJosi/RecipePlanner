using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Models;

namespace RecipePlanner.Helpers
{
    internal class DayFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is PlannedMeal meal && values[1] is DateTime day)
            {
                return meal.Date.Date == day.Date
                ? Visibility.Visible
                : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
