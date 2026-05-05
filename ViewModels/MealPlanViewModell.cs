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
    public  class MealPlanViewModell : BaseViewModel
    {
        private readonly IPlannedMealsService _plannedMealsService;
        private readonly IDialogService _dialogService;
        private readonly IRecipeService _recipeService;
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

        private List<DateTime> _currentWeek;
        public List<DateTime> CurrentWeek
        {
            get => _currentWeek;
            set
            {
                _currentWeek = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeletePlannedMealCommand { get; }
        public ICommand EditPlannedMealCommand { get; }
        public ICollectionView PlannedMealsView { get; }
        public IEnumerable<PlannedMeal> GetMealsForDay(DateTime day)
        {
            return PlannedMeals.Where(m => m.Date.Date == day);
        }

        public MealPlanViewModell(IPlannedMealsService plannedMealsService, IDialogService dialogService)
        {
            _plannedMealsService = plannedMealsService;
            _dialogService = dialogService;
            _recipeService = recipeService;

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

            DeletePlannedMealCommand = new RelayCommand(DeletePlannedMeal, () => SelectedPlannedMeal != null);
            EditPlannedMealCommand = new RelayCommand(EditPlannedMeal, () => SelectedPlannedMeal != null);
            GenerateWeek(DateTime.Today.StartOfWeek(DayOfWeek.Monday));
        }
        private void PlannedMeal_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SavePlannedMeals();
        }
        public void GenerateWeek(DateTime start)
        {
            CurrentWeek = Enumerable.Range(0, 7)
                .Select(i => start.Date.AddDays(i))
                .ToList();
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
        public void PlanRecipeFromDrop(Recipe recipe)
        {
            var newMeal = new PlannedMeal
            {
                Recipe = recipe,
                Date = DateTime.Today
            };

            if (_dialogService.ShowPlannedMealDialog(newMeal, Recipes.ToList()) == true)
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

            if (_dialogService.ShowPlannedMealDialog(SelectedPlannedMeal, Recipes.ToList()) == true)
            {
                PlannedMealsView.Refresh();
            }
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

