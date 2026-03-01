using RecipePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    public class PlannedMealsService : IPlannedMealsService
    {
        public readonly SQLiteDataService _data;

        public PlannedMealsService(SQLiteDataService data) 
        { 
            _data = data;
        }

        public List<PlannedMeal> GetAll() =>
            _data.GetPlannedMeals();

        public void Save(List<PlannedMeal> meals)
        {
            _data.SavePlannedMeals(meals);
        }
    }
}
