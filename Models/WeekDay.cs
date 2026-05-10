using RecipePlanner.ViewModels.Base;
using System.Collections.ObjectModel;

namespace RecipePlanner.Models
{
    public class WeekDay : BaseViewModel
    {
        public DateTime Date { get; set; }

        private ObservableCollection<PlannedMeal> _meals = [];
        public ObservableCollection<PlannedMeal> Meals
        {
            get => _meals;
            set { _meals = value; OnPropertyChanged(); }
        }
    }
}
