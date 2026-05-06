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
        public RecipesViewModell Recipes {  get; }
        public PantryViewModell Pantry { get; }
        public MealPlanViewModell MealPlan { get; }
        public MainViewModel(
            IRecipeService recipeService,
            IPantryService pantryService,
            IPlannedMealsService plannedMealsService,
            IDialogService dialogService) 
        {
            Pantry = new PantryViewModell(pantryService, dialogService);
            Recipes = new RecipesViewModell(recipeService, dialogService, Pantry);
            MealPlan = new MealPlanViewModell(plannedMealsService, dialogService, Recipes);
            Recipes.SetMealPlan(MealPlan);
        }
    }
}
