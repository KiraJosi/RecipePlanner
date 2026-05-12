using RecipePlanner.Models;

namespace RecipePlanner.Services
{
    public interface IPantryService
    {
        List<PantryItem> GetAll();
        void Save(List<PantryItem> items);
    }
}
