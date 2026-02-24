using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NarativeNodeGraph.Converters
{
    public class PointComponentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Point p || parameter is not string axis)
                return 0d;

            return axis == "X" ? p.X : p.Y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
