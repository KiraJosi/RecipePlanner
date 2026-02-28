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
        private readonly SQLiteDataService _dataService;
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
            _dataService = new SQLiteDataService();
            _dialogService = new DialogService();

            Recipes = new ObservableCollection<Recipe>(
                _dataService.GetRecipes()
            );
            RecipesView = CollectionViewSource.GetDefaultView(Recipes);

            PantryItems = new ObservableCollection<string>(
                _dataService.Load<List<string>>("pantry.json") ?? new List<string>()
            );

            var loadedPlannedMeals = _dataService.Load<List<PlannedMeal>>("plannedMeals.json") ?? new List<PlannedMeal>();
            PlannedMeals = new ObservableCollection<PlannedMeal>(loadedPlannedMeals);

            PlannedMeals.CollectionChanged += (_, __) => SavePlannedMeals();

            foreach (var meal in PlannedMeals)
                meal.PropertyChanged += PlannedMeal_PropertyChanged;

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
                _dataService.AddRecipe(recipe);
                Recipes.Add(recipe);
                SelectedRecipe = recipe;
            }
        }

        private void DeleteRecipe()
        {
            if (SelectedRecipe != null)
            {
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
                newMeal.PropertyChanged += PlannedMeal_PropertyChanged;
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
                PlannedMeals.Remove(SelectedPlannedMeal);
            }
        }
        private void EditRecipe()
        {
            if (SelectedRecipe == null)
                return;

            if (_dialogService.ShowEditRecipeDialog(SelectedRecipe) == true)
            {
                OnPropertyChanged(nameof(Recipes));
                OnPropertyChanged(nameof(SelectedRecipe));
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
