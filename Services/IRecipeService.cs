using RecipePlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    internal interface IRecipeService
    {
        List<Recipe> GetAll();
        void Add (Recipe recipe);
        void Update (Recipe recipe);
        void Delete (int  id);
    }
}
