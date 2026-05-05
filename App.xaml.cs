using RecipePlanner.Services;
using RecipePlanner.ViewModels;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization.DataContracts;
using System.Windows;

namespace RecipePlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var db = new SQLiteDataService();
            var vm = new MainViewModel(
                new RecipeService(db),
                new PantryService(db),
                new PlannedMealsService(db),
                new DialogService()
                );

            var window = new MainWindow(vm);
            window.Show();
        }
    }

}
