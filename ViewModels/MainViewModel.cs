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
        public MainViewModel() {}
    }
}
