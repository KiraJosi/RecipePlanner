using RecipePlanner.Helpers;
using RecipePlanner.Models;
using RecipePlanner.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RecipePlanner.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private readonly IPantryService _pantryService;
        private readonly IPlannedMealsService _plannedMealsService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Recipe> Recipes { get; }
        public ObservableCollection<string> PantryItems { get; }
        public ObservableCollection<PlannedMeal> PlannedMeals { get; }

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

        private string? _newPantryText;
        public string? NewPantryText
        {
            get => _newPantryText;
            set
            {
                _newPantryText = value;
                OnPropertyChanged();

                (SavePantryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private PlannedMeal? _selectedPlannedMeal;
        public PlannedMeal? SelectedPlannedMeal
        {
            get => _selectedPlannedMeal;
            set
            {
                _selectedPlannedMeal = value;
                OnPropertyChanged();
                (DeletePlannedMealCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditPlannedMealCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddRecipeCommand { get; }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand PlanRecipeCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand ShowAllRecipesCommand { get; }
        public ICommand FindRecipesCommand { get; }
        public ICommand SavePantryCommand { get; }
        public ICommand DeletePantryCommand { get; }
        public ICommand EditPantryCommand { get; }
        public ICommand DeletePlannedMealCommand { get; }
        public ICommand EditPlannedMealCommand { get; }

        public ICollectionView RecipesView { get; }


        public MainViewModel()
        {
            var dataService = new SQLiteDataService();
            _recipeService = new RecipeService(dataService);
            _pantryService = new PantryService(dataService);
            _plannedMealsService = new PlannedMealsService(dataService);

            _dialogService = new DialogService();

            Recipes = new ObservableCollection<Recipe>(
                _recipeService.GetAll()
            );
            RecipesView = CollectionViewSource.GetDefaultView(Recipes);

            PantryItems = new ObservableCollection<string>(
                _pantryService.GetAll()
                );

            PantryItems.CollectionChanged += (_, __) =>
            {
                _pantryService.Save(PantryItems.ToList());
            };

            PlannedMeals = new ObservableCollection<PlannedMeal>(
                _plannedMealsService.GetAll()
            );

            PlannedMeals.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (PlannedMeal meal in e.NewItems)
                        meal.PropertyChanged += PlannedMeal_PropertyChanged;

                if (e.OldItems != null)
                    foreach (PlannedMeal meal in e.OldItems)
                        meal.PropertyChanged -= PlannedMeal_PropertyChanged;

                SavePlannedMeals();
            };

            AddRecipeCommand = new RelayCommand(AddRecipe);
            DeleteRecipeCommand = new RelayCommand(DeleteRecipe, CanDeleteRecipe);
            PlanRecipeCommand = new RelayCommand(PlanRecipe, CanPlanRecipe);
            SavePantryCommand = new RelayCommand(SavePantry, CanSavePantry);
            DeletePantryCommand = new RelayCommand(DeletePantry, () => PantryItems.Any());
            DeletePlannedMealCommand = new RelayCommand(DeletePlannedMeal, () => SelectedPlannedMeal != null);
            EditRecipeCommand = new RelayCommand(EditRecipe, () => SelectedRecipe != null);
            FindRecipesCommand = new RelayCommand(FindRecipes, CanFindRecipes);
            ShowAllRecipesCommand = new RelayCommand(ShowAllRecipes);
            EditPantryCommand = new RelayCommand(EditPantry);
            EditPlannedMealCommand = new RelayCommand(EditPlannedMeal, () => SelectedPlannedMeal != null);

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

        private void PlanRecipe()
        {
            if (SelectedRecipe == null)
                return;

            var newMeal = new PlannedMeal 
            { 
                Recipe = SelectedRecipe, 
                Date = DateTime.Today 
            };

            if (_dialogService.ShowPlannedMealDialog(newMeal, Recipes.ToList()) == true)
            {
                PlannedMeals.Add(newMeal);
            }
        }

        private bool CanSavePantry()
        {
            return !string.IsNullOrWhiteSpace(NewPantryText);
        }

        private void SavePantry()
        {
            if (string.IsNullOrWhiteSpace(NewPantryText))
                return;

            var items = NewPantryText
                .Split(',')
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i));

            foreach (var item in items)
                PantryItems.Add(item);

            NewPantryText = string.Empty;
        }

        private void DeletePantry()
        {
            PantryItems.Clear();
        }
        private void DeletePlannedMeal()
        {
            if (SelectedPlannedMeal != null)
            {
                SelectedPlannedMeal.PropertyChanged -= PlannedMeal_PropertyChanged;
                PlannedMeals.Remove(SelectedPlannedMeal);
            }
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

                return r.Ingredients.Any(i =>
                    PantryItems.Contains(i, StringComparer.OrdinalIgnoreCase));
            };
        }
        private void ShowAllRecipes()
        {
            RecipesView.Filter = null;
        }
        private void EditPantry()
        {
            if (!PantryItems.Any())
                return;

            var editText = string.Join(", ", PantryItems);

            if (_dialogService.ShowEditPantryDialog(editText, out string? updatedText) == true
                && updatedText != null)
            {
                PantryItems.Clear();

                var items = updatedText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrWhiteSpace(i));

                foreach (var item in items)
                    PantryItems.Add(item);

                NewPantryText = string.Empty;
            }
        }

        private void SavePlannedMeals()
        {
            _plannedMealsService.Save(PlannedMeals.ToList());
        }

        private void PlannedMeal_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SavePlannedMeals();
        }

        private void EditPlannedMeal()
        {
            if (SelectedPlannedMeal == null)
                return;

            if (_dialogService.ShowPlannedMealDialog(SelectedPlannedMeal, Recipes.ToList()) == true)
            {
                OnPropertyChanged(nameof(PlannedMeals));
                OnPropertyChanged(nameof(SelectedPlannedMeal));
            }
        }

    }
}
