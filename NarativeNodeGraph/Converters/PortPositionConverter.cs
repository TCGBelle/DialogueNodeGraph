using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NarativeNodeGraph.ViewModels;

namespace NarativeNodeGraph.Converters
{
    public class PortToPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not PortViewModel port)
                return new Point(0, 0);

            var node = port.ParentNode;

            // Basic layout assumptions:
            // Title height ≈ 25
            // Each port row ≈ 22
            // Port circle center offset ≈ 5

            int portIndex = node.Ports.IndexOf(port);

            double portOffsetY = 30 + portIndex * 22 + 5;

            double portOffsetX = port.Type == Models.PortType.Output
                ? 120   // right side of node (adjust to your node width)
                : 0;    // left side

            return new Point(
                node.X + portOffsetX,
                node.Y + portOffsetY
            );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
