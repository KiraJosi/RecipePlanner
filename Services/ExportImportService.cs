using RecipePlanner.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace RecipePlanner.Services
{
    public class ExportImportService : IExportImportService
    {
        private readonly string _databasePath;

        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public ExportImportService(string databasePath = "recipes.db")
        {
            _databasePath = databasePath;
        }

        public string ExportRecipesToJson(List<Recipe> recipes)
        {
            var export = new RecipeExportFile()
            {
                Version = 1,
                ExportedAt = DateTime.Now,
                Recipes = recipes.Select(r => new RecipeDto()
                {
                    Name = r.Name,
                    Source = r.Source,
                    Servings = r.Servings,
                    Ingredients = r.Ingredients.ToList(),
                    Steps = r.Steps.ToList(),
                    Tags = r.Tags.ToList()
                }).ToList()
            };

            return JsonSerializer.Serialize(export, _options);
        }

        public List<Recipe> ImportRecipesFromJson(string json)
        {
            var file = JsonSerializer.Deserialize<RecipeExportFile>(json, _options)
                ?? throw new InvalidDataException("Ungültiges Dateiformat.");

            return file.Recipes.Select(dto => new Recipe()
            {
                Name = dto.Name,
                Source = dto.Source,
                Servings = dto.Servings > 0 ? dto.Servings : 4,
                Ingredients = new ObservableCollection<string>(dto.Ingredients),
                Steps = new ObservableCollection<string>(dto.Steps),
                Tags = new ObservableCollection<string>(dto.Tags)
            }).ToList();
        }

        public void BackupDatabase(string destinationPath)
        {
            if (!File.Exists(_databasePath))
                throw new FileNotFoundException("Datenbankdatei nicht gefunden.", _databasePath);

            File.Copy(_databasePath, destinationPath, overwrite: true);
        }
    }

    internal class RecipeExportFile
    {
        public int Version { get; set; }
        public DateTime ExportedAt { get; set; }
        public List<RecipeDto> Recipes { get; set; } = new List<RecipeDto>();
    }

    internal class RecipeDto
    {
        public string Name { get; set; } = "";
        public string Source { get; set; } = "";
        public int Servings { get; set; }
        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Steps { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
    }
}
