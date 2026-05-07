using RecipePlanner.Models;
using RecipePlanner.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    public class PlannedMealsService : IPlannedMealsService
    {
        public readonly PlannedMealRepository _repo;

        public PlannedMealsService(PlannedMealRepository repo) 
        {
            _repo = repo;
        }

        public List<PlannedMeal> GetAll() => _repo.GetAll();

        public void Save(List<PlannedMeal> meals) => _repo.Save(meals);
    }
}
