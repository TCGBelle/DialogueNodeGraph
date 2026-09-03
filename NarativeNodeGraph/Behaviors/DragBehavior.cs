using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NarativeNodeGraph.Behaviors
{
    public static class DragBehavior
    {
        public static readonly DependencyProperty DragCommandProperty =
            DependencyProperty.RegisterAttached(
                "DragCommand",
                typeof(ICommand),
                typeof(DragBehavior),
                new PropertyMetadata(null, OnDragCommandChanged));

        public static readonly DependencyProperty ZoomProperty =
    DependencyProperty.RegisterAttached(
        "Zoom",
        typeof(double),
        typeof(DragBehavior),
        new PropertyMetadata(1.0));

        public static void SetZoom(UIElement element, double value) =>
            element.SetValue(ZoomProperty, value);

        public static double GetZoom(UIElement element) =>
            (double)element.GetValue(ZoomProperty);

        public static void SetDragCommand(UIElement element, ICommand value) =>
            element.SetValue(DragCommandProperty, value);

        public static ICommand GetDragCommand(UIElement element) =>
            (ICommand)element.GetValue(DragCommandProperty);

        private static void OnDragCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewMouseLeftButtonDown -= OnMouseDown;
                element.PreviewMouseMove -= OnMouseMove;
                element.PreviewMouseLeftButtonUp -= OnMouseUp;

                if (e.NewValue is ICommand)
                {
                    element.PreviewMouseLeftButtonDown += OnMouseDown;
                    element.PreviewMouseMove += OnMouseMove;
                    element.PreviewMouseLeftButtonUp += OnMouseUp;
                }
            }
        }

        private static Point _start;
        private static bool _isDragging = false;
        private static UIElement? _currentElement;

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                if (IsInputElement(source))
                    return;
            }
            // Walk up from the original source until we reach the element with Tag="Port" (if any)
            DependencyObject? current = e.OriginalSource as DependencyObject;
            while (current != null)
            {
                if (current is FrameworkElement fe)
                {
                    if ((fe.Tag as string) == "Port")
                        return;

                    if (fe is Thumb)
                        return;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            if (sender is UIElement element)
            {
                _start = e.GetPosition(null);
                _currentElement = element;
                _isDragging = true;
                element.CaptureMouse();
                e.Handled = true; // good practice so node drag doesn't bubble weirdly
            }
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _currentElement != null && GetDragCommand(_currentElement) is ICommand cmd)
            {
                var position = e.GetPosition(null);
                var delta = position - _start;
                _start = position;

                double zoom = GetZoom(_currentElement);
                var scaledDelta = new Vector(delta.X / zoom, delta.Y / zoom);

                if (cmd.CanExecute((scaledDelta.X, scaledDelta.Y)))
                    cmd.Execute((scaledDelta.X, scaledDelta.Y));
            }
        }

        private static void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentElement != null)
            {
                _isDragging = false;
                _currentElement.ReleaseMouseCapture();
                _currentElement = null;
            }
        }

        private static bool IsInputElement(DependencyObject obj)
        {
            while (obj != null)
            {
                if (obj is TextBox || obj is PasswordBox || obj is RichTextBox || obj is ButtonBase)
                    return true;

                if (obj is FrameworkElement fe)
                {
                    if ((fe.Tag as string) == "Port")
                        return true;

                    if (fe is Thumb)
                        return true;
                }

                obj = VisualTreeHelper.GetParent(obj);
            }

            return false;
        }
    }
}
