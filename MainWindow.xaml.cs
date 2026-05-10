using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using RecipePlanner.Models;
using RecipePlanner.ViewModels;

namespace RecipePlanner
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        
        private Point _startPoint;

        private void Recipe_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void Recipe_MouseMove (object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var listBoxItem = sender as ListBoxItem;
                if (listBoxItem == null) return;

                var recipe = listBoxItem.DataContext as Recipe;
                if (recipe != null)
                {
                    DragDrop.DoDragDrop((DependencyObject)sender, recipe, DragDropEffects.Copy);
                }
            }
        }

        private void Day_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Recipe))) return;
            {
                var recipe = (Recipe)e.Data.GetData(typeof(Recipe));
                var weekDay = (sender as FrameworkElement)?.DataContext as WeekDay;
                
                if (DataContext is MainViewModel vm && weekDay != null)
                    vm.MealPlan.PlanRecipeFromDrop(recipe, weekDay.Date);
            }
        }
    }
}