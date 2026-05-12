using RecipePlanner.Helpers;
using RecipePlanner.Models;
using RecipePlanner.Services;
using RecipePlanner.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;

namespace RecipePlanner.ViewModels
{
    public class PantryViewModel : BaseViewModel
    {
        private readonly IPantryService _pantryService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<PantryItem> PantryItems { get; }

        private string? _newPantryName;
        public string? NewPantryName
        {
            get => _newPantryName;
            set
            {
                _newPantryName = value;
                OnPropertyChanged();

                (SavePantryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string? _newPantryAmount;
        public string? NewPantryAmount
        {
            get => _newPantryAmount;
            set { _newPantryAmount = value; OnPropertyChanged(); }
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
        public ICommand DeletePantryItemCommand { get; }

        public PantryViewModel(IPantryService pantryService, IDialogService dialogService)
        {
            _pantryService = pantryService;
            _dialogService = dialogService;

            PantryItems = new ObservableCollection<PantryItem>(_pantryService.GetAll());

            PantryItems.CollectionChanged += (_, __) =>
            {
                _pantryService.Save(PantryItems.ToList());
                (DeletePantryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };

            SavePantryCommand = new RelayCommand(SavePantry, CanSavePantry);
            DeletePantryCommand = new RelayCommand(DeletePantry, () => PantryItems.Any());
            EditPantryCommand = new RelayCommand(EditPantry);
            DeletePantryItemCommand = new RelayCommand<PantryItem>(DeletePantryItem);
        }
        private bool CanSavePantry() => !string.IsNullOrWhiteSpace(NewPantryName);
        private void SavePantry()
        {
            if (string.IsNullOrWhiteSpace(NewPantryName)) return;

            if (!string.IsNullOrWhiteSpace(NewPantryAmount))
            {
                PantryItems.Add(new PantryItem()
                {
                    Name = NewPantryName.Trim(),
                    Amount = NewPantryAmount.Trim()
                });
            }
            else
            {
                var names = NewPantryName
                    .Split(',')
                    .Select(n => n.Trim())
                    .Where(n => !string.IsNullOrWhiteSpace(n));

                foreach (var name in names)
                    PantryItems.Add(new PantryItem {  Name = name });
            }

            NewPantryName = string.Empty;
            _newPantryAmount = string.Empty;
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
            if (!PantryItems.Any()) return;

            var editText = string.Join(", ", PantryItems.Select(p => p.DisplayText));

            if (_dialogService.ShowEditPantryDialog(editText, out string? updatedText) == true
                && updatedText != null)
            {
                PantryItems.Clear();

                var parts = updatedText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p));

                foreach (var part in parts)
                    PantryItems.Add(ParsePantryItem(part));

                NewPantryName = string.Empty;
                NewPantryAmount = string.Empty;
            }
        }

        private static PantryItem ParsePantryItem(string text)
        {
            var match = Regex.Match(text.Trim(),
                @"^(\d[\d.,]*\s*[a-zA-Z]*)\s+(.*)$");

            if (match.Success)
                return new PantryItem
                {
                    Amount = match.Groups[1].Value.Trim(),
                    Name = match.Groups[2].Value.Trim()
                };

            return new PantryItem { Name = text.Trim() };
        }

        private void ShowStatus(string message)
        {
            StatusMessage = message;
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (_, __) => { StatusMessage = ""; timer.Stop(); };
            timer.Start();
        }

        private void DeletePantryItem(PantryItem item)
        {
            PantryItems.Remove(item);
        }
    }
}
