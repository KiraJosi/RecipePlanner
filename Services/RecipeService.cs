using RecipePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly SQLiteDataService _data;

        public RecipeService(SQLiteDataService data)
        {
            _data = data;
        }

        public List<Recipe> GetAll() =>
            _data.GetRecipes();

        public void Add(Recipe recipe) =>
            _data.AddRecipe(recipe);

        public void Update(Recipe recipe) =>
            _data.UpdateRecipe(recipe);

        public void Delete(int id) =>
            _data.DeleteRecipe(id);
    }
}
