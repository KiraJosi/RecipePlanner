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
    public  class MealPlanViewModel : BaseViewModel
    {
        private readonly IPlannedMealsService _plannedMealsService;
        private readonly IDialogService _dialogService;
        private readonly RecipesViewModel _recipes;
        public ObservableCollection<PlannedMeal> PlannedMeals { get; }

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

        private List<DateTime> _currentWeek = [];
        public List<DateTime> CurrentWeek
        {
            get => _currentWeek;
            set
            {
                _currentWeek = value;
                OnPropertyChanged();
            }
        }

        private DateTime _weekStart;

        public ICommand DeletePlannedMealCommand { get; }
        public ICommand EditPlannedMealCommand { get; }
        public ICollectionView PlannedMealsView { get; }
        public ICommand PlanRecipeCommand { get; }
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand GenerateShoppingListCommand { get; }

        public MealPlanViewModel(IPlannedMealsService plannedMealsService, IDialogService dialogService, RecipesViewModel recipes)
        {
            _plannedMealsService = plannedMealsService;
            _dialogService = dialogService;
            _recipes = recipes;

            PlannedMeals = new ObservableCollection<PlannedMeal>(_plannedMealsService.GetAll());

            PlannedMealsView = CollectionViewSource.GetDefaultView(PlannedMeals);
            PlannedMealsView.GroupDescriptions.Clear();
            PlannedMealsView.SortDescriptions.Clear();
            PlannedMealsView.GroupDescriptions.Add(
                new PropertyGroupDescription(nameof(PlannedMeal.DateOnly))
            );
            PlannedMealsView.SortDescriptions.Add(
                new SortDescription(nameof(PlannedMeal.Date), ListSortDirection.Ascending)
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

            PlanRecipeCommand = new RelayCommand(PlanRecipe, CanPlanRecipe);
            DeletePlannedMealCommand = new RelayCommand(DeletePlannedMeal, () => SelectedPlannedMeal != null);
            EditPlannedMealCommand = new RelayCommand(EditPlannedMeal, () => SelectedPlannedMeal != null);
            PreviousWeekCommand = new RelayCommand(() => GenerateWeek(_weekStart.AddDays(-7)));
            NextWeekCommand = new RelayCommand (() => GenerateWeek(_weekStart.AddDays(7)));
            GenerateShoppingListCommand = new RelayCommand(GenerateShoppingList);

            GenerateWeek(DateTime.Today.StartOfWeek(DayOfWeek.Monday));
        }
        private void PlannedMeal_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SavePlannedMeals();
        }
        public void GenerateWeek(DateTime start)
        {
            _weekStart = start.Date;
            CurrentWeek = Enumerable.Range(0, 7)
                .Select(i => _weekStart.AddDays(i))
                .ToList();
        }
        private bool CanPlanRecipe()
        {
            return _recipes.SelectedRecipe != null;
        }
        private void PlanRecipe()
        {
            if (_recipes.SelectedRecipe == null)
                return;

            var newMeal = new PlannedMeal
            {
                Recipe = _recipes.SelectedRecipe,
                Date = DateTime.Today
            };

            if (_dialogService.ShowPlannedMealDialog(newMeal, _recipes.Recipes.ToList()) == true)
            {
                PlannedMeals.Add(newMeal);
            }
        }
        public void PlanRecipeFromDrop(Recipe recipe)
        {
            var newMeal = new PlannedMeal
            {
                Recipe = recipe,
                Date = DateTime.Today
            };

            if (_dialogService.ShowPlannedMealDialog(newMeal, _recipes.Recipes.ToList()) == true)
            {
                PlannedMeals.Add(newMeal);
            }
        }
        private void DeletePlannedMeal()
        {
            if (SelectedPlannedMeal != null)
            {
                SelectedPlannedMeal.PropertyChanged -= PlannedMeal_PropertyChanged;
                PlannedMeals.Remove(SelectedPlannedMeal);
            }
        }
        private void SavePlannedMeals()
        {
            _plannedMealsService.Save(PlannedMeals.ToList());
        }
        private void EditPlannedMeal()
        {
            if (SelectedPlannedMeal == null)
                return;

            if (_dialogService.ShowPlannedMealDialog(SelectedPlannedMeal, _recipes.Recipes.ToList()) == true)
            {
                PlannedMealsView.Refresh();
            }
        }

        private void GenerateShoppingList()
        {
            var mealsThisWeek = PlannedMeals
                .Where(m => CurrentWeek.Contains(m.Date.Date))
                .ToList();

            if (!mealsThisWeek.Any())
            {
                MessageBox.Show(
                    "Für diese Woche sind keine Mahlzeiten geplant.",
                    "Einkaufsliste",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var allIngredients = mealsThisWeek
                .Where(m => m.Recipe != null)
                .SelectMany(m => m.Recipe!.Ingredients)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrEmpty(i))
                .ToList();

            var pantryItems = _recipes.GetPantryItems();

            var missing = allIngredients
                .Where(i => !pantryItems.Any(p =>
                    string.Equals(p, i, StringComparison.OrdinalIgnoreCase)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(i => i)
                .ToList();

            if (!missing.Any())
            {
                MessageBox.Show(
                    "Alle Zutaten für diese Woche sind bereits im Vorrat!",
                    "Einkaufsliste",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var list = string.Join(Environment.NewLine, missing.Select(i => $"• {i}"));
            MessageBox.Show(
                $"Fehlende Zutaten für diese Woche: \n\n{list}",
                "Einkaufsliste",
                MessageBoxButton.OK,
                MessageBoxImage.None);
        }

    }
    public static class DateTimeExtension
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}

