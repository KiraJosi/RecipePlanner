using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using RecipePlanner.Models;
using System.Net;

namespace RecipePlanner.Views
{
    public partial class AddRecipeWindow : Window
    {
        private readonly Recipe? _recipeToEdit;

        public Recipe? NewRecipe { get; private set; }

        public AddRecipeWindow()
        {
            InitializeComponent();
            Title = "Neues Rezept hinzufügen";
        }

        public AddRecipeWindow(Recipe recipeToEdit)
        {
            InitializeComponent();

            Title = "Rezept bearbeiten";
            _recipeToEdit = recipeToEdit;

            NameTextBox.Text = recipeToEdit.Name;
            IngredientsTextBox.Text = string.Join(Environment.NewLine, recipeToEdit.Ingredients);
            StepsTextBox.Text = string.Join(Environment.NewLine, recipeToEdit.Steps);
            SourceTextBox.Text = recipeToEdit.Source;
            ServingsTextBox.Text = recipeToEdit.Servings.ToString();
            TagsTextBox.Text = string.Join(", ", recipeToEdit.Tags);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Bitte einen Rezeptnamen eingeben!");
                return;
            }

            var ingredients = new ObservableCollection<string>(
                IngredientsTextBox.Text
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                );
                

            var steps = new ObservableCollection<string> (
                StepsTextBox.Text
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
            );

            var tags = new ObservableCollection<string>(
                TagsTextBox.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                );

            int servings = int.TryParse(ServingsTextBox.Text, out int s) && s > 0 ? s : 4;

            if (_recipeToEdit != null)
            {
                _recipeToEdit.Name = NameTextBox.Text;
                _recipeToEdit.Ingredients = ingredients;
                _recipeToEdit.Steps = steps;
                _recipeToEdit.Source = SourceTextBox.Text;
                _recipeToEdit.Servings = servings;
                _recipeToEdit.Tags = tags;
            }
            else
            {
                NewRecipe = new Recipe
                {
                    Name = NameTextBox.Text,
                    Ingredients = ingredients,
                    Steps = steps,
                    Source = SourceTextBox.Text,
                    Tags = tags,
                    Servings = servings
                };
            }

            DialogResult = true;
            Close();
        }

        private async void ImportFromUrlButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Rezept-URL eingeben:", "", "Rezept importieren");
            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.InputText))
                return;

            ImportFromUrlButton.IsEnabled = false;
            ImportFromUrlButton.Content = "⏳ Wird geladen...";

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (compatible; RecipePlanner/1.0)");

                var html = await client.GetStringAsync(dialog.InputText.Trim());
                var imported = ParseRecipeFromHtml(html);

                if (imported == null)
                {
                    MessageBox.Show(
                        "Kein Rezept auf dieser Seite gefunden. \n" +
                        "Die Seite unterstützt möglicherweise kein strukturiertes Rezeptformat.",
                        "Import fehlgeschlagen",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrEmpty(imported.Name))
                    NameTextBox.Text = imported.Name;
                if (imported.Ingredients.Any())
                    IngredientsTextBox.Text = string.Join(Environment.NewLine, imported.Ingredients);
                if (imported.Steps.Any())
                    StepsTextBox.Text = string.Join(Environment.NewLine, imported.Steps);
                if (!string.IsNullOrEmpty(imported.Source))
                    SourceTextBox.Text = imported.Source;
                if (imported.Servings > 0)
                    ServingsTextBox.Text = imported.Servings.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Importieren:\n{ex.Message}",
                    "Import fehlgeschlagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                ImportFromUrlButton.IsEnabled = true;
                ImportFromUrlButton.Content = "🌐 Aus URL importieren";
            }
        }

        private Recipe? ParseRecipeFromHtml(string html)
        {
            var pattern = new Regex(
                @"<script[^>]*type=[""']application/ld\+json[""'][^>]*>(.+?)</script>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match m in pattern.Matches(html))
            {
                try
                {
                    var doc = JsonDocument.Parse(m.Groups[1].Value.Trim());
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in root.EnumerateArray())
                            if (IsRecipeType(item))
                                return ExtractRecipe(item);
                    }
                    else if (IsRecipeType(root))
                    {
                        return ExtractRecipe(root);
                    }
                }
                catch { }
            }
            return null;
        }

        private static bool IsRecipeType(JsonElement el)
        {
            if (!el.TryGetProperty("@type", out var type)) return false;

            if (type.ValueKind == JsonValueKind.String)
                return type.GetString()?.Equals("Recipe", StringComparison.OrdinalIgnoreCase) == true;

            if (type.ValueKind == JsonValueKind.Array)
                return type.EnumerateArray()
                    .Any(t => t.GetString()?.Equals("Recipe", StringComparison.OrdinalIgnoreCase) == true);

            return false;
        }

        private static Recipe ExtractRecipe(JsonElement el)
        {
            var recipe = new Recipe();

            if (el.TryGetProperty("name", out var name))
                recipe.Name = WebUtility.HtmlDecode(name.GetString() ?? "");

            if (el.TryGetProperty("recipeIngredient", out var ings))
                foreach (var ing in ings.EnumerateArray())
                {
                    var s = ing.GetString()?.Trim();
                    if (!string.IsNullOrEmpty(s))
                        recipe.Ingredients.Add(WebUtility.HtmlDecode(s));
                }

            if (el.TryGetProperty("recipeInstructions", out var steps))
            {
                if (steps.ValueKind == JsonValueKind.Array)
                    foreach (var step in steps.EnumerateArray())
                    {
                        string? text = step.ValueKind == JsonValueKind.String
                            ? step.GetString()
                            : step.TryGetProperty("text", out var t) ? t.GetString() : null;

                        if (!string.IsNullOrEmpty(text))
                            recipe.Steps.Add(WebUtility.HtmlEncode(text.Trim()));
                    }
                else if (steps.ValueKind == JsonValueKind.String)
                {
                    var text = steps.GetString()?.Trim();
                    if (!string.IsNullOrEmpty(text))
                        recipe.Steps.Add(text);
                }
            }

            if (el.TryGetProperty("recipeYield", out var yield))
            {
                var yieldStr = yield.ValueKind == JsonValueKind.String
                    ? yield.GetString()
                    : yield.ValueKind == JsonValueKind.Array
                        ?yield.EnumerateArray().FirstOrDefault().GetString()
                        : null;

                if (yieldStr != null)
                {
                    var numMatch = Regex.Match(yieldStr, @"\d+");
                    if (numMatch.Success && int.TryParse(numMatch.Value, out int servings))
                        recipe.Servings = servings;
                }
            }

            return recipe;
        }
    }
}
