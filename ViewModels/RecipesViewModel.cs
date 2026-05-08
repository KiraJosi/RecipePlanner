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
    public class RecipesViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private readonly IDialogService _dialogService;
        private readonly PantryViewModel _pantry;
        private bool _pantryFilterActive = false;
        public ObservableCollection<Recipe> Recipes { get; }

        private Recipe? _selectedRecipe;
        public Recipe? SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                _selectedRecipe = value;
                OnPropertyChanged();
                (DeleteRecipeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditRecipeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
        public void SetMealPlan(MealPlanViewModel mealPlan)
        {
            _mealPlanCommand = mealPlan.PlanRecipeCommand as RelayCommand;
        }
        private RelayCommand? _mealPlanCommand;

        public ICommand AddRecipeCommand { get; }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand ShowAllRecipesCommand { get; }
        public ICommand FindRecipesCommand { get; }
        public ICollectionView RecipesView { get; }

        public RecipesViewModel(IRecipeService recipeService, IDialogService dialogService, PantryViewModel pantry)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
            _pantry = pantry;

            Recipes = new ObservableCollection<Recipe>(_recipeService.GetAll());

            RecipesView = CollectionViewSource.GetDefaultView(Recipes);

            AddRecipeCommand = new RelayCommand(AddRecipe);
            DeleteRecipeCommand = new RelayCommand(DeleteRecipe, CanDeleteRecipe);
            EditRecipeCommand = new RelayCommand(EditRecipe, () => SelectedRecipe != null);
            FindRecipesCommand = new RelayCommand(FindRecipes, CanFindRecipes);
            ShowAllRecipesCommand = new RelayCommand(ShowAllRecipes);
            _mealPlanCommand?.RaiseCanExecuteChanged();
        }
        private void AddRecipe()
        {
            if (_dialogService.ShowAddRecipeDialog(out Recipe? recipe) == true
                && recipe != null)
            {
                _recipeService.Add(recipe);
                Recipes.Add(recipe);
                SelectedRecipe = recipe;
            }
        }
        private void DeleteRecipe()
        {
            if (SelectedRecipe != null)
            {
                _recipeService.Delete(SelectedRecipe.Id);
                Recipes.Remove(SelectedRecipe);
            }
        }
        private bool CanDeleteRecipe()
        {
            return SelectedRecipe != null;
        }
        private void EditRecipe()
        {
            if (SelectedRecipe == null)
                return;

            if (_dialogService.ShowEditRecipeDialog(SelectedRecipe) == true)
            {
                _recipeService.Update(SelectedRecipe);
            }
        }
        private bool CanFindRecipes()
        {
            return _pantry.PantryItems.Any();
        }
        private void FindRecipes()
        {
            _pantryFilterActive = true;
            ApplyFilter();
        }
        private void ShowAllRecipes()
        {
            _pantryFilterActive = false;
            ApplyFilter();
        }
        private void ApplyFilter()
        {
            RecipesView.Filter = obj =>
            {
                if (obj is not Recipe r)
                    return false;

                bool matchesSearch = string.IsNullOrWhiteSpace(_searchText)
                    || r.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase);

                bool matchesPantry = !_pantryFilterActive
                    || _pantry.PantryItems.Any(p => r.HasIngredient(p));

                return matchesSearch && matchesPantry;
            };
        }
    }
}
