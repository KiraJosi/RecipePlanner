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

        private readonly ObservableCollection<string> _activeFilters = new();
        public ObservableCollection<string> ActiveFilters => _activeFilters;

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

                CurrentServings = _selectedRecipe?.Servings ?? 4;
                OnPropertyChanged(nameof(ScaledIngredients));
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
                (AddFilterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private int _currentServings = 4;
        public int CurrentServings
        {
            get => _currentServings;
            set
            {
                if (value < 1) return;
                _currentServings = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScalingFactor));
                OnPropertyChanged(nameof(ScaledIngredients));
            }
        }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public double ScalingFactor =>
            SelectedRecipe == null || SelectedRecipe.Servings == 0
            ? 1.0
            : (double)CurrentServings /SelectedRecipe.Servings;

        public IEnumerable<string> ScaledIngredients =>
            SelectedRecipe?.Ingredients
                .Select(i => IngredientScaler.Scale(i, ScalingFactor))
            ?? [];

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
        public ICommand IncrementServingsCommand { get; }
        public ICommand DecrementServingsCommand { get; }
        public ICommand AddFilterCommand { get; }
        public ICommand RemoveFilterCommand { get; }
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
            IncrementServingsCommand = new RelayCommand(() => CurrentServings++);
            DecrementServingsCommand = new RelayCommand(() => CurrentServings--);
            AddFilterCommand = new RelayCommand(AddFilter, () => !string.IsNullOrWhiteSpace(_searchText));
            RemoveFilterCommand = new RelayCommand<string>(RemoveFilter);

            _pantry.PantryItems.CollectionChanged += (_, __) =>
            {
                (FindRecipesCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };

            _mealPlanCommand?.RaiseCanExecuteChanged();
        }
        private void AddRecipe()
        {
            if (_dialogService.ShowAddRecipeDialog(out Recipe? recipe) == true && recipe != null)
            {
                _recipeService.Add(recipe);
                Recipes.Add(recipe);
                SelectedRecipe = recipe;
                ShowStatus("✓ Rezept gespeichert");
            }
        }
        private void DeleteRecipe()
        {
            if (SelectedRecipe == null) return;

            var result = MessageBox.Show(
                $"Rezept \"{SelectedRecipe.Name}\"wirklich löschen?",
                "Rezept löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

                _recipeService.Delete(SelectedRecipe.Id);
                Recipes.Remove(SelectedRecipe);
        }
        private bool CanDeleteRecipe()
        {
            return SelectedRecipe != null;
        }
        private void EditRecipe()
        {
            if (SelectedRecipe == null) return;

            if (_dialogService.ShowEditRecipeDialog(SelectedRecipe) == true)
            {
                _recipeService.Update(SelectedRecipe);
                ShowStatus("✓ Rezept aktualisiert");
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
            _activeFilters.Clear();
            SearchText = "";
        }
        private void ApplyFilter()
        {
            RecipesView.Filter = obj =>
            {
                if (obj is not Recipe r)
                    return false;

                bool matchesSearch = string.IsNullOrWhiteSpace(_searchText)
                    || r.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                    || r.Tags.Any(t => t.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                    || r.Ingredients.Any(i => i.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

                bool matchesFilters = _activeFilters.All(filter =>
                    r.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || r.Tags.Any(t => t.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    || r.Ingredients.Any(i => i.Contains(filter, StringComparison.OrdinalIgnoreCase)));

                bool matchesPantry = !_pantryFilterActive
                    || (r.Ingredients.Any() && r.Ingredients.All(i =>
                        _pantry.PantryItems.Any(p =>
                            i.Contains(p.Name, StringComparison.OrdinalIgnoreCase))));
                        

                return matchesSearch && matchesFilters && matchesPantry;
            };
        }

        private void ShowStatus(string message)
        {
            StatusMessage = message;
            var timer = new System.Windows.Threading.DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (_, __) => { StatusMessage = ""; timer.Stop(); };
            timer.Start();
        }

        private void AddFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            if (!_activeFilters.Any(f => f.Equals(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)))
                _activeFilters.Add(SearchText.Trim());

            SearchText = "";
        }

        private void RemoveFilter(string filter)
        {
            _activeFilters.Remove(filter);
            ApplyFilter();
        }

        public IEnumerable<string> GetPantryItems() => _pantry.PantryItems.Select(p => p.Name);
    }
}
