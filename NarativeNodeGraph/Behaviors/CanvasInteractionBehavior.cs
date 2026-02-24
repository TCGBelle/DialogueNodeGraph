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

        protected override void OnAttached()
        {
            base.OnAttached();

            // GLOBAL mouse move — works even during capture
            InputManager.Current.PreProcessInput += OnPreProcessInput;

            AssociatedObject.MouseLeftButtonUp += OnMouseUp;
        }

        protected override void OnDetaching()
        {
            InputManager.Current.PreProcessInput -= OnPreProcessInput;

            AssociatedObject.MouseLeftButtonUp -= OnMouseUp;

            base.OnDetaching();
        }

        private void OnPreProcessInput(object sender, PreProcessInputEventArgs e)
        {
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
    }
}
