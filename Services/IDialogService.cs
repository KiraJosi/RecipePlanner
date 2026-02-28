using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Models;

namespace RecipePlanner.Services
{
    interface IDialogService
    {
        bool? ShowAddRecipeDialog(out Recipe? recipe);
        bool? ShowEditRecipeDialog(Recipe recipe);
        bool? ShowPlannedMealDialog(PlannedMeal meal,List <Recipe> recipes);
        bool? ShowEditPantryDialog(string currentText, out string? updatedText);
    }
}
