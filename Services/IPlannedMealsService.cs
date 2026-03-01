using RecipePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    internal interface IPlannedMealsService
    {
        List<PlannedMeal> GetAll();
        void Save(List<PlannedMeal> meals);
    }
}
