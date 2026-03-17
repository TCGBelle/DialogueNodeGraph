using System;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;
using NarativeNodeGraph.ViewModels;

namespace NarativeNodeGraph.Behaviors
{
    public class NodeResizeBehavior : Behavior<Thumb>
    {
        public static readonly System.Windows.DependencyProperty DirectionProperty =
            System.Windows.DependencyProperty.Register(
                nameof(Direction),
                typeof(ResizeDirection),
                typeof(NodeResizeBehavior),
                new System.Windows.PropertyMetadata(ResizeDirection.Right));

        public ResizeDirection Direction
        {
            get => (ResizeDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DragDelta += OnDragDelta;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DragDelta -= OnDragDelta;
            base.OnDetaching();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (AssociatedObject.DataContext is not NodeViewModel node)
                return;

            var left = Direction is ResizeDirection.Left
                or ResizeDirection.TopLeft
                or ResizeDirection.BottomLeft;

            var right = Direction is ResizeDirection.Right
                or ResizeDirection.TopRight
                or ResizeDirection.BottomRight;

            var top = Direction is ResizeDirection.Top
                or ResizeDirection.TopLeft
                or ResizeDirection.TopRight;

            var bottom = Direction is ResizeDirection.Bottom
                or ResizeDirection.BottomLeft
                or ResizeDirection.BottomRight;

            if (right)
            {
                node.Width = Math.Max(node.MinWidth, node.Width + e.HorizontalChange);
            }

            if (bottom)
            {
                node.Height = Math.Max(node.MinHeight, node.Height + e.VerticalChange);
            }

            if (left)
            {
                var newWidth = Math.Max(node.MinWidth, node.Width - e.HorizontalChange);
                var widthDelta = newWidth - node.Width;

                node.Width = newWidth;
                node.X -= widthDelta;
            }

            if (top)
            {
                var newHeight = Math.Max(node.MinHeight, node.Height - e.VerticalChange);
                var heightDelta = newHeight - node.Height;

                node.Height = newHeight;
                node.Y -= heightDelta;
            }
        }
    }

    public enum ResizeDirection
    {
        Left,
        Top,
        Right,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}

