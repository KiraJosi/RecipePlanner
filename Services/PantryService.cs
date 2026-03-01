using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    public class PantryService : IPantryService
    {
        public readonly SQLiteDataService _data;

        public PantryService(SQLiteDataService data)
        { 
            _data = data;
        }

        public List<string> GetAll() =>
            _data.GetPantryItems();

        public void Save(List<string> items) 
        { 
            _data.SavePantryItems(items);
        }
    }
}
