using Moq;
using RecipePlanner.Models;
using RecipePlanner.Services;
using RecipePlanner.ViewModels;

namespace RecipePlanner.Tests
{
    public class RecipesViewModelTests
    {
        private readonly Mock<IRecipeService> _recipeService = new();
        private readonly Mock<IDialogService> _dialogService = new();
        private readonly Mock<IPantryService> _pantryService = new();

        private RecipesViewModel CreateVm(List<Recipe>? recipes = null)
        {
            _recipeService.Setup(s => s.GetAll()).Returns(recipes ?? []);
            _pantryService.Setup(s => s.GetAll()).Returns([]);
            _pantryService.Setup(s => s.Save(It.IsAny<List<PantryItem>>()));

            var pantryVm = new PantryViewModel(_pantryService.Object, _dialogService.Object);
            return new RecipesViewModel(_recipeService.Object, _dialogService.Object, pantryVm);
        }

        [Fact]
        public void SearchText_FiltersRecipesByName()
        {
            var recipes = new List<Recipe>()
            {
                new() { Id = 1, Name = "Spaghetti" },
                new() { Id = 2, Name = "Pizza" }
            };
            var vm = CreateVm(recipes);
            vm.SearchText = "spag";

            var visible = vm.RecipesView.Cast<Recipe>().ToList();
            Assert.Single(visible);
            Assert.Equal("Spaghetti", visible[0].Name);
        }

        [Fact]
        public void SearchText_FiltersRecipesByTag()
        {
            var recipe = new Recipe { Id = 1, Name = "Bolognese" };
            recipe.Tags.Add("italienisch");

            var vm = CreateVm([recipe]);
            vm.SearchText = "italienisch";

            var visible = vm.RecipesView.Cast<Recipe>().ToList();
            Assert.Single(visible);
        }

        [Fact]
        public void IncrementServings_IncreasesCurrentServings()
        {
            var vm = CreateVm();
            var before = vm.CurrentServings;
            vm.IncrementServingsCommand.Execute(null);
            Assert.Equal(before + 1, vm.CurrentServings);
        }

        [Fact]
        public void DecrementServings_DoesNotGoBelowOne()
        {
            var vm = CreateVm();
            vm.CurrentServings = 1;
            vm.DecrementServingsCommand.Execute(null);
            Assert.Equal(1, vm.CurrentServings);
        }

        [Fact]
        public void DeleteRecipe_CannotExecute_WhenNothingSelected()
        {
            var vm = CreateVm();
            Assert.False(vm.DeleteRecipeCommand.CanExecute(null));
        }

        [Fact]
        public void SearchText_FiltersRecipesByIngredient()
        {
            var recipe = new Recipe { Id = 1, Name = "Pasta" };
            recipe.Ingredients.Add("200g Mehl");
            recipe.Ingredients.Add("3 Eier");

            var vm = CreateVm([recipe]);

            vm.SearchText = "mehl";

            var visible = vm.RecipesView.Cast<Recipe>().ToList();
            Assert.Single(visible);
            Assert.Equal("Pasta", visible[0].Name);
        }
    }
}
