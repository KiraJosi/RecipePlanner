using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Helpers;
using System.ComponentModel;

namespace RecipePlanner.Models
{
    public class PlannedMeal : BaseViewModel
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText)); 
                }
            }
        }

        private Recipe? _recipe;
        public Recipe? Recipe
        {
            get => _recipe;
            set
            {
                if (_recipe != value)
                {
                    _recipe = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText)); 
                }
            }
        }

        public string DisplayText => $"{Date:dd.MM.yyyy} - {Recipe?.Name}";

    }
}
