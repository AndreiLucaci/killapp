using System.Globalization;
using System.Windows;
using System;
using System.Windows.Data;

namespace KillApp.Converters
{
    internal sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            var invert = (parameter == null) ? false : Boolean.Parse(parameter.ToString());
            if ((Boolean)value ^ invert)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
