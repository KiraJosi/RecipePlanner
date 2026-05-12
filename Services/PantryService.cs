using RecipePlanner.Models;
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

        public List<PantryItem> GetAll() => _repo.GetAll();

        public void Save(List<PantryItem> items) => _repo.Save(items);
        
    }
}
