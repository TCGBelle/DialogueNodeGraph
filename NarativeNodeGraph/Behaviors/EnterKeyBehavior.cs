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
    public static class EnterKeyBehavior
    {
        public static readonly DependencyProperty CommitOnEnterProperty =
            DependencyProperty.RegisterAttached(
                "CommitOnEnter",
                typeof(bool),
                typeof(EnterKeyBehavior),
                new PropertyMetadata(false, OnCommitOnEnterChanged));

        public static void SetCommitOnEnter(UIElement element, bool value) =>
            element.SetValue(CommitOnEnterProperty, value);

        public static bool GetCommitOnEnter(UIElement element) =>
            (bool)element.GetValue(CommitOnEnterProperty);

        private static void OnCommitOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.PreviewKeyDown -= OnPreviewKeyDown;
                if ((bool)e.NewValue)
                    textBox.PreviewKeyDown += OnPreviewKeyDown;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}
