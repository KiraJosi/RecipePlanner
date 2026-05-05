using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    public interface IPantryService
    {
        List<string> GetAll();
        void Save(List<string> items);
    }
}
