using RecipePlanner.Helpers;
using RecipePlanner.Models;
using RecipePlanner.Services;
using RecipePlanner.ViewModels.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RecipePlanner.ViewModels
{
    public class PantryViewModell
    {
        private readonly IPantryService _pantryService;
        public ObservableCollection<string> PantryItems { get; }

        private string? _newPantryText;
        public string? NewPantryText
        {
            get => _newPantryText;
            set
            {
                _newPantryText = value;
                OnPropertyChanged();

                (SavePantryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        public ICommand SavePantryCommand { get; }
        public ICommand DeletePantryCommand { get; }
        public ICommand EditPantryCommand { get; }

        public PantryViewModell(IPantryService pantryService)
        {
            _pantryService = pantryService;

            PantryItems = new ObservableCollection<string>(_pantryService.GetAll());

            PantryItems.CollectionChanged += (_, __) =>
            {
                _pantryService.Save(PantryItems.ToList());
                (DeletePantryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };

            SavePantryCommand = new RelayCommand(SavePantry, CanSavePantry);
            DeletePantryCommand = new RelayCommand(DeletePantry, () => PantryItems.Any());
            EditPantryCommand = new RelayCommand(EditPantry);
        }
        private bool CanSavePantry()
        {
            return !string.IsNullOrWhiteSpace(NewPantryText);
        }
        private void SavePantry()
        {
            if (string.IsNullOrWhiteSpace(NewPantryText))
                return;

            var items = NewPantryText
                .Split(',')
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i));

            foreach (var item in items)
                PantryItems.Add(item);

            NewPantryText = string.Empty;
        }
        private void DeletePantry()
        {
            PantryItems.Clear();
        }
        private void EditPantry()
        {
            if (!PantryItems.Any())
                return;

            var editText = string.Join(", ", PantryItems);

            if (_dialogService.ShowEditPantryDialog(editText, out string? updatedText) == true
                && updatedText != null)
            {
                PantryItems.Clear();

                var items = updatedText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrWhiteSpace(i));

                foreach (var item in items)
                    PantryItems.Add(item);

                NewPantryText = string.Empty;
            }
        }

    }
}
