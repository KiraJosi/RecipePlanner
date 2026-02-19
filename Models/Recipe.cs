using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Models
{
    public class Recipe
    {
        public string Name { get; set; } = "";
        public List<string> Ingredients { get; set; } = new();
        public List<string> Steps { get; set; } = new();
        public string Source { get; set; } = "";
    }
}
