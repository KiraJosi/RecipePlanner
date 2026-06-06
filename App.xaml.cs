using RecipePlanner.Services;
using RecipePlanner.Services.Data;
using RecipePlanner.ViewModels;
using System.Windows;

namespace RecipePlanner
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var factory = new SQLiteConnectionFactory();

            var vm = new MainViewModel(
                new RecipeService(new RecipeRepository(factory)),
                new PantryService(new PantryRepository(factory)),
                new PlannedMealsService(new PlannedMealRepository(factory)),
                new DialogService(),
                new ExportImportService()
            );

            var window = new MainWindow(vm);
            window.Show();
        }
    }

}
