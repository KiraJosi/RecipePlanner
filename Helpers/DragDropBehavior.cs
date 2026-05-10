using RecipePlanner.Models;
using System.DirectoryServices.ActiveDirectory;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RecipePlanner.Helpers
{
    public static class DragDropBehavior
    {
        public static readonly DependencyProperty EnableDragProperty =
            DependencyProperty.RegisterAttached(
                "EnableDrag",
                typeof(bool),
                typeof(DragDropBehavior),
                new PropertyMetadata(false, OnEnableDragChanged));
        public static bool GetEnableDrag(DependencyObject obj) =>
            (bool)obj.GetValue(EnableDragProperty);

        public static void SetEnableDrag(DependencyObject obj, bool value) =>
            obj.SetValue(EnableDragProperty, value);

        private static void OnEnableDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBoxItem item) return;

            if ((bool)e.NewValue)
            {
                item.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
                item.PreviewMouseMove += Item_PreviewMouseMove;
            }
            else
            {
                item.PreviewMouseLeftButtonDown -= Item_PreviewMouseLeftButtonDown;
                item.PreviewMouseMove -= Item_PreviewMouseMove;
            }
        }

        private static Point _startPoint;
        private static void Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private static void Item_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var diff = _startPoint - e.GetPosition(null);
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            if (sender is ListBoxItem item && item.DataContext is Recipe recipe)
                DragDrop.DoDragDrop(item, recipe, DragDropEffects.Copy);
        }
    }
}
