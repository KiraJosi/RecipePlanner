using RecipePlanner.Models;

namespace RecipePlanner.Services
{
    public interface IExportImportService
    {
        string ExportRecipesToJson(List<Recipe> recipes);
        List<Recipe> ImportRecipesFromJson(string json);
        void BackupDatabase(string destinationPath);
    }
}
