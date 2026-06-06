using RecipePlanner.Helpers;
using RecipePlanner.Services;
using RecipePlanner.ViewModels.Base;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace RecipePlanner.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IExportImportService _exportImportService;
        private readonly IDialogService _dialogService;

        public RecipesViewModel Recipes {  get; }
        public PantryViewModel Pantry { get; }
        public MealPlanViewModel MealPlan { get; }

        public ICommand ExportRecipesCommand { get; }
        public ICommand ImportRecipesCommand { get; }
        public ICommand BackupDatabaseCommand { get; }

        public MainViewModel(
            IRecipeService recipeService,
            IPantryService pantryService,
            IPlannedMealsService plannedMealsService,
            IDialogService dialogService,
            IExportImportService exportImportService)
        {
            _dialogService = dialogService;
            _exportImportService = exportImportService;

            Pantry = new PantryViewModel(pantryService, dialogService);
            Recipes = new RecipesViewModel(recipeService, dialogService, Pantry);
            MealPlan = new MealPlanViewModel(plannedMealsService, dialogService, Recipes);
            Recipes.SetMealPlan(MealPlan);

            ExportRecipesCommand = new RelayCommand(ExportRecipes);
            ImportRecipesCommand = new RelayCommand(ImportRecipes);
            BackupDatabaseCommand = new RelayCommand(BackupDatabase);
        }

        private void ExportRecipes()
        {
            var path = _dialogService.ShowSaveFileDialog(
                "JSON-Dateien (*.json)|*.json",
                $"rezepte_export_{DateTime.Today:yyyy-MM-dd}.json");

            if (path == null) return;

            try
            {
                var json = _exportImportService.ExportRecipesToJson(Recipes.Recipes.ToList());
                File.WriteAllText(path, json, System.Text.Encoding.UTF8);

                MessageBox.Show(
                    $"✓ {Recipes.Recipes.Count} Rezepte erfolgreich exportiert.",
                    "Export erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Exportieren:\n{ex.Message}",
                    "Export fehlgeschlagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ImportRecipes()
        {
            var path = _dialogService.ShowOpenFileDialog("JSON-Dateien (*.json)|*.json");
            if (path == null) return;

            try
            {
                var json = File.ReadAllText(path, System.Text.Encoding.UTF8);
                var importedRecipes = _exportImportService.ImportRecipesFromJson(json);
                var (added, skipped) = Recipes.ImportRecipes(importedRecipes);

                var message = skipped == 0
                    ? $"✓ {added} Rezepte erfolgreich importiert."
                    : $"✓ {added} Rezepte importiert.\n{skipped} bereits vorhanden und übersprungen.";

                MessageBox.Show(message, "Import abgeschlossen",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Importieren:\n{ex.Message}",
                    "Import fehlgeschlagen",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void BackupDatabase()
        {
            var path = _dialogService.ShowSaveFileDialog(
                "SQLite-Datenbank (*.db)|*.db",
                $"recipes_backup_{DateTime.Today:yyyy-MM-dd}.db");

            if (path == null) return;

            try
            {
                _exportImportService.BackupDatabase(path);
                MessageBox.Show(
                    "✓ Datenbank erfolgreich gesichert.",
                    "Backup erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Sichern:\n{ex.Message}",
                    "Backup fehlgeschlagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
