using RecipePlanner.Models;

namespace RecipePlanner.Services
{
    public interface IDialogService
    {
        bool? ShowAddRecipeDialog(out Recipe? recipe);
        bool? ShowEditRecipeDialog(Recipe recipe);
        bool? ShowPlannedMealDialog(PlannedMeal meal,List <Recipe> recipes);
        bool? ShowEditPantryDialog(string currentText, out string? updatedText);
        string? ShowSaveFileDialog(string filter, string defaultFileName);
        string? ShowOpenFileDialog(string filter);
    }
}
