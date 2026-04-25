using Microsoft.Xaml.Behaviors;
using NarativeNodeGraph.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NarativeNodeGraph.Behaviors
{
    public class NodeAutoSizeBehavior : Behavior<FrameworkElement>
    {
        private bool _done;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.LayoutUpdated += OnLayoutUpdated;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LayoutUpdated -= OnLayoutUpdated;
            base.OnDetaching();
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (_done) return;

            if (AssociatedObject.DataContext is NodeViewModel vm)
            {
                vm.SetSize(
                    AssociatedObject.ActualWidth,
                    AssociatedObject.ActualHeight);

                _done = true;
            }
        }
    }
}