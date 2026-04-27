using Microsoft.Xaml.Behaviors;
using NarativeNodeGraph.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NarativeNodeGraph.Behaviors
{
    public sealed class NodeAutoSizeBehavior : Behavior<FrameworkElement>
    {
        private bool _hasAppliedAutoSize;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_hasAppliedAutoSize)
                return;

            AssociatedObject.Dispatcher.BeginInvoke(
                ApplyAutoSize,
                DispatcherPriority.Loaded);
        }

        private void ApplyAutoSize()
        {
            if (_hasAppliedAutoSize)
                return;

            if (AssociatedObject.DataContext is not NodeViewModel node)
                return;

            // Only autosize nodes that have not already been manually sized.
            if (!double.IsNaN(node.Width) || !double.IsNaN(node.Height))
                return;

            AssociatedObject.UpdateLayout();

            var measuredWidth = AssociatedObject.ActualWidth;
            var measuredHeight = AssociatedObject.ActualHeight;

            if (double.IsNaN(measuredWidth) ||
                double.IsNaN(measuredHeight) ||
                measuredWidth <= 0 ||
                measuredHeight <= 0)
                return;

            node.SetSize(measuredWidth, measuredHeight);
            _hasAppliedAutoSize = true;
        }
    }
}