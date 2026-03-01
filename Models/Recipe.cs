using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Helpers;

namespace RecipePlanner.Models
{
    public class Recipe : BaseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<string> Ingredients { get; set; }
            = new ObservableCollection<string>();
        public ObservableCollection<string> Steps { get; set; } 
            = new ObservableCollection<string>();
        public string Source { get; set; } = "";

        public bool HasIngredient(string ingredient)
        {
            if (string.IsNullOrWhiteSpace(ingredient)) 
                return false;

            return Ingredients.Any(i =>
                string.Equals(i, ingredient, StringComparison.OrdinalIgnoreCase));
        }
    }
}
