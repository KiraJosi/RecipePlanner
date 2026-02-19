using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Helpers;
using System.ComponentModel;

namespace RecipePlanner.Models
{
    public class PlannedMeal : INotifyPropertyChanged
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
                    OnPropertyChanged(nameof(Date));
                    OnPropertyChanged(nameof(DisplayText)); // UI aktualisieren
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
                    OnPropertyChanged(nameof(Recipe));
                    OnPropertyChanged(nameof(DisplayText)); // UI aktualisieren
                }
            }
        }

        public string DisplayText => $"{Date:dd.MM.yyyy} - {Recipe?.Name}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
