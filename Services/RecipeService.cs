using RecipePlanner.Models;
using RecipePlanner.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly RecipeRepository _repo;

        public RecipeService(RecipeRepository repo)
        {
            _repo = repo;
        }

        public List<Recipe> GetAll() => _repo.GetAll();
        public void Add(Recipe recipe) => _repo.Add(recipe);
        public void Update(Recipe recipe) => _repo.Update(recipe);
        public void Delete(int id) => _repo.Delete(id);
    }
}
