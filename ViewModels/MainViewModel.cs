using RecipePlanner.Helpers;
using RecipePlanner.Models;
using RecipePlanner.Services;
using RecipePlanner.ViewModels.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RecipePlanner.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public RecipesViewModel Recipes {  get; }
        public PantryViewModel Pantry { get; }
        public MealPlanViewModel MealPlan { get; }
        public MainViewModel(
            IRecipeService recipeService,
            IPantryService pantryService,
            IPlannedMealsService plannedMealsService,
            IDialogService dialogService) 
        {
            Pantry = new PantryViewModel(pantryService, dialogService);
            Recipes = new RecipesViewModel(recipeService, dialogService, Pantry);
            MealPlan = new MealPlanViewModel(plannedMealsService, dialogService, Recipes);
            Recipes.SetMealPlan(MealPlan);
        }
    }
}
