using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace RecipePlanner.Views
{
    public partial class AddRecipeWindow : Window
    {
        private readonly Recipe? _recipeToEdit;

        public Recipe? NewRecipe { get; private set; }

        public AddRecipeWindow()
        {
            InitializeComponent();
            Title = "Neues Rezept hinzufügen";
        }

        public AddRecipeWindow(Recipe recipeToEdit)
        {
            InitializeComponent();

            Title = "Rezept bearbeiten";
            _recipeToEdit = recipeToEdit;

            NameTextBox.Text = recipeToEdit.Name;
            IngredientsTextBox.Text = string.Join(", ", recipeToEdit.Ingredients);
            StepsTextBox.Text = string.Join(Environment.NewLine, recipeToEdit.Steps);
            SourceTextBox.Text = recipeToEdit.Source;
            TagsTextBox.Text = string.Join(", ", recipeToEdit?.Tags);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Bitte einen Rezeptnamen eingeben!");
                return;
            }

            var ingredients = new ObservableCollection<string>(
                IngredientsTextBox.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                );
                

            var steps = new ObservableCollection<string> (
                StepsTextBox.Text
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
            );

            var tags = new ObservableCollection<string>(
                TagsTextBox.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                );

            if (_recipeToEdit != null)
            {
                _recipeToEdit.Name = NameTextBox.Text;
                _recipeToEdit.Ingredients = ingredients;
                _recipeToEdit.Steps = steps;
                _recipeToEdit.Source = SourceTextBox.Text;
                _recipeToEdit.Tags = tags;
            }
            else
            {
                NewRecipe = new Recipe
                {
                    Name = NameTextBox.Text,
                    Ingredients = ingredients,
                    Steps = steps,
                    Source = SourceTextBox.Text,
                    Tags = tags
                };
            }

            DialogResult = true;
            Close();
        }
    }
}
