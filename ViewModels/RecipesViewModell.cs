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
    public class RecipesViewModell
    {
        private readonly IRecipeService _recipeService;
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
                (PlanRecipeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditRecipeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        public ICommand AddRecipeCommand { get; }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand PlanRecipeCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand ShowAllRecipesCommand { get; }
        public ICommand FindRecipesCommand { get; }
        public ICollectionView RecipesView { get; }

        public RecipesViewModell(IRecipeService recipeService)
        {
            _recipeService = recipeService;

            Recipes = new ObservableCollection<Recipe>(_recipeService.GetAll());

            RecipesView = CollectionViewSource.GetDefaultView(Recipes);

            AddRecipeCommand = new RelayCommand(AddRecipe);
            DeleteRecipeCommand = new RelayCommand(DeleteRecipe, CanDeleteRecipe);
            PlanRecipeCommand = new RelayCommand(PlanRecipe, CanPlanRecipe);
            EditRecipeCommand = new RelayCommand(EditRecipe, () => SelectedRecipe != null);
            FindRecipesCommand = new RelayCommand(FindRecipes, CanFindRecipes);
            ShowAllRecipesCommand = new RelayCommand(ShowAllRecipes);
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
        private bool CanPlanRecipe()
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
            return PantryItems.Any();
        }
        private void FindRecipes()
        {
            RecipesView.Filter = obj =>
            {
                if (obj is not Recipe r)
                    return false;

                return PantryItems.Any(p =>
                    r.HasIngredient(p));
            };
        }
        private void ShowAllRecipes()
        {
            RecipesView.Filter = null;
        }

    }
}
