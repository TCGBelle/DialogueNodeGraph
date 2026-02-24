using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NarativeNodeGraph.Models;

namespace NarativeNodeGraph.Converters
{
    public class PortTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not PortType portType) return Visibility.Collapsed;
            if (parameter is not string desiredText) return Visibility.Collapsed;

            return Enum.TryParse<PortType>(desiredText, out var desired) && portType == desired
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
