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
    public class PantryViewModel : BaseViewModel
    {
        private readonly IPantryService _pantryService;
        private readonly IDialogService _dialogService;

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

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand SavePantryCommand { get; }
        public ICommand DeletePantryCommand { get; }
        public ICommand EditPantryCommand { get; }

        public PantryViewModel(IPantryService pantryService, IDialogService dialogService)
        {
            _pantryService = pantryService;
            _dialogService = dialogService;

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
            if (string.IsNullOrWhiteSpace(NewPantryText)) return;

            var items = NewPantryText
                .Split(',')
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i));

            foreach (var item in items)
                PantryItems.Add(item);

            NewPantryText = string.Empty;
            ShowStatus("✓ Vorrat gespeichert");
        }
        private void DeletePantry()
        {
            if (!PantryItems.Any()) return;

            var result = MessageBox.Show(
                "Gesamten Vorrat wirklich löschen?",
                "Vorrat löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

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

        private void ShowStatus(string message)
        {
            StatusMessage = message;
            var timer = new System.Windows.Threading.DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (_, __) => { StatusMessage = ""; timer.Stop(); };
            timer.Start();
        }

    }
}
