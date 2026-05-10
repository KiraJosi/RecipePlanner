using Moq;
using RecipePlanner.Services;
using RecipePlanner.ViewModels;

namespace RecipePlanner.Tests
{
    public class PantryViewModelTests
    {
        private readonly Mock<IPantryService> _pantryService = new();
        private readonly Mock<IDialogService> _dialogService = new();

        private PantryViewModel CreateVm(List<string>? items = null)
        {
            _pantryService.Setup(s => s.GetAll()).Returns(items ?? []);
            _pantryService.Setup(s => s.Save(It.IsAny<List<string>>()));
            return new PantryViewModel(_pantryService.Object, _dialogService.Object);
        }

        [Fact]
        public void SavePantry_AddsItems_WhenTextIsValid()
        {
            var vm = CreateVm();
            vm.NewPantryText = "Mehl, Zucker, Salz";
            vm.SavePantryCommand.Execute(null);
            Assert.Equal(3, vm.PantryItems.Count);
        }

        [Fact]
        public void SavePantry_CannotExecute_WhenTextIsEmpty()
        {
            var vm = CreateVm();
            vm.NewPantryText = "";
            Assert.False(vm.SavePantryCommand.CanExecute(null));
        }

        [Fact]
        public void SavePantry_ClearsInputField_AfterSaving()
        {
            var vm = CreateVm();
            vm.NewPantryText = "Mehl";
            vm.SavePantryCommand.Execute(null);
            Assert.True(string,IsNullOrEmpty(vm.NewPantryText));
        }

        [Fact]
        public void DeletePantry_CannotExecute_WhenEmpty()
        {
            var vm = CreateVm();
            Assert.False(vm.DeletePantryCommand.CanExecute(null));
        }

    }
}
