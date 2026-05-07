using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Services.Data;

namespace RecipePlanner.Services
{
    public class PantryService : IPantryService
    {
        private readonly PantryRepository _repo;

        public PantryService(PantryRepository repo)
        {
            _repo = repo;
        }

        public List<string> GetAll() => _repo.GetAll();

        public void Save(List<string> items) => _repo.Save(items);
        }
    }
}
