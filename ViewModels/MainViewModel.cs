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
        private readonly JsonDataService _dataService;

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
            _dataService = new JsonDataService();

            Recipes = new ObservableCollection<Recipe>(
                _dataService.Load<List<Recipe>>("recipes.json") ?? new List<Recipe>()
            );
            RecipesView = CollectionViewSource.GetDefaultView(Recipes);

            PantryItems = new ObservableCollection<string>(
                _dataService.Load<List<string>>("pantry.json") ?? new List<string>()
            );

            PlannedMeals = new ObservableCollection<PlannedMeal>(
                _dataService.Load<List<PlannedMeal>>("plannedMeals.json") ?? new List<PlannedMeal>()
            );

            Recipes.CollectionChanged += (_, __) =>
            {
                _dataService.Save("recipes.json", Recipes);
            };

            PlannedMeals.CollectionChanged += (_, __) =>
            {
                _dataService.Save("plannedMeals.json", PlannedMeals);
            };

            PantryItems.CollectionChanged += (_, __) =>
            {
                _dataService.Save("pantry.json", PantryItems);
                (DeletePantryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };

            AddRecipeCommand = new RelayCommand(AddRecipe);
            DeleteRecipeCommand = new RelayCommand(DeleteRecipe, CanDeleteRecipe);
            PlanRecipeCommand = new RelayCommand(PlanRecipe, CanPlanRecipe);
            SavePantryCommand = new RelayCommand(SavePantry);
            DeletePantryCommand = new RelayCommand(DeletePantry, () => PantryItems.Any());
            DeletePlannedMealCommand = new RelayCommand(DeletePlannedMeal, () => SelectedPlannedMeal != null);
            EditRecipeCommand = new RelayCommand(EditRecipe, () => SelectedRecipe != null);
            FindRecipesCommand = new RelayCommand(FindRecipes);
            ShowAllRecipesCommand = new RelayCommand(ShowAllRecipes);
            EditPantryCommand = new RelayCommand(EditPantry);
            EditPlannedMealCommand = new RelayCommand(EditPlannedMeal, () => SelectedPlannedMeal != null);

        }

        private void AddRecipe()
        {
            var addWindow = new AddRecipeWindow();
            bool? result = addWindow.ShowDialog();

            if (result == true && addWindow.NewRecipe != null)
            {
                Recipes.Add(addWindow.NewRecipe);

                SelectedRecipe = addWindow.NewRecipe;
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

        private void PlanRecipe()
        {
            if (SelectedRecipe == null)
                return;

            var newMeal = new PlannedMeal { Recipe = SelectedRecipe, Date = DateTime.Today };
            var addWindow = new PlannedMealWindow(newMeal, Recipes.ToList());
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                PlannedMeals.Add(newMeal);
            }
        }

        private bool CanPlanRecipe()
        {
            return SelectedRecipe != null;
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

            // Neues Fenster mit bestehendem Rezept öffnen
            var editWindow = new AddRecipeWindow(SelectedRecipe);

            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                // SelectedRecipe wurde direkt im Dialog geändert
                // CollectionChanged Event sorgt dafür, dass JSON gespeichert wird
                // Optional: PropertyChanged feuern, damit UI aktualisiert wird
                OnPropertyChanged(nameof(Recipes));
                OnPropertyChanged(nameof(SelectedRecipe));
            }
        }
        private void FindRecipes()
        {
            if (PantryItems.Count == 0)
            {
                MessageBox.Show("Keine Vorratszutaten vorhanden!");
                return;
            }

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

            // Dialog oder Input-Box für Bearbeitung
            var editText = string.Join(", ", PantryItems);

            // Beispiel: InputDialog öffnen (kann selbst erstellt werden)
            var inputDialog = new InputDialog("Vorrat bearbeiten:", editText);
            bool? result = inputDialog.ShowDialog();

            if (result == true)
            {
                PantryItems.Clear();
                var items = inputDialog.InputText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrWhiteSpace(i));

                foreach (var item in items)
                    PantryItems.Add(item);

                NewPantryText = string.Empty;
            }
        }

        private void EditPlannedMeal()
        {
            if (SelectedPlannedMeal == null)
                return;

            // Dialog mit bestehendem Meal öffnen
            var editWindow = new PlannedMealWindow(SelectedPlannedMeal, Recipes.ToList());
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                // PlannedMeal ist direkt aktualisiert
                OnPropertyChanged(nameof(PlannedMeals));
                OnPropertyChanged(nameof(SelectedPlannedMeal));
            }
        }

    }
}
