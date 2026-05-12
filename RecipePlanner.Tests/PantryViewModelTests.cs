using Moq;
using RecipePlanner.Models;
using RecipePlanner.Services;
using RecipePlanner.ViewModels;

namespace RecipePlanner.Tests
{
    public class PantryViewModelTests
    {
        private readonly Mock<IPantryService> _pantryService = new();
        private readonly Mock<IDialogService> _dialogService = new();

        private PantryViewModel CreateVm(List<PantryItem>? items = null)
        {
            _pantryService.Setup(s => s.GetAll()).Returns(items ?? []);
            _pantryService.Setup(s => s.Save(It.IsAny<List<PantryItem>>()));
            return new PantryViewModel(_pantryService.Object, _dialogService.Object);
        }

        [Fact]
        public void SavePantry_AddsItems_WhenTextIsValid()
        {
            var vm = CreateVm();
            vm.NewPantryName = "Mehl, Zucker, Salz";
            vm.SavePantryCommand.Execute(null);
            Assert.Equal(3, vm.PantryItems.Count);
        }

        [Fact]
        public void SavePantry_CannotExecute_WhenTextIsEmpty()
        {
            var vm = CreateVm();
            vm.NewPantryName = "";
            Assert.False(vm.SavePantryCommand.CanExecute(null));
        }

        [Fact]
        public void SavePantry_ClearsInputField_AfterSaving()
        {
            var vm = CreateVm();
            vm.NewPantryName = "Mehl";
            vm.SavePantryCommand.Execute(null);
            Assert.True(string.IsNullOrEmpty(vm.NewPantryName));
        }

        [Fact]
        public void DeletePantry_CannotExecute_WhenEmpty()
        {
            var vm = CreateVm();
            Assert.False(vm.DeletePantryCommand.CanExecute(null));
        }

        [Fact]
        public void DeletePantryItem_RemovesSingleItem()
        {
            var items = new List<PantryItem>()
            {
                new() { Name = "Mehl" },
                new() { Name = "Zucker" },
                new() { Name = "Salz" }
            };
            var vm = CreateVm(items);
            vm.DeletePantryItemCommand.Execute(vm.PantryItems.First(p => p.Name == "Zucker"));
            Assert.Equal(2, vm.PantryItems.Count);
            Assert.DoesNotContain(vm.PantryItems, p => p.Name == "Zucker");
        }

        [Fact]
        public void SavePantry_WithAmountAndName_AddsOneItemWithAmount()
        {
            var vm = CreateVm();
            vm.NewPantryAmount = "500g";
            vm.NewPantryName = "Mehl";
            vm.SavePantryCommand.Execute(null);
            Assert.Single(vm.PantryItems);
            Assert.Equal("500g", vm.PantryItems[0].Amount);
            Assert.Equal("Mehl", vm.PantryItems[0].Name);
        }
    }
}
