using Microsoft.Xaml.Behaviors;
using System.Windows.Input;
using System.Windows.Shapes;

namespace NarativeNodeGraph.Behaviors
{
    public class PortConnectionBehavior : Behavior<Ellipse>
    {
        public static readonly System.Windows.DependencyProperty BeginCommandProperty =
            System.Windows.DependencyProperty.Register(nameof(BeginCommand), typeof(ICommand), typeof(PortConnectionBehavior));

        public static readonly System.Windows.DependencyProperty EndCommandProperty =
            System.Windows.DependencyProperty.Register(nameof(EndCommand), typeof(ICommand), typeof(PortConnectionBehavior));

        public ICommand? BeginCommand
        {
            get => (ICommand?)GetValue(BeginCommandProperty);
            set => SetValue(BeginCommandProperty, value);
        }

        public ICommand? EndCommand
        {
            get => (ICommand?)GetValue(EndCommandProperty);
            set => SetValue(EndCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnMouseUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseUp;
            base.OnDetaching();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var param = AssociatedObject.DataContext; // <-- PortViewModel
            if (BeginCommand?.CanExecute(param) == true)
                BeginCommand.Execute(param);

            e.Handled = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var param = AssociatedObject.DataContext; // <-- PortViewModel
            if (EndCommand?.CanExecute(param) == true)
                EndCommand.Execute(param);

            e.Handled = true;
        }
    }
}
