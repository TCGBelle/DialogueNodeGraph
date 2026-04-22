using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NarativeNodeGraph.ViewModels
{
    public class ConnectionViewModel : INotifyPropertyChanged
    {
        public PortViewModel From { get; }
        public PortViewModel? To { get; }

        public ICommand? DeleteCommand { get; }
        public ConnectionViewModel(PortViewModel from, PortViewModel? to)
            : this(from, to, null)
        {
        }

        public ConnectionViewModel(PortViewModel from, PortViewModel? to, ICommand? deleteCommand)
        {
            From = from;
            To = to;
            DeleteCommand = deleteCommand;

            From.ParentNode.PropertyChanged += (_, __) => RaiseAll();
            if (To != null)
            {
                To.ParentNode.PropertyChanged += (_, __) => RaiseAll();
            }
        }

        public double X1 => GetPortPoint(From).X;
        public double Y1 => GetPortPoint(From).Y;
        public double X2 => To != null ? GetPortPoint(To).X : 0;
        public double Y2 => To != null ? GetPortPoint(To).Y : 0;

        public Point StartPoint => GetPortPoint(From);
        public Point? OverrideEndPoint { get; set; }
        public Point EndPoint
        {
            get
            {
                if (OverrideEndPoint is Point p) return p;
                if (To != null) return GetPortPoint(To);
                return StartPoint;
            }
        }
        private Point GetPortPoint(PortViewModel port)
        {
            var node = port.ParentNode;

            var sameSidePorts = node.Ports
                .Where(p => p.Type == port.Type)
                .ToList();

            int portIndex = sameSidePorts
            .OrderBy(p => p.Id)
            .ToList()
            .IndexOf(port);

            double offsetY = 12 + (portIndex * 22) + 5;
            double offsetX = port.Type == PortType.Output
            ? node.Width
            : 0;

            return new Point(node.X + offsetX,
                             node.Y + offsetY);
        }

        public void RaiseGeometryChanged()
        {
            PropertyChanged?.Invoke(this, new(nameof(StartPoint)));
            PropertyChanged?.Invoke(this, new(nameof(EndPoint)));
            PropertyChanged?.Invoke(this, new(nameof(ControlPoint1)));
            PropertyChanged?.Invoke(this, new(nameof(ControlPoint2)));
        }
        private void RaiseAll()
        {
            PropertyChanged?.Invoke(this, new(nameof(StartPoint)));
            PropertyChanged?.Invoke(this, new(nameof(EndPoint)));
            PropertyChanged?.Invoke(this, new(nameof(ControlPoint1)));
            PropertyChanged?.Invoke(this, new(nameof(ControlPoint2)));
            PropertyChanged?.Invoke(this, new(nameof(X1)));
            PropertyChanged?.Invoke(this, new(nameof(Y1)));
            PropertyChanged?.Invoke(this, new(nameof(X2)));
            PropertyChanged?.Invoke(this, new(nameof(Y2)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Point ControlPoint1
        {
            get
            {
                var start = StartPoint;
                var end = EndPoint;
                var dx = Math.Abs(end.X - start.X) * 0.5;
                return new Point(start.X + dx, start.Y);
            }
        }

        public Point ControlPoint2
        {
            get
            {
                var start = StartPoint;
                var end = EndPoint;
                var dx = Math.Abs(end.X - start.X) * 0.5;
                return new Point(end.X - dx, end.Y);
            }
        }
    }
}
