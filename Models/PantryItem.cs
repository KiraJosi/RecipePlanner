using RecipePlanner.ViewModels.Base;

namespace RecipePlanner.Models
{
    public class PantryItem : BaseViewModel
    {
        public int Id { get; set; }

        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        private string _amount = "";
        public string Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
        }

        public string DisplayText => string.IsNullOrWhiteSpace(Amount)
            ? Name
            : $"{Amount} {Name}";
    }
}
