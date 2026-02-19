using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RecipePlanner.Models;

namespace RecipePlanner
{
    public partial class PlannedMealWindow : Window
    {
        private readonly PlannedMeal _meal;
        private readonly List<Recipe> _recipes;

        public PlannedMealWindow(PlannedMeal meal, List<Recipe> recipes)
        {
            InitializeComponent();

            _meal = meal;
            _recipes = recipes;

            Title = meal.Date == default
                ? "Mahlzeit planen"
                : "Geplante Mahlzeit bearbeiten";

            RecipeComboBox.ItemsSource = _recipes;

            DatePicker.SelectedDate =
                meal.Date == default ? DateTime.Today : meal.Date;

            if (meal.Recipe != null)
                RecipeComboBox.SelectedItem = meal.Recipe;
            else if (_recipes.Any())
                RecipeComboBox.SelectedIndex = 0;

            DatePicker.DisplayDateStart = DateTime.Today;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Bitte ein Datum auswählen.");
                return;
            }

            if (RecipeComboBox.SelectedItem is not Recipe selectedRecipe)
            {
                MessageBox.Show("Bitte ein Rezept auswählen.");
                return;
            }

            _meal.Date = DatePicker.SelectedDate.Value;
            _meal.Recipe = selectedRecipe;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

