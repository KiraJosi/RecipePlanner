using System.Collections.ObjectModel;
using RecipePlanner.ViewModels.Base;

namespace RecipePlanner.Models
{
    public class Recipe : BaseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Servings { get; set; } = 4;
        public ObservableCollection<string> Ingredients { get; set; }
            = new ObservableCollection<string>();
        public ObservableCollection<string> Steps { get; set; } 
            = new ObservableCollection<string>();
        public ObservableCollection<string> Tags { get; set; }
        = new ObservableCollection<string>();
        public string Source { get; set; } = "";

        public bool HasIngredient(string ingredientName)
        {
            if (string.IsNullOrWhiteSpace(ingredientName)) 
                return false;

            return Ingredients.Any(i =>
                i.Contains(ingredientName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
