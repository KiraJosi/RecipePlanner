using RecipePlanner.Models;

namespace RecipePlanner.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowAddRecipeDialog(out Recipe? recipe)
        {
            var window = new AddRecipeWindow();
            var result = window.ShowDialog();
            recipe = window.NewRecipe;
            return result;
        }

        public bool? ShowEditRecipeDialog(Recipe recipe)
        {
            var window = new AddRecipeWindow(recipe);
            return window.ShowDialog();
        }

        public bool? ShowPlannedMealDialog(PlannedMeal meal, List <Recipe> recipes)
        {
            var window = new PlannedMealWindow(meal, recipes);  
            return window.ShowDialog();
        }

        public bool? ShowEditPantryDialog(string currentText, out string? updatedText)
        {
            var dialog = new InputDialog("Vorrat bearbeiten:", currentText);
            var result = dialog.ShowDialog();
            updatedText = dialog.InputText;
            return result;
        }
    }
}