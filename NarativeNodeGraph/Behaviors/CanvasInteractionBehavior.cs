using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NarativeNodeGraph.Behaviors
{
    public class CanvasInteractionBehavior : Behavior<Canvas>
    {
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(CanvasInteractionBehavior));

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register(nameof(MouseUpCommand), typeof(ICommand), typeof(CanvasInteractionBehavior));
        public static readonly DependencyProperty RightClickCommandProperty =
            DependencyProperty.Register(nameof(RightClickCommand), typeof(ICommand), typeof(CanvasInteractionBehavior));

        public static readonly DependencyProperty ZoomProperty =
    DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(CanvasInteractionBehavior),
        new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PanXProperty =
            DependencyProperty.Register(nameof(PanX), typeof(double), typeof(CanvasInteractionBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PanYProperty =
            DependencyProperty.Register(nameof(PanY), typeof(double), typeof(CanvasInteractionBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public double PanX
        {
            get => (double)GetValue(PanXProperty);
            set => SetValue(PanXProperty, value);
        }

        public double PanY
        {
            get => (double)GetValue(PanYProperty);
            set => SetValue(PanYProperty, value);
        }

        public ICommand? MouseMoveCommand
        {
            get => (ICommand?)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        public ICommand? MouseUpCommand
        {
            get => (ICommand?)GetValue(MouseUpCommandProperty);
            set => SetValue(MouseUpCommandProperty, value);
        }
        public ICommand? RightClickCommand
        {
            get => (ICommand?)GetValue(RightClickCommandProperty);
            set => SetValue(RightClickCommandProperty, value);
        }

        private bool _isPanning = false;
        private Point _panStart;

        protected override void OnAttached()
        {
            base.OnAttached();

            // GLOBAL mouse move — works even during capture
            InputManager.Current.PreProcessInput += OnPreProcessInput;

            AssociatedObject.MouseLeftButtonUp += OnMouseUp;
            AssociatedObject.PreviewMouseRightButtonDown += OnMouseRightButtonDown;
            AssociatedObject.PreviewMouseWheel += OnMouseWheel;
            AssociatedObject.PreviewMouseDown += OnMouseDown;
            AssociatedObject.PreviewMouseMove += OnMouseMovePan;
            AssociatedObject.PreviewMouseUp += OnMouseUpPan;
        }

        protected override void OnDetaching()
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;

            AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
            AssociatedObject.PreviewMouseRightButtonDown -= OnMouseRightButtonDown;
            AssociatedObject.PreviewMouseWheel -= OnMouseWheel;
            base.OnDetaching();
        }

        private void OnPreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            if (_isPanning) return; // don't process connection-preview movement while panning

            if (MouseMoveCommand == null)
                return;
            if (e.StagingItem.Input is MouseEventArgs mouseArgs &&
                mouseArgs.RoutedEvent == Mouse.MouseMoveEvent)
            {
                var p = mouseArgs.GetPosition(AssociatedObject);
                if (MouseMoveCommand.CanExecute(p))
                    MouseMoveCommand.Execute(p);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Ignore if released on a port
            DependencyObject? current = e.OriginalSource as DependencyObject;

            while (current != null)
            {
                if (current is FrameworkElement fe && (fe.Tag as string) == "Port")
                    return;

                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }

            if (MouseUpCommand?.CanExecute(null) == true)
                MouseUpCommand.Execute(null);
        }
        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(AssociatedObject);
            System.Diagnostics.Debug.WriteLine($"Right click at: {position}");
            if (RightClickCommand?.CanExecute(position) == true)
                RightClickCommand.Execute(position);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var cursorPos = e.GetPosition(null);

            var canvasPointBefore = new Point(
                (cursorPos.X - PanX) / Zoom,
                (cursorPos.Y - PanY) / Zoom);

            double zoomFactor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            double newZoom = Zoom * zoomFactor;
            newZoom = Math.Max(0.2, Math.Min(newZoom, 3.0));

            Zoom = newZoom;

            PanX = cursorPos.X - canvasPointBefore.X * newZoom;
            PanY = cursorPos.Y - canvasPointBefore.Y * newZoom;

            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _isPanning = true;
                _panStart = e.GetPosition(null);
                AssociatedObject.CaptureMouse();
                e.Handled = true;
            }
        }

        private void OnMouseMovePan(object sender, MouseEventArgs e)
        {
            if (_isPanning && e.MiddleButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(null);
                var delta = currentPos - _panStart;
                PanX += delta.X;
                PanY += delta.Y;
                _panStart = currentPos;
                e.Handled = true;
            }
        }

        private void OnMouseUpPan(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning && e.ChangedButton == MouseButton.Middle)
            {
                _isPanning = false;
                AssociatedObject.ReleaseMouseCapture();
                e.Handled = true;
            }
        }
    }
}
